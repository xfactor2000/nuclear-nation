module FS.Road

open Godot

type Road(from:Vector2,``to``:Vector2) =
    inherit Reference()
    
    member val from:Vector2 = from
    member val ``to``:Vector2 = ``to``
    
    member this.IsVertical() =
        int from.x = int ``to``.x
    
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
       
    
