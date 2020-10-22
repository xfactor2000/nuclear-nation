using Godot;

namespace nuclearnation
{
    public abstract class Building: Node2D
    {
        public abstract Vector2 GetSizeInTiles(int tileSize);
    }
}