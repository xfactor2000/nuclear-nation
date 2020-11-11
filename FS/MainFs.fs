namespace nuclearnation

open FS.Settlement
open Godot
open System.Linq


type MainFs() as this = 
    inherit Node()

    let mutable turn = 0
    
    let updateTurn turn =
        turn + 1
        
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    
    let populationLabel = lazy(this.GetNode(new NodePath("PopulationLabel")) :?> RichTextLabel)
    
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanelCanvasLayer/CheatPanel")) :?> Panel)
    
    let addPopulationButton = lazy(cheatMenu.Value.GetNode(new NodePath("AddPopulationButton")) :?> Button)
    
    let removePopulationButton = lazy(cheatMenu.Value.GetNode(new NodePath("RemovePopulationButton")) :?> Button)
    
    let settlement:Lazy<Settlement> = lazy(this.GetChildren().Cast<Node>().Where(fun (n)-> n :? Settlement).Cast<Settlement>().First())
    
    let mutable totalPopulation = 0
    
    let mutable accumulatedPopulationDuringTurn = 0
    
    let _onAddPopulationButtonPressed() =
        accumulatedPopulationDuringTurn <- accumulatedPopulationDuringTurn + 10
        
    let _onRemovePopulationButtonPressed() =
        accumulatedPopulationDuringTurn <- accumulatedPopulationDuringTurn - 10
        
    override this._Ready() =
        addPopulationButton.Value.Connect("pressed",this,"_onAddPopulationButtonPressed") |> ignore
        removePopulationButton.Value.Connect("pressed",this,"_onRemovePopulationButtonPressed") |> ignore
        this.AddChild(new Settlement())
//        let house = houseScene.Instance() :?> HouseFs;
//        this.AddChild(house);
//        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y)
//        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 40, float32 16)))
//        this.AddChild(new Road(Vector2(float32 20, float32 10), Vector2(float32 20, float32 30)))
//        this.AddChild(new Road(Vector2(float32 30, float32 10), Vector2(float32 30, float32 30)))
//        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 4, float32 16)))
//        this.AddChild(new Road(Vector2(float32 4, float32 17), Vector2(float32 4, float32 20)))
        turn <- updateTurn turn

    override this._Process(_) =
        turn <-
            if Input.IsActionJustPressed("ui_accept") then
                totalPopulation<-totalPopulation + settlement.Value.AddPopulation(accumulatedPopulationDuringTurn)
                accumulatedPopulationDuringTurn <- 0
                updateTurn turn
            else
                turn

        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        populationLabel.Value.BbcodeText <- sprintf "[center] %d [/center]" totalPopulation
        
        settlement.Value.Redraw()
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


