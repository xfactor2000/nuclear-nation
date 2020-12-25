package com.anton.nuclearnation.map

import scala.collection.mutable.ListBuffer

/**
 * Represents the data about everything on the map.
 * @param mapWidth - width of map in smallest cells(32x32)
 * @param mapHeight - height of map in smallest cells (32x32)
 */
class MapData(mapWidth: Int,mapHeight:Int) {
  private val cellsBuffer = ListBuffer[MapCellData]()
  for (
    x <- 0 until mapWidth;
    y <- 0 until mapHeight
  ) yield  {
    cellsBuffer += MapCellData(x,y,None)
  }

  val cells = cellsBuffer.toList

  def getCell(x:Int,y:Int): Option[MapCellData] ={
    cells.find(cell=>cell.x == x && cell.y == y)
  }
}
