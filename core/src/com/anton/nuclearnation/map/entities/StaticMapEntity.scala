package com.anton.nuclearnation.map.entities

import com.anton.nuclearnation.map.MapTile

/**
 * Represents static entities on the map (which can not move), i.e. - buildings, towns, camps, improvements, etc.
 * It knows about itself the bottom left map tile that serves as its anchor.
 */
trait StaticMapEntity {

  /**
   * Represents the bottom left tile on the map which "anchors" this entity
   */
  val bottomLeftTile:MapTile
  /**
   * Provides size X*Y size in cells
   * @return
   */
  def size:(Int,Int)
}
