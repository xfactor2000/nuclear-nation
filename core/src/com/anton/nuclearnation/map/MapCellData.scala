package com.anton.nuclearnation.map

import com.anton.nuclearnation.MapScreen.MapLocation

/**
 * Describes the location on the map. The x,y are related to the bigger cell coordinates,
 * inside of which smaller cells can be placed.
 * @param x
 * @param y
 * @param location
 */
case class MapCellData(x:Int,y:Int, var location:Option[MapLocation]) {}


