package com.anton.nuclearnation

import com.anton.nuclearnation.Extensions._
import com.anton.nuclearnation.MapScreen._
import com.badlogic.gdx.Input.Keys
import com.badlogic.gdx._
import com.badlogic.gdx.graphics._
import com.badlogic.gdx.graphics.g2d.TextureRegion
import com.badlogic.gdx.maps.tiled.{TiledMap, TiledMapTileLayer}
import com.badlogic.gdx.maps.tiled.TiledMapTileLayer.Cell
import com.badlogic.gdx.maps.tiled.renderers.OrthogonalTiledMapRenderer
import com.badlogic.gdx.maps.tiled.tiles.StaticTiledMapTile
import com.badlogic.gdx.math.Vector3
import com.badlogic.gdx.scenes.scene2d.Stage
import com.badlogic.gdx.scenes.scene2d.ui.{Skin, TextButton}
import com.badlogic.gdx.utils.viewport.StretchViewport


class MapScreen(game: NuclearNation) extends Screen{

  val assetManager = game.assetManager
  var turn = 0

  val map = new TiledMap
  val layers = map.getLayers

  val mapWidthTiles = 60
  val mapHeightTiles = 60

  val mapTileSize: Int = 32

  val desertTileTexture = assetManager.get("desert_tile-64x64.png",classOf[Texture])
//  val ruinedBuildingTexture = assetManager.get("ruined-building.png",classOf[Texture])
  val desertLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
//  val townLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val desertTileCell:Cell = new Cell
  val region = new TextureRegion(desertTileTexture)
  val hamletImage: Texture = assetManager.get("settlements/house_active-64x64.png",classOf[Texture])
  val townQuarterImage: Texture = assetManager.get("settlements/house_active-32x32.png",classOf[Texture])
  desertTileCell.setTile(new StaticTiledMapTile(region))

  val mapData = new MapData(mapWidthTiles,mapHeightTiles)


  val skin: Skin = assetManager.get("data/commodore64/skin/uiskin.json",classOf[Skin])

  val stage = new Stage(new StretchViewport(1600,960,new OrthographicCamera()))
  val camera: OrthographicCamera = stage.getCamera.asInstanceOf[OrthographicCamera]

  val centerOnCapitalButton = new TextButton("Re-center",skin)
  val pauseButton = new TextButton("Pause",skin)

  val hamletCell: MapCellData = mapData.getCell(10,10).get
  hamletCell.location = Some(HamletQuarter())


  val turnLabel = new TextButton(s"Turn: $turn",skin) //using Button because it looks better with this skin
  val mapScale = 1.0f

    stage.addActor(turnLabel)



  val mapInputProcessor: InputProcessor = new InputProcessor() {

    override def touchDown(screenX: Int, screenY: Int, pointer: Int, button: Int): Boolean = {true}

    override def keyDown(keycode: Int): Boolean = {true}

    override def keyUp(keycode: Int): Boolean = {
      keycode match {
        case Input.Keys.ENTER=>
          updateTurn()
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



  for (
    x <- 0 until mapWidthTiles;
    y <- 0 until mapHeightTiles
  ) yield  {
    desertLayer.setCell(x, y, desertTileCell)

  }
  map.getLayers.add(desertLayer)

  val renderer = new OrthogonalTiledMapRenderer(map, mapScale)

  val mapWidthPixels: Int = (desertLayer.getWidth * desertLayer.getTileWidth * mapScale).asInstanceOf[Int]
  val mapHeightPixels: Int = (desertLayer.getHeight * desertLayer.getTileHeight * mapScale).asInstanceOf[Int]

  var cameraCenterX = 0f
  var cameraCenterY = 0f

  def centerScreen(): Unit ={
    cameraCenterX = camera.viewportWidth*mapScale/2
    cameraCenterY = camera.viewportHeight*mapScale/2
  }



  override def show(): Unit = {

    val multiplexer = new InputMultiplexer()
    multiplexer.addProcessor(stage)
    multiplexer.addProcessor(mapInputProcessor)
    Gdx.input.setInputProcessor(multiplexer)
    centerScreen()

  }

  override def render(delta: Float): Unit = {

    val moveXDirection =
      if (Gdx.input.isKeyPressed(Keys.RIGHT) || Gdx.input.isKeyPressed(Keys.D)){
        1
      } else if (Gdx.input.isKeyPressed(Keys.LEFT) || Gdx.input.isKeyPressed(Keys.A)){
        -1
      } else
        0

    val moveYDirection =
      if (Gdx.input.isKeyPressed(Keys.UP) || Gdx.input.isKeyPressed(Keys.W)){
        1
      } else if (Gdx.input.isKeyPressed(Keys.DOWN) || Gdx.input.isKeyPressed(Keys.S)){
        -1
      } else
        0

    cameraCenterY = {
      val newCameraCenterY = cameraCenterY + 25 * moveYDirection
      newCameraCenterY.min(mapHeightPixels - camera.viewportHeight /2).max(camera.viewportHeight /2)
    }

    cameraCenterX = {
      val newCameraCenterX = cameraCenterX + 25 * moveXDirection
      newCameraCenterX.min(mapWidthPixels - camera.viewportWidth /2).max(camera.viewportWidth /2)
    }

    camera.position.set(cameraCenterX,cameraCenterY,0)
    camera.update()
    renderer.setView(camera)

    renderer.getBatch.drawBatch(batch=>{
      renderer.renderTileLayer(desertLayer)
      //drawing the quarters
      batch.draw(townQuarterImage,0 * mapTileSize * mapScale,0 * mapTileSize * mapScale)
    })

    val scrapLabelCoords = camera.unproject(new Vector3(stage.getViewport.getScreenWidth - turnLabel.getPrefWidth,turnLabel.getPrefHeight,0))
    turnLabel.setPosition(scrapLabelCoords.x,scrapLabelCoords.y)

    stage.act(delta)
    stage.draw()

}

  case class ActorMapCoords(tileX:Int,tileY:Int)



  override def resize(width: Int, height: Int): Unit = {}

  override def pause(): Unit = {}

  override def resume(): Unit = {}



  override def hide(): Unit = {
  }

  override def dispose(): Unit = {
    map.dispose()
    renderer.dispose()
  }

  def getBorderTiles(bottomLeftCoord:(Int,Int),sizeInTiles:(Int,Int)):List[MapTile] = {
    val (fromX,toX) = (bottomLeftCoord._1, bottomLeftCoord._1 + sizeInTiles._1)
    val (fromY,toY) = (bottomLeftCoord._2, bottomLeftCoord._2 + sizeInTiles._2)
    val borderTiles = for {
      x <- fromX to toX
      y <- fromY to toY
    } yield new MapTile(x,y)

    borderTiles.toList
  }

  def getSizeInTiles(t:Texture, tileSize:Int = mapTileSize): (Int,Int) = {
    (t.getWidth / tileSize,t.getHeight/tileSize)
  }

  def updateTurn(): Unit ={
    turn +=1
    turnLabel.setText(s"Turn: $turn")
    turnLabel.setWidth(turnLabel.getPrefWidth)

    //adding new house each turn
//    def findNeighboringEmptyCell(sourceCell:MapCellData):Option[MapCellData] = {
//      val surroundingCells = List(
//        mapData.getCell(sourceCell.x-1,sourceCell.y),
//        mapData.getCell(sourceCell.x+1,sourceCell.y),
//        mapData.getCell(sourceCell.x,sourceCell.y-1),
//        mapData.getCell(sourceCell.x,sourceCell.y+1)
//      )
//        .filter(c=>c.isDefined)
//        .map(c=>c.get)
//        .filter(c=> c.x >0 && c.y > 0 && c.x< mapWidthTiles && c.y < mapHeightTiles)
//      val random = new Random()
//      val emptyCells = surroundingCells.filter(c=>c.location.isEmpty)
//      if (emptyCells.isEmpty ) {
//        random.shuffle(surroundingCells).foreach(c=>{
//          val result = findNeighboringEmptyCell(c)
//          result match {
//            case Some(c) => return Some(c)
//            case None =>
//          }
//        })
//        None
//      } else {
//        val randomCell = emptyCells(random.nextInt(emptyCells.length))
//        Some(randomCell)
//      }
//
//
//    }
//    val cell = findNeighboringEmptyCell(hamletCell)
//    cell.get.location = Some(TownQuarter())
//    val townRegion = new TextureRegion(hamletImage)
//    val townTile = new StaticTiledMapTile(townRegion)
//    val townCell = new Cell
//    townCell.setTile(townTile)
//    townLayer.setCell(cell.get.x,cell.get.y,townCell)


  }

}

object MapScreen{


  case class MapClickInfo(pixelX:Float, pixelY: Float, tileX:Int,tileY:Int)


  sealed abstract class MapLocation(){
//    def name:String
  }
  case class TownQuarter() extends MapLocation
  case class HamletQuarter() extends MapLocation

}
