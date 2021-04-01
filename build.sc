import mill._
import scalalib._

def projectScalaVersion = "2.13.5"
object core extends ScalaModule {
  val gdxVersion="1.9.14"

  override def ivyDeps = Agg(
    ivy"com.badlogicgames.gdx:gdx-backend-lwjgl:$gdxVersion",
    ivy"com.badlogicgames.gdx:gdx-platform:$gdxVersion;classifier=natives-desktop",
    ivy"com.badlogicgames.gdx:gdx-freetype-platform:$gdxVersion;classifier=natives-desktop",
    ivy"com.badlogicgames.gdx:gdx-tools:$gdxVersion"
  )
  def scalaVersion = projectScalaVersion
}

object desktop extends ScalaModule {
  val gdxVersion="1.9.12"
  override def moduleDeps = Seq(core)
  def scalaVersion = projectScalaVersion
  override def forkArgs =  Seq("-agentlib:jdwp=transport=dt_socket,server=y,suspend=n,address=5005")

}