package com.anton.nuclearnation.map

/**
 * Represents the smallest tile on the map (32x32)
 * @param x
 * @param y
 */
class MapTile(val x:Int,val y:Int){
  def bottomLeftScreenCoords:(Int,Int)= {
    (x*mapTileSizeX,y*mapTileSizeY)
  }
}

object MapTile {
  def apply(x:Int, y:Int): MapTile = {
    new MapTile(x,y)
  }
}

