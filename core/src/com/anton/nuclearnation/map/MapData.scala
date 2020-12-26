package com.anton.nuclearnation.map

import com.anton.nuclearnation.map.entities.StaticMapEntity

import scala.collection.mutable.ListBuffer

/**
 * Represents the data about everything on the map.
 * @param mapWidth - width of map in smallest cells(32x32)
 * @param mapHeight - height of map in smallest cells (32x32)
 */
class MapData(mapWidth: Int,mapHeight:Int) {
  val staticEntities:ListBuffer[StaticMapEntity] = ListBuffer[StaticMapEntity]()

  def addStaticEntity(entity: StaticMapEntity):List[StaticMapEntity] = {
    (staticEntities += entity).toList
  }
}
