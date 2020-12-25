package com.anton.nuclearnation.map

/**
 * Represents the smallest tile on the map (32x32)
 * @param x
 * @param y
 */
class MapTile(val x:Int,val y:Int){
  def getBottomLeftScreenCoords:(Int,Int)= {
    (x*32,y*32)
  }
}
