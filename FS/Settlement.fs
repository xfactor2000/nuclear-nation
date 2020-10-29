module FS.Settlement

open System
open Godot

[<AbstractClass>]
type BuildingFs() =
    inherit Node2D() 
       abstract member GetSizeInTiles: int->Vector2

type HouseFs() =
    inherit BuildingFs()
    
    override this.GetSizeInTiles(tileSize:int) =
        (this.GetNode(new NodePath("ActiveHouse")) :?> Sprite).Texture.GetSize() / (float32 tileSize);

type Road(from:Vector2,``to``:Vector2) =
    inherit Node()
    
    member val from:Vector2 = from
    member val ``to``:Vector2 = ``to``
    
    member this.IsVertical() =
        int from.x = int ``to``.x
    
    /// <summary>
    /// This function returns two lists of free tiles alongside the road
    /// </summary>
    /// <returns>Two lists of free tiles alongside the road, top/bottom or left/right</returns>
    member this.GetFreeTilesAlong():Tuple<List<Vector2>,List<Vector2>> =
        let tiles = this.GetTiles()
        let tilesList1 =
                if this.IsVertical() then
                    seq{for tile in tiles do yield Vector2(tile.x-1.0f,tile.y)}
                else
                    seq{for tile in tiles do yield Vector2(tile.x,tile.y-1.0f)}
        
        let tilesList2 =
                if this.IsVertical() then
                    seq{for tile in tiles do yield Vector2(tile.x+1.0f,tile.y)}
                else
                    seq{for tile in tiles do yield Vector2(tile.x,tile.y+1.0f)}    
       
        (Seq.toList tilesList1,Seq.toList tilesList2)
    
    member this.GetTiles():List<Vector2> =
        let to_x:int =
            let tmp_to_x = int (if ((int)``to``.x = (int)from.x) then int ``to``.x + 1 else int ``to``.x)
            if (this.IsVertical() = false) then tmp_to_x else tmp_to_x - 1
        let to_y:int =
            let tmp_to_y = int (if ((int)``to``.y = (int)from.y) then int ``to``.y + 1 else int ``to``.y)
            if (this.IsVertical()) then tmp_to_y else tmp_to_y - 1
        
        let xs = seq{for i in int from.x .. to_x do yield i}
        let ys = seq{for i in int from.y .. to_y do yield i}
        
        let vectors = seq {
            for x in xs do
                for y in ys do yield Vector2(float32 x,float32 y)
        }
        
        Seq.toList vectors