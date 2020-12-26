package com.anton.nuclearnation

package object map {
  /**
   * Defines the size of the basic map tile
   */
  val (mapTileSizeX:Int,mapTileSizeY:Int) = (32,32)

  /**
   * Defines the map data for this specific map
   */
  val mapWidthTiles = 60
  val mapHeightTiles = 60
  val mapData = new MapData(mapWidthTiles,mapHeightTiles)
}
