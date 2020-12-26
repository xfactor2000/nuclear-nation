package com.anton.nuclearnation.map.entities

import com.anton.nuclearnation.map.MapTile

import scala.collection.mutable.ListBuffer

/**
 * Represents static entities on the map (which can not move), i.e. - buildings, towns, camps, improvements, etc.
 * It knows about itself the bottom left map tile that serves as its anchor.
 */
trait StaticMapEntity {

  val children:ListBuffer[StaticMapEntity] = ListBuffer()
  /**
   * Represents the bottom left tile on the map which "anchors" this entity
   */
  val bottomLeftTile:MapTile
  /**
   * Provides size X*Y size in cells
   * @return
   */
  def size:(Int,Int)

  /**
   * Adds child entity to this entity and makes sure it's not outside the map
   * @param entity
   */
  def addChild(entity: StaticMapEntity): Unit ={
    children += entity
  }

  /**
   * Returns the tiles owned by this entity
   * @return
   */
  def selfOwnedTiles:List[MapTile] = {
    val (startX,startY) = (bottomLeftTile.x,bottomLeftTile.y)
    val tiles =
      for {
        x <- startX until (startX + size._1)
        y <- startY until (startY + size._2)
      } yield new MapTile(x, y)

    tiles.toList
  }

  def childrenOwnedTiles:List[MapTile] = {
    val tiles = children.flatMap(c=>c.selfOwnedTiles)
    tiles.toList
  }

  def totalOwnedTiles:List[MapTile] = selfOwnedTiles ++ totalOwnedTiles

}
