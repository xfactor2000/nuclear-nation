namespace nuclearnation

open FS.Settlement
open Godot
open System.Linq


type MainFs() as this = 
    inherit Node()

    let mutable turn = 0

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
    
        
    let updateTurn turn =
        do totalPopulation<-totalPopulation + settlement.Value.AddPopulation(accumulatedPopulationDuringTurn)
        do accumulatedPopulationDuringTurn <- 0
        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" (turn + 1)
        populationLabel.Value.BbcodeText <- sprintf "[center] %d [/center]" totalPopulation
        settlement.Value.Redraw()
        this.EmitSignal("turn_complete")
        turn + 1
        
        
    override this._Ready() =
        this.AddUserSignal("turn_complete")
        addPopulationButton.Value.Connect("pressed",this,"_onAddPopulationButtonPressed") |> ignore
        removePopulationButton.Value.Connect("pressed",this,"_onRemovePopulationButtonPressed") |> ignore
        let settlement = new Settlement()
        this.AddChild(settlement)
        accumulatedPopulationDuringTurn<-300
        turn <- updateTurn turn

    override this._Process(_) =
        if Input.IsActionJustPressed("ui_accept") then
            turn <- updateTurn turn
           
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


