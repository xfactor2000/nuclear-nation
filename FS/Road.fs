module FS.Road

open Godot

type Road(from:Vector2,``to``:Vector2) =
    inherit Reference()
    
    member val from:Vector2 = from
    member val ``to``:Vector2 = ``to``
    
    member this.IsVertical() =
        int from.x = int ``to``.x
    
    member this.GetTiles():List<Vector2> =
        let to_x:int = int (if ((int)``to``.x = (int)from.x) then int ``to``.x + 1 else int ``to``.x)
        let to_y:int = int (if ((int)``to``.y = (int)from.y) then int ``to``.y + 1 else int ``to``.y)
        
        let xs = seq{for i in int from.x .. to_x - 1 do yield i}
        let ys = seq{for i in int from.y .. to_y - 1 do yield i}
        
        let vectors = seq {
            for x in xs do
                for y in ys do yield new Vector2(float32 x,float32 y)
        }
        
        Seq.toList vectors
       
    
