package com.anton.nuclearnation

import com.badlogic.gdx.graphics.g2d.Batch

object Extensions {
  implicit class BatchExtras(val b: Batch) extends AnyVal {
    def drawBatch(actions:(Batch)=>Unit){
      b.begin()
      actions(b)
      b.`end`()
    }
  }
}
