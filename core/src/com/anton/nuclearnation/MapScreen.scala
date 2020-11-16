package com.anton.nuclearnation

import java.lang.Math

import com.anton.nuclearnation.ControlledBy.ControlledBy
import com.anton.nuclearnation.MapScreen._
import com.badlogic.gdx.Input.{Buttons, Keys}
import com.badlogic.gdx.assets.AssetManager
import com.badlogic.gdx.assets.loaders.resolvers.InternalFileHandleResolver
import com.badlogic.gdx._
import com.badlogic.gdx.graphics.Pixmap.Format
import com.badlogic.gdx.graphics._
import com.badlogic.gdx.graphics.g2d.{BitmapFont, Sprite, TextureRegion}
import com.badlogic.gdx.graphics.g2d.freetype.{FreeTypeFontGenerator, FreeTypeFontGeneratorLoader, FreetypeFontLoader}
import com.badlogic.gdx.maps.MapLayers
import com.badlogic.gdx.maps.tiled.TiledMap
import com.badlogic.gdx.maps.tiled.TiledMapTileLayer
import com.badlogic.gdx.maps.tiled.TiledMapTileLayer.Cell
import com.badlogic.gdx.maps.tiled.tiles.StaticTiledMapTile
import com.badlogic.gdx.math.{Vector2, Vector3}

import scala.util.Random
import com.badlogic.gdx.graphics.g2d.freetype.FreetypeFontLoader.FreeTypeFontLoaderParameter
import com.badlogic.gdx.graphics.glutils.ShapeRenderer
import com.badlogic.gdx.maps.tiled.renderers.{IsometricStaggeredTiledMapRenderer, IsometricTiledMapRenderer, OrthogonalTiledMapRenderer}
import com.badlogic.gdx.scenes.scene2d.{Group, InputEvent, InputListener, Stage}
import com.badlogic.gdx.scenes.scene2d.ui.Label.LabelStyle
import com.badlogic.gdx.scenes.scene2d.ui.TextButton.TextButtonStyle
import com.badlogic.gdx.scenes.scene2d.ui.TextTooltip.TextTooltipStyle
import com.badlogic.gdx.scenes.scene2d.ui.Window.WindowStyle
import com.badlogic.gdx.scenes.scene2d.ui.{Button, Dialog, Image, Label, ScrollPane, Skin, Table, TextButton, TextTooltip, Tooltip, TooltipManager, Value}
import com.badlogic.gdx.scenes.scene2d.utils.ClickListener
import com.badlogic.gdx.utils.Timer.Task
import com.badlogic.gdx.utils.viewport.StretchViewport
import com.badlogic.gdx.utils.{Align, Timer}

import scala.collection.mutable.ListBuffer

class MapScreen(game: NuclearNation) extends Screen{

  val technologies:List[Technology] = List(Technology("Advanced tactics"),Technology("Automatic weapons"))

  val assetManager = game.assetManager

  val map = new TiledMap
  val layers = map.getLayers

  val mapWidthTiles = 30
  val mapHeightTiles = 30

  val desertTileTexture = assetManager.get("desert_tile.png",classOf[Texture])
  val ruinedBuildingTexture = assetManager.get("ruined-building.png",classOf[Texture])
  val desertLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val townLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val fogOfWarLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val desertTileCell:Cell = new Cell
  val fogOfWarCell = new Cell

  val expeditionTexture = assetManager.get("expedition.png",classOf[Texture])

  val tradeCaravanTexture = assetManager.get("tradeCaravan.png",classOf[Texture])
  val militaryCaravanTexture = assetManager.get("militaryCaravan.png",classOf[Texture])

  val region = new TextureRegion(desertTileTexture)

  val raiderCampImage = new Sprite(assetManager.get("raider_camp.png",classOf[Texture]))


  val fogOfWarTexture = assetManager.get("fog_of_war_tile.png",classOf[Texture])
  val townImage = assetManager.get("town.png",classOf[Texture])

  desertTileCell.setTile(new StaticTiledMapTile(region))
  fogOfWarCell.setTile(new StaticTiledMapTile(new TextureRegion(fogOfWarTexture)))

  val coordsGenerator = new MapCoordsGenerator(mapWidthTiles,mapHeightTiles,3)

  val mapData = new MapData(mapWidthTiles,mapHeightTiles)


  val gameFont = assetManager.get("fonts/lunchtime-doubly-so/lunchds.ttf",classOf[BitmapFont])

  val skin = assetManager.get("data/commodore64/skin/uiskin.json",classOf[Skin])

  val stage = new Stage(new StretchViewport(1600,960,new OrthographicCamera()))
  val camera = stage.getCamera.asInstanceOf[OrthographicCamera]

  val centerOnCapitalButton = new TextButton("Re-center",skin)
  val pauseButton = new TextButton("Pause",skin)

  var isPaused = false

  val music = Gdx.audio.newMusic(Gdx.files.internal("music/POL-dark-crossing-short.mp3"))

  val roads = ListBuffer[(MapCellData,MapCellData)]()

  var scrap = 0
  val scrapLabelButton = new TextButton("Scrap: ",skin)
  scrapLabelButton.setDisabled(true)

  val mapScale = 0.5f;

  val mapDebugOutputEnabled = sys.env.get("ENABLE_MAP_DEBUG_OUTPUT") match {
    case Some(v)=>v.toLowerCase().toBoolean
    case None=>false
  }

  centerOnCapitalButton.addCaptureListener(new ClickListener(){
    override def clicked (event:InputEvent, x:Float, y:Float):Unit= {
      centerScreen()
    }
  })

  pauseButton.addCaptureListener(new ClickListener(){
    override def clicked (event:InputEvent, x:Float, y:Float):Unit= {
      pauseGame()
    }
  })

  val buttonsGroup = new Group()
  val tileGroup = new Group()
  buttonsGroup.addActor(centerOnCapitalButton)
  buttonsGroup.addActor(pauseButton)
  stage.addActor(tileGroup)
  stage.addActor(buttonsGroup)

  def pauseGame(): Unit ={
    isPaused = !isPaused
  }


  val mapInputProcessor = new InputProcessor() {

    override def touchDown(screenX: Int, screenY: Int, pointer: Int, button: Int): Boolean = {
      if (button == Input.Buttons.RIGHT) {
        mapRightClicked(screenX,screenY)
        true
      } else if (button == Input.Buttons.LEFT) {
//          mapLeftClicked(screenX, screenY)
          true
      } else {
        false
      }
    }

    override def keyDown(keycode: Int): Boolean = {true}

    override def keyUp(keycode: Int): Boolean = {
      keycode match {
        case Input.Keys.C=>
          true
        case Input.Keys.SPACE=>
          centerScreen()
          true
        case Input.Keys.P=>
          pauseGame()
          true
        case _=> false
      }
    }


    override def keyTyped(character: Char): Boolean = {true}

    override def touchUp(screenX: Int, screenY: Int, pointer: Int, button: Int): Boolean = {true}

    override def touchDragged(screenX: Int, screenY: Int, pointer: Int): Boolean = {true}

    override def mouseMoved(screenX: Int, screenY: Int): Boolean = {true}

    override def scrolled(amountX: Float, amountY: Float): Boolean = {true}
  }

  val locations = ListBuffer[MapLocation]()

  val cityNames = List[String]("New Reno","Modoc","Arroyo","Den","Heaven","Nuke","Tumbleweed")

  val capitalCoords = coordsGenerator.getCoords

  val capitalCell = mapData.cells.find(cell=>cell.x == capitalCoords._1 && cell.y == capitalCoords._2).get
  val capital = CityInfo("Hope",capitalCell,100, isOwnedByPlayer = true)
  capitalCell.location = Some(capital)
  locations += capital

  cityNames.foreach(cityName=> {
    val coords = coordsGenerator.getCoords
    val cityCell = mapData.getCell(coords._1,coords._2).get
    val city = CityInfo(cityName, cityCell,100)
    cityCell.location = Some(city)
    locations += city
    Gdx.app.log("INFO",s"Generated city $cityName with population ${city.population} at coords $coords")
  })


  for (
    x <- 0 until mapWidthTiles;
    y <- 0 until mapHeightTiles
  ) yield  {
    desertLayer.setCell(x, y, desertTileCell)

    val location = mapData.getCell(x,y).get.location

    location match {
      case Some(_:CityInfo) => {
        val townRegion = new TextureRegion(townImage)
        val townTile = new StaticTiledMapTile(townRegion)
        val townCell = new Cell
        townCell.setTile(townTile)
        townLayer.setCell(x,y,townCell)
      }

      case _=>

    }
  }
  map.getLayers.add(townLayer)
  map.getLayers.add(desertLayer)

  val renderer = new OrthogonalTiledMapRenderer(map, mapScale)

  val mapWidthPixels = (desertLayer.getWidth * desertLayer.getTileWidth * mapScale).asInstanceOf[Int]
  val mapHeightPixels = (desertLayer.getHeight * desertLayer.getTileHeight * mapScale).asInstanceOf[Int]

  var cameraCenterX = 0f
  var cameraCenterY = 0f

  val roadRenderer = new ShapeRenderer()


  import com.badlogic.gdx.Gdx
  import com.badlogic.gdx.graphics.glutils.ShapeRenderer
  import com.badlogic.gdx.math.Matrix4
  import com.badlogic.gdx.math.Vector2

  def drawRoadLine(start: Vector2, end: Vector2, lineWidth: Int, color: Color = Color.BROWN, projectionMatrix: Matrix4 = camera.combined): Unit = {
    Gdx.gl.glLineWidth(lineWidth.toFloat)
    roadRenderer.setProjectionMatrix(projectionMatrix)
    roadRenderer.begin(ShapeRenderer.ShapeType.Line)
    roadRenderer.setColor(color)
    roadRenderer.line(start, end)
    roadRenderer.end()
    Gdx.gl.glLineWidth(1)
  }

  def centerScreen(): Unit ={
    cameraCenterX = capitalCell.x * desertLayer.getTileWidth * mapScale - desertLayer.getTileWidth/2  * mapScale
    cameraCenterY = capitalCell.y * desertLayer.getTileHeight * mapScale - desertLayer.getTileHeight /2 * mapScale
  }



  override def show(): Unit = {

   Timer.instance().start()
    import com.badlogic.gdx.Gdx

    if (!game.NO_MUSIC){
      music.setLooping(true)
      music.setVolume(0.1f)
      music.play()
    }

    val multiplexer = new InputMultiplexer()
    multiplexer.addProcessor(stage)
    multiplexer.addProcessor(mapInputProcessor)
    Gdx.input.setInputProcessor(multiplexer)

    centerScreen()

  }

  override def render( d: Float): Unit = {

    val _delta= if (isPaused) 0 else d

    Gdx.gl.glClearColor(1, 0, 0, 1)
    Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT | GL20.GL_DEPTH_BUFFER_BIT)
    stage.getBatch.setColor(Color.WHITE)

    if (Gdx.input.isKeyPressed(Keys.UP) || Gdx.input.isKeyPressed(Keys.W)){
      cameraCenterY += 25
    }

    if (Gdx.input.isKeyPressed(Keys.DOWN) || Gdx.input.isKeyPressed(Keys.S)){
      cameraCenterY -= 25
    }

    if (Gdx.input.isKeyPressed(Keys.LEFT) || Gdx.input.isKeyPressed(Keys.A)){
      cameraCenterX-=25
    }

    if (Gdx.input.isKeyPressed(Keys.RIGHT) || Gdx.input.isKeyPressed(Keys.D)){
      cameraCenterX += 25
    }


    setCameraPosition(camera,_delta)
    stage.act(_delta)
    stage.draw()

    //drawing roads between cities
    roads
        .foreach(road => {
            drawRoadLine(new Vector2(road._1.x * desertLayer.getTileWidth.toFloat + townImage.getWidth /2, road._1.y * desertLayer.getTileHeight.toFloat + townImage.getHeight/2),
              new Vector2(road._2.x  * desertLayer.getTileWidth.toFloat + townImage.getWidth/2, road._2.y * desertLayer.getTileHeight.toFloat + townImage.getHeight/2), 3)
        })

  }

  case class ActorMapCoords(tileX:Int,tileY:Int)


  private def setCameraPosition(camera: OrthographicCamera, delta: Float): Unit ={


    if (cameraCenterY + camera.viewportHeight /2 > mapHeightPixels) {
      cameraCenterY = mapHeightPixels - camera.viewportHeight /2
    }

    if (cameraCenterY - camera.viewportHeight /2 < 0) {
      cameraCenterY = camera.viewportHeight /2
    }

    if (cameraCenterX - camera.viewportWidth/2<0){
      cameraCenterX = camera.viewportWidth/2
    }

    if (cameraCenterX + camera.viewportWidth /2 > mapWidthPixels) {
      cameraCenterX = mapWidthPixels - camera.viewportWidth /2
    }

    camera.position.set(cameraCenterX,cameraCenterY,0)
    camera.update()
    renderer.setView(camera)

    renderer.getBatch.begin()
    renderer.renderTileLayer(desertLayer)
    renderer.renderTileLayer(townLayer)


    if (!game.DISABLE_FOG_OF_WAR){
      renderer.renderTileLayer(fogOfWarLayer)
    }

    renderer.getBatch.end()

    stage.getBatch.begin()
    stage.getBatch.setProjectionMatrix(camera.combined)

    //drawing names where applicable
    locations.foreach(location=>{
      val pixelX : Int = (location.mapCell.x * desertLayer.getTileWidth * mapScale).asInstanceOf[Int]
      val pixelY : Int = (location.mapCell.y * desertLayer.getTileHeight * mapScale).asInstanceOf[Int]
      gameFont.draw(stage.getBatch,location.name,pixelX.toFloat,pixelY.toFloat)
    })

    if (mapDebugOutputEnabled){
      gameFont.draw(stage.getBatch,s"Camera position: ($cameraCenterX,$cameraCenterY), camera viewport size: ${camera.viewportWidth}/${camera.viewportHeight} ,player position: ($cameraCenterX,$cameraCenterY)",camera.unproject(new Vector3(0,0,0)).x,camera.unproject(new Vector3(0,0,0)).y)
    }
    stage.getBatch.end()

    val centerOnCapitalButtonCoords = camera.unproject(new Vector3(stage.getViewport.getScreenWidth - centerOnCapitalButton.getPrefWidth,stage.getViewport.getScreenHeight.toFloat,0))
    centerOnCapitalButton.setPosition(centerOnCapitalButtonCoords.x,centerOnCapitalButtonCoords.y)
    pauseButton.setPosition(centerOnCapitalButton.getX - pauseButton.getPrefWidth - 5,centerOnCapitalButton.getY)
    val scrapLabelCoords = camera.unproject(new Vector3(stage.getViewport.getScreenWidth - scrapLabelButton.getPrefWidth,scrapLabelButton.getPrefHeight,0))
    scrapLabelButton.setPosition(scrapLabelCoords.x,scrapLabelCoords.y)
  }

  override def resize(width: Int, height: Int): Unit = {}

  override def pause(): Unit = {}

  override def resume(): Unit = {}



  override def hide(): Unit = {
    Timer.instance().stop()
    music.stop()
  }

  override def dispose(): Unit = {
    map.dispose()
    renderer.dispose()
    music.dispose()
  }

  private def getClickInfo(cameraXPixel:Float,cameraYPixel:Float):MapClickInfo = {
    val coordX = camera.unproject(new Vector3(cameraXPixel,0,0)).x
    val coordY = camera.unproject(new Vector3(0,cameraYPixel,0)).y
    val clickedTileX = (coordX / desertLayer.getTileWidth).toInt
    val clickedTileY = (coordY / desertLayer.getTileHeight).toInt
    MapClickInfo(coordX,coordY,clickedTileX,clickedTileY)
  }


  private def mapRightClicked(screenX: Int, screenY: Int):Unit = {


    Gdx.app.log("INFO","Right clicked on map")

    val clickInfo = getClickInfo(screenX.toFloat,screenY.toFloat)

    val mapCell = mapData.getCell(clickInfo.tileX,clickInfo.tileY)


  }



  def deleteCamp(camp: RaiderCampInfo) = {
    if (locations.contains(camp)){
      locations -= camp
      camp.mapCell.location = None
      val desertTile = new StaticTiledMapTile(new TextureRegion(desertTileTexture))
      val desertCell = new Cell
      desertCell.setTile(desertTile)
      townLayer.setCell(camp.mapCell.x,camp.mapCell.y,desertCell)
    }
  }

}

object MapScreen{


  case class MapClickInfo(pixelX:Float, pixelY: Float, tileX:Int,tileY:Int)


  sealed abstract class MapLocation(){
    def mapCell:MapCellData
    def name:String
  }
  case class RaiderCampInfo( name:String,mapCell: MapCellData) extends MapLocation()
  case class CityInfo(name:String,mapCell: MapCellData, population: Int, var isOwnedByPlayer:Boolean = false) extends MapLocation()
  case class RuinsInfo(mapCell: MapCellData,name:String = "Pre-war ruins") extends MapLocation()
  case class CoveredAreaInfo(mapCell: MapCellData,name:String="") extends MapLocation

  case class Technology(name:String, var enabled:Boolean = false)

}
