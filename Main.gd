extends Node2D


var turn = 0
onready var cheatMenu = get_node("CheatPanel") as Panel
onready var turnLabel = get_node("TurnLabel") as RichTextLabel

var addPop = 100
var area  = Array()
const AREAHEIGHT = 80
const AREAWIDTH = 80

enum TileContent {EMPTY=0, PATH=1, HOUSE=2}

func accomodatePopulation(pop:int):
	pass

func updateTurn(t = turn):
	turnLabel.bbcode_text = "[center]Turn %s[/center]" % (t +1)

# Called when the node enters the scene tree for the first time.
func _ready():
	updateTurn()
	#initializing area data
	for y in range(AREAHEIGHT):
		var new_row:PoolIntArray = []
		for x in range(AREAWIDTH):
			new_row.append(TileContent.EMPTY)
		area.append(new_row)




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
	
