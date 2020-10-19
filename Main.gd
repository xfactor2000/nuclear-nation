extends Node2D

var turn = 0
var totalPopulation=0

onready var cheatMenu = get_node("CheatPanel") as Panel
onready var turnLabel = get_node("TurnLabel") as RichTextLabel
onready var desertTilemap = get_node("DesertTileMap") as TileMap

const POPDELTA = 100
const POPSPERHOUSE=10

var area  = Array()
const AREAHEIGHT = 80
const AREAWIDTH = 80

class Road:
	var from:Vector2
	var to: Vector2
	
	func isVertical()->bool:
		return (from.x == to.x)
	
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
	#print(newHousesNeeded)
	#place the houses along the existing roads or allocate new ones
	return AccomodatePopulationResponse.new(newTotalPop,roads,houses)


func updateTurn(t = turn):
	accomodatePopulation(POPDELTA, totalPopulation, roads, houses)
	drawRoads(roads)
	turnLabel.bbcode_text = "[center]Turn %s[/center]" % (t +1)

func drawRoads(roads: Array):
	for road in roads:
		var to_x = road.to.x
		if (to_x == road.from.x):
			to_x +=1
		
		var to_y = road.to.y
		if (to_y == road.from.y):
			to_y +=1
		
		for x in range(road.from.x,to_x):
			for y in range(road.from.y,to_y):
				print(x)
				print(y)
				desertTilemap.set_cell(x,y,1)
	pass

# Called when the node enters the scene tree for the first time.
func _ready():
	roads.append(Road.new(Vector2(0,16),Vector2(32,16)))
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
	
