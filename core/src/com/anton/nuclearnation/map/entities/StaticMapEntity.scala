package com.anton.nuclearnation.map.entities

import com.anton.nuclearnation.map.{MapTile, mapTileSizeX, mapTileSizeY}

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
   * Adds child entity to this entity
   * @param entity
   */
  def addChild(entity: StaticMapEntity): Unit ={
    children += entity
  }

  /**
   * Returns the list of bordering tiles minus illegitimate ones (outside of map borders)
   */
  def borderingTiles:List[MapTile] = {
    val (fromX, fromY) = (bottomLeftTile.x - 1, bottomLeftTile.y - 1)
    val (toX, toY) = (bottomLeftTile.x + size._1, bottomLeftTile.y + size._2)

    //calculating total tiles (one tile bigger on every dimenstion, meaning if the original entity size is 1 then it will return 3x3 cells)
    val totalTiles = {
      for {
        x <- fromX to toX
        y <- fromY to toY
      } yield new MapTile(x, y)
    }.toList

    //removing self owned tiles from the total tiles (from 9 cells calculating earlier, remove the one cell that actually belongs to the entity itself)
    //after that, remove illegitimate cells (cells outside of map borders)
    totalTiles
      .filterNot(selfOwnedTiles.toSet)
      .filter(c => c.x > 0 && c.x < mapTileSizeX && c.y > 0 && c.y < mapTileSizeY)
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
