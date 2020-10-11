extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var turn = 0
onready var cheatMenu = get_node("CheatPanel") as Panel
onready var turnLabel = get_node("TurnLabel") as RichTextLabel

func updateTurn(t = turn):
	turnLabel.bbcode_text = "[center]Turn %s[/center]" % (t +1)

# Called when the node enters the scene tree for the first time.
func _ready():
	updateTurn()



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Input.is_action_just_pressed("ui_accept"):
		turn +=1
		updateTurn()
		
	if Input.is_action_just_pressed("ui_show_cheat_menu"):
		if cheatMenu.visible:
			cheatMenu.hide()
		else:
			cheatMenu.show()
	
