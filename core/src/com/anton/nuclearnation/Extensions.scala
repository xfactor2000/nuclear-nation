package com.anton.nuclearnation

import com.anton.nuclearnation.map.entities.{Hamlet, StaticMapEntity}
import com.badlogic.gdx.graphics.Texture
import com.badlogic.gdx.graphics.g2d.Batch

object Extensions {
  implicit class BatchExtras(val b: Batch) extends AnyVal {
    def drawBatch(actions:(Batch)=>Unit){
      b.begin()
      actions(b)
      b.`end`()
    }
  }

  implicit class MapEntitiesExtras(val e: StaticMapEntity) extends AnyVal {
    def mapImage:Texture = {
      e match {
        case _:Hamlet =>
          val hamletImage: Texture = assetManager.get("settlements/house_active-64x64.png",classOf[Texture])
          hamletImage
        case _ => throw new RuntimeException("Unknown map entity; Can not render")
      }
    }
  }
}
