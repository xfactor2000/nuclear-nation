namespace FS

open Godot 

type MainFs() as this = 
    inherit Node()
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let mutable turn = 0
    let updateTurn turn =
        let newTurn = turn+1
        GD.Print(newTurn)
        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" newTurn
        newTurn
    
    override this._Ready() =
        turn <- updateTurn turn

    override this._Process(delta) =
        turn <- if Input.IsActionJustPressed("ui_accept") then updateTurn turn else turn
        ()

