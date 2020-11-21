import mill._
import scalalib._


object core extends ScalaModule {
  val gdxVersion="1.9.12"

  override def ivyDeps = Agg(
    ivy"com.badlogicgames.gdx:gdx-backend-lwjgl:$gdxVersion",
    ivy"com.badlogicgames.gdx:gdx-platform:$gdxVersion;classifier=natives-desktop",
    ivy"com.badlogicgames.gdx:gdx-freetype-platform:$gdxVersion;classifier=natives-desktop",
    ivy"com.badlogicgames.gdx:gdx-tools:$gdxVersion"
  )
  def scalaVersion = "2.13.3"
}

object desktop extends ScalaModule {
  val gdxVersion="1.9.12"
  override def moduleDeps = Seq(core)
  def scalaVersion = "2.13.3"
  override def forkArgs =  Seq("-agentlib:jdwp=transport=dt_socket,server=y,suspend=n,address=5005")

}