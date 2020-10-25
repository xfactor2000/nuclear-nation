module FS.Building


open Godot

[<AbstractClass>]
type BuildingFs() =
    inherit Node2D() 
       abstract member GetSizeInTiles: int->Vector2