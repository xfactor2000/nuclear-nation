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

  var turn = 0

  val map = new TiledMap

  val mapWidthTiles = 60
  val mapHeightTiles = 60

  val desertTileTexture: Texture = assetManager.get("desert_tile-64x64.png",classOf[Texture])
//  val ruinedBuildingTexture = assetManager.get("ruined-building.png",classOf[Texture])
  val desertLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
//  val townLayer = new TiledMapTileLayer(mapHeightTiles, mapWidthTiles, desertTileTexture.getWidth, desertTileTexture.getHeight)
  val desertTileCell:Cell = new Cell
  val region = new TextureRegion(desertTileTexture)

  desertTileCell.setTile(new StaticTiledMapTile(region))

  val mapData = new MapData(mapWidthTiles,mapHeightTiles)

  val skin: Skin = assetManager.get("data/commodore64/skin/uiskin.json",classOf[Skin])

  val stage = new Stage(new StretchViewport(1600,960,new OrthographicCamera()))
  val camera: OrthographicCamera = stage.getCamera.asInstanceOf[OrthographicCamera]

  val centerOnCapitalButton = new TextButton("Re-center",skin)
  val pauseButton = new TextButton("Pause",skin)

  val turnLabel = new TextButton(s"Turn: $turn",skin) //using Button because it looks better with this skin

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

  val hopeTown = new Town(MapTile(10,10))

  mapData.addStaticEntity(hopeTown)

  for (
    x <- 0 until mapWidthTiles;
    y <- 0 until mapHeightTiles
  ) yield  {
    desertLayer.setCell(x, y, desertTileCell)

  }
  map.getLayers.add(desertLayer)

  val renderer = new OrthogonalTiledMapRenderer(map, 1.0f)

  val mapWidthPixels: Int = desertLayer.getWidth * desertLayer.getTileWidth
  val mapHeightPixels: Int = desertLayer.getHeight * desertLayer.getTileHeight

  var cameraCenterX = 0f
  var cameraCenterY = 0f

  def centerScreen(): Unit ={
    cameraCenterX = camera.viewportWidth/2
    cameraCenterY = camera.viewportHeight/2
  }



  override def show(): Unit = {
    val multiplexer = new InputMultiplexer()
    multiplexer.addProcessor(stage)
    multiplexer.addProcessor(mapInputProcessor)
    Gdx.input.setInputProcessor(multiplexer)
    centerScreen()
  }

  def getMapMovementVector(keys:List[Int], directionToReturn:Int):Option[Int]={
    keys.foreach(key=>{
      if (Gdx.input.isKeyPressed(key)) return Some(directionToReturn)
    })
    None
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
        batch.draw(entity.mapImage,entity.bottomLeftTile.x * mapTileSizeX,entity.bottomLeftTile.y * mapTileSizeY)
        //rendering children
        entity.children.foreach(c=>{
          batch.draw(c.mapImage,entity.bottomLeftTile.x * mapTileSizeX,entity.bottomLeftTile.y * mapTileSizeY)
        })
      })

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

  def updateTurn(): Unit ={
    turn +=1
    turnLabel.setText(s"Turn: $turn")
    turnLabel.setWidth(turnLabel.getPrefWidth)
  }

}
