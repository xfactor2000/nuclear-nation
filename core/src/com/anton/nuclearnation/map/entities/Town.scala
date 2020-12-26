package com.anton.nuclearnation.map.entities
import com.anton.nuclearnation.map.MapTile

class Town(val bottomLeftTile: MapTile) extends StaticMapEntity {

  /**
   * Provides size X*Y size in cells
   */
  override def size: (Int, Int) = (4,4)
}
