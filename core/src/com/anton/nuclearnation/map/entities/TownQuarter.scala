package com.anton.nuclearnation.map.entities
import com.anton.nuclearnation.map.MapTile

class TownQuarter(val bottomLeftTile:MapTile) extends StaticMapEntity {
  /**
   * Provides size X*Y size in cells
   *
   * @return
   */
  override def size: (Int, Int) = (1,1)
}
