namespace FS

open Godot 

type MainFs() as this = 
    inherit Node()
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanel")) :?> Panel)
    let mutable turn = 0
    let updateTurn turn =
        let newTurn = turn+1
        newTurn
    
    override this._Ready() =
        turn <- updateTurn turn

    override this._Process(_) =
        turn <- if Input.IsActionJustPressed("ui_accept") then updateTurn turn else turn
        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


