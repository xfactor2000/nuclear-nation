package com.anton.nuclearnation

import java.io.File

import com.anton.nuclearnation.MapScreen.MapLocation
import com.badlogic.gdx.{ApplicationAdapter, Game, Gdx, Screen}
import com.badlogic.gdx.assets.AssetManager
import com.badlogic.gdx.assets.loaders.resolvers.{ExternalFileHandleResolver, InternalFileHandleResolver}
import com.badlogic.gdx.graphics.GL20
import com.badlogic.gdx.graphics.Texture
import com.badlogic.gdx.graphics.g2d.freetype.{FreeTypeFontGenerator, FreeTypeFontGeneratorLoader, FreetypeFontLoader}
import com.badlogic.gdx.graphics.g2d.freetype.FreetypeFontLoader.FreeTypeFontLoaderParameter
import com.badlogic.gdx.graphics.g2d.{BitmapFont, SpriteBatch}
import com.badlogic.gdx.scenes.scene2d.ui.Skin

import scala.collection.mutable.ListBuffer


class NuclearNation extends Game {
  var batch:SpriteBatch = _
  val assetManager = new AssetManager

  val resolver = new InternalFileHandleResolver
  val fontGenerator = new FreeTypeFontGeneratorLoader(resolver)

  assetManager.setLoader(classOf[FreeTypeFontGenerator], fontGenerator)
  assetManager.setLoader(classOf[BitmapFont], ".ttf", new FreetypeFontLoader(resolver))



  val DISABLE_FOG_OF_WAR = if (sys.env.get("DISABLE_FOG_OF_WAR").isEmpty) false else sys.env("DISABLE_FOG_OF_WAR").toLowerCase().toBoolean

  lazy val skin = assetManager.get("data/commodore64/skin/uiskin.json",classOf[Skin])
  val gameFontParam = new FreeTypeFontLoaderParameter()

  lazy val mapScreen = new MapScreen(this)

  val NO_MUSIC = if (sys.env.get("NO_MUSIC").isEmpty) true else sys.env("NO_MUSIC").toLowerCase().toBoolean

  //unit counts
  val initialUnitCount = if (sys.env.get("INITIAL_UNIT_COUNT").isEmpty) 0 else sys.env("INITIAL_UNIT_COUNT").toInt
  var soldierCounter:Int = initialUnitCount
  var scientistCounter:Int = initialUnitCount
  var engineerCounter:Int = initialUnitCount


  override def create(): Unit = {
    batch = new SpriteBatch

    gameFontParam.fontFileName = "fonts/lunchtime-doubly-so/lunchds.ttf"
    gameFontParam.fontParameters.size = 30
    assetManager.load("fonts/lunchtime-doubly-so/lunchds.ttf", classOf[BitmapFont], gameFontParam)

    assetManager.load("raider_camp.png",classOf[Texture])
    assetManager.load("expedition.png",classOf[Texture])
    assetManager.load("tradeCaravan.png",classOf[Texture])
    assetManager.load("militaryCaravan.png",classOf[Texture])
    assetManager.load("desert_tile.png",classOf[Texture])
    assetManager.load("fog_of_war_tile.png",classOf[Texture])
    assetManager.load("town.png",classOf[Texture])
    assetManager.load("raider-facing-left.png",classOf[Texture])
    assetManager.load("soldier-facing-right.png",classOf[Texture])
    assetManager.load("soldier-facing-left.png",classOf[Texture])
    assetManager.load("ruined-building.png",classOf[Texture])
    assetManager.load("data/commodore64/skin/uiskin.json",classOf[Skin])
    assetManager.load("unitConstruction/crossedSwords.png",classOf[Texture])
    assetManager.load("unitConstruction/soldierUnit.png",classOf[Texture])
    assetManager.load("unitConstruction/engineerUnit.png",classOf[Texture])
    assetManager.load("unitConstruction/scientistUnit.png",classOf[Texture])
    assetManager.load("unitConstruction/commandoUnit.png",classOf[Texture])
    assetManager.load("unitConstruction/spyUnit.png",classOf[Texture])
    assetManager.load("unitConstruction/spyKeyhole.png",classOf[Texture])
    assetManager.load("unitConstruction/questionMark.png",classOf[Texture])
    assetManager.load("actionMix/expeditionOutcome.png",classOf[Texture])

    assetManager.load("cardGameScreen/rubble.png",classOf[Texture])
    assetManager.load("cardGameScreen/brokenMachinery.png",classOf[Texture])
    assetManager.load("cardGameScreen/monster.png",classOf[Texture])
    assetManager.load("city/cityPicture.png",classOf[Texture])

    assetManager.finishLoading()

    this.setScreen(mapScreen)
  }


  override def render(): Unit = {
    super.render()
  }

  override def dispose(): Unit = {
    batch.dispose()
    assetManager.dispose()
    mapScreen.dispose()
  }

}
