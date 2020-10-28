namespace nuclearnation

open FS.Settlement
open Godot
open System.Linq

type MainFs() as this = 
    inherit Node()
    
    [<Literal>]
    let PopsPerHouse = 10
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let desertTilemap = lazy(this.GetNode(new NodePath("DesertTileMap")) :?> TileMap)
    let houseScene = ResourceLoader.Load("res://scenes/settlements/House2D.tscn") :?> PackedScene
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanel")) :?> Panel)
    let mutable turn = 0
    
    let mutable totalPopulation = 0
    
    member private this.UpdateTurn(turn) =
        turn + 1
    
    member private this.DrawRoads() =
        let roads = this.GetChildren().Cast<Node>().Where(fun (n)-> n :? Road).Cast<Road>().ToList()
        for road in roads do
            for tile in road.GetTiles() do
                desertTilemap.Value.SetCell(int tile.x, int tile.y, 1)
    
    override this._Ready() =
        let house = houseScene.Instance() :?> HouseFs;
        this.AddChild(house);
        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y);
        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 8, float32 16)))
        this.DrawRoads()
        turn <- this.UpdateTurn(turn)

    override this._Process(_) =
        turn <- if Input.IsActionJustPressed("ui_accept") then this.UpdateTurn(turn) else turn
        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


