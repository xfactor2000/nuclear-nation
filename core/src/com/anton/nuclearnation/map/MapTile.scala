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

  override def equals(that:Any): Boolean = {
    that match {
      case other: MapTile =>
        other == that || x == other.x && y == other.y
      case _ =>
        super.equals(that)
    }
  }
}

object MapTile {
  def apply(x:Int, y:Int): MapTile = {
    new MapTile(x,y)
  }
}

