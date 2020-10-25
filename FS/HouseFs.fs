module FS.House

open FS.Building
open Godot

type HouseFs() =
    inherit BuildingFs()
    
    override this.GetSizeInTiles(tileSize:int) =
        (this.GetNode(new NodePath("ActiveHouse")) :?> Sprite).Texture.GetSize() / (float32 tileSize);
