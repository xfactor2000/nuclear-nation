using Godot;

namespace nuclearnation
{
	public class House: Building
	{

		public override Vector2 GetSizeInTiles(int tileSize)
		{
			return ((Sprite) GetNode("ActiveHouse")).Texture.GetSize() / tileSize;
		}
		
	}
}
