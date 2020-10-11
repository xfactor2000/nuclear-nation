extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Input.is_action_just_pressed("ui_show_cheat_menu"):
		var cheatMenu = get_node("CheatPanel") as Panel
		if cheatMenu.visible:
			cheatMenu.hide()
		else:
			cheatMenu.show()
	
