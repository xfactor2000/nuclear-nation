extends Node2D

var turn = 0
var totalPopulation=0

onready var cheatMenu = get_node("CheatPanel") as Panel
onready var turnLabel = get_node("TurnLabel") as RichTextLabel

const POPDELTA = 100
const POPSPERHOUSE=10


var area  = Array()
const AREAHEIGHT = 80
const AREAWIDTH = 80

class Road:
	var from:Vector2
	var to: Vector2
	
	func _init(from:Vector2, to:Vector2):
		self.from=from
		self.to=to
		
class House:
	var topLeftCoords:Vector2
	func _init(topLeftCoords:Vector2):
		self.topLeftCoords = topLeftCoords

class AccomodatePopulationResponse:
	var newTotalPopulation: int
	var newRoads: Array
	var newHouses: Array
	func _init(newTotalPopulation:int, newRoads: Array,newHouses:Array):
		self.newTotalPopulation = newTotalPopulation
		self.newRoads = newRoads
		self.newHouses = newHouses

var roads = Array()
var houses = Array()

func accomodatePopulation(pop:int,totalPopulation:int,roads:Array,houses:Array)->AccomodatePopulationResponse:
	var newTotalPop = totalPopulation + pop
	var existingHouseCount = houses.size()
	var newHousesNeeded = newTotalPop / POPSPERHOUSE - existingHouseCount
	print(newHousesNeeded)
	#place the houses along the existing roads or allocate new ones
	return AccomodatePopulationResponse.new(newTotalPop,roads,houses)


func updateTurn(t = turn):
	accomodatePopulation(POPDELTA, totalPopulation, roads, houses)
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
	
