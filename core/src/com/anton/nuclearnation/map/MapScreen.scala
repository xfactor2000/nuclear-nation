package com.anton.nuclearnation.map

import com.anton.nuclearnation.Extensions._
import com.anton.nuclearnation.assetManager
import com.anton.nuclearnation.map.entities.{StaticMapEntity, Town, TownQuarter}
import com.badlogic.gdx.Input.Keys
import com.badlogic.gdx._
import com.badlogic.gdx.graphics._
import com.badlogic.gdx.graphics.g2d.TextureRegion
import com.badlogic.gdx.maps.tiled.TiledMapTileLayer.Cell
import com.badlogic.gdx.maps.tiled.renderers.OrthogonalTiledMapRenderer
import com.badlogic.gdx.maps.tiled.tiles.StaticTiledMapTile
import com.badlogic.gdx.maps.tiled.{TiledMap, TiledMapTileLayer}
import com.badlogic.gdx.math.Vector3
import com.badlogic.gdx.scenes.scene2d.Stage
import com.badlogic.gdx.scenes.scene2d.ui.{Skin, TextButton}
import com.badlogic.gdx.utils.viewport.StretchViewport


class MapScreen() extends Screen{

  val map = new TiledMap
  var day = 0
  var isPaused = false

  val desertTileTexture: Texture = assetManager.get("desert_tile-64x64.png",classOf[Texture])
//  val ruinedBuildingTexture = assetManager.get("ruined-building.png",classOf[Texture])
  val desertLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
//  val townLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val desertTileCell:Cell = new Cell
  val region = new TextureRegion(desertTileTexture)


  desertTileCell.setTile(new StaticTiledMapTile(region))
  val skin: Skin = assetManager.get("data/commodore64/skin/uiskin.json",classOf[Skin])
  val stage = new Stage(new StretchViewport(1600,960,new OrthographicCamera()))
  val camera: OrthographicCamera = stage.getCamera.asInstanceOf[OrthographicCamera]
  val centerOnCapitalButton = new TextButton("Re-center",skin)
  val pauseButton = new TextButton("Pause",skin)
  val dayLabel = new TextButton(s"Day: $day",skin) //using Button because it looks better with this skin
  val mapInputProcessor: InputProcessor = new InputProcessor() {

    override def touchDown(screenX: Int, screenY: Int, pointer: Int, button: Int): Boolean = {true}

    override def keyDown(keycode: Int): Boolean = {true}

    override def keyUp(keycode: Int): Boolean = {
      keycode match {
        case Input.Keys.SPACE=>
          pauseUnpause()
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

  stage.addActor(dayLabel)
  val hopeTown = new Town(MapTile(10,10))
  val renderer = new OrthogonalTiledMapRenderer(map, 1.0f)

  mapData.addStaticEntity(hopeTown)

  for (
    x <- 0 until mapWidthTiles;
    y <- 0 until mapHeightTiles
  ) yield  {
    desertLayer.setCell(x, y, desertTileCell)

  }
  map.getLayers.add(desertLayer)
  val mapWidthPixels: Int = desertLayer.getWidth * desertLayer.getTileWidth
  val mapHeightPixels: Int = desertLayer.getHeight * desertLayer.getTileHeight
  var lastDelta = 0f
  var cameraCenterX = 0f
  var cameraCenterY = 0f

  override def show(): Unit = {
    val multiplexer = new InputMultiplexer()
    multiplexer.addProcessor(stage)
    multiplexer.addProcessor(mapInputProcessor)
    Gdx.input.setInputProcessor(multiplexer)
    centerScreen()
  }

  def centerScreen(): Unit ={
    cameraCenterX = camera.viewportWidth/2
    cameraCenterY = camera.viewportHeight/2
  }

  override def render(delta: Float): Unit = {

    val moveXDirection =
      getMapMovementVector(List(Keys.RIGHT,Keys.D),1).getOrElse(0) +
        getMapMovementVector(List(Keys.LEFT,Keys.A),-1).getOrElse(0)

    val moveYDirection =
      getMapMovementVector(List(Keys.UP,Keys.W),1).getOrElse(0) +
        getMapMovementVector(List(Keys.DOWN,Keys.S),-1).getOrElse(0)

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

    //drawing static entities
    renderer.getBatch.drawBatch(batch=>{
      renderer.renderTileLayer(desertLayer)

      mapData.staticEntities.foreach(entity=>{
        val entityAndChildren = List(entity) ++ entity.children
        entityAndChildren.foreach(e=>{
          batch.draw(e.mapImage,entity.bottomLeftTile.x * mapTileSizeX,entity.bottomLeftTile.y * mapTileSizeY)
        })
      })

    })

    //updating day every 5 seconds
    lastDelta += delta
    if (lastDelta > 1){
      print(lastDelta)
      day +=1
      lastDelta = 0
    }

    dayLabel.setText(s"Day: $day")
    dayLabel.setWidth(dayLabel.getPrefWidth)

    val scrapLabelCoords = camera.unproject(new Vector3(stage.getViewport.getScreenWidth - dayLabel.getPrefWidth,dayLabel.getPrefHeight,0))
    dayLabel.setPosition(scrapLabelCoords.x,scrapLabelCoords.y)

    stage.act(delta)
    stage.draw()

}

  def getMapMovementVector(keys:List[Int], directionToReturn:Int):Option[Int]={
    keys.foreach(key=>{
      if (Gdx.input.isKeyPressed(key)) return Some(directionToReturn)
    })
    None
  }

  override def resize(width: Int, height: Int): Unit = {}

  override def pause(): Unit = {}

  override def resume(): Unit = {}

  override def hide(): Unit = {
  }

  override def dispose(): Unit = {
    map.dispose()
    renderer.dispose()
  }

  def pauseUnpause(): Unit ={
    isPaused = !isPaused
  }

  case class ActorMapCoords(tileX:Int,tileY:Int)


}
