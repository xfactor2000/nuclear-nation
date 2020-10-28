namespace nuclearnation

open FS.Settlement
open Godot

type MainFs() as this = 
    inherit Node()
    
    [<Literal>]
    let PopsPerHouse = 10
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let desertTilemap = lazy(this.GetNode(new NodePath("DesertTileMap")) :?> TileMap)
    let houseScene = ResourceLoader.Load("res://scenes/settlements/House2D.tscn") :?> PackedScene
    let house = houseScene.Instance() :?> BuildingFs;
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanel")) :?> Panel)
    let mutable turn = 0
    
    let mutable totalPopulation = 0
    
    let roads: List<Road> = []
    
    let updateTurn turn =
        turn + 1
    
    override this._Ready() =
        let house = houseScene.Instance() :?> HouseFs;
        this.AddChild(house);
        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y);
//        this.AddChild(new Road(new Vector2(0, 16), new Vector2(8, 16)));
        turn <- updateTurn turn

    override this._Process(_) =
        turn <- if Input.IsActionJustPressed("ui_accept") then updateTurn turn else turn
        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


