extends Node2D

var houseScene = preload("res://scenes/settlements/House2D.tscn")

var turn = 0
var totalPopulation=0

onready var cheatMenu = get_node("CheatPanel") as Panel
onready var turnLabel = get_node("TurnLabel") as RichTextLabel
onready var desertTilemap = get_node("DesertTileMap") as TileMap

const POPDELTA = 100
const POPSPERHOUSE=10
const TILEMAPCELLSIZE=32

var houseSizeTiles = (houseScene.instance().get_node("ActiveHouse") as Sprite).texture.get_size() / TILEMAPCELLSIZE

var area  = Array()
const AREAHEIGHT = 80
const AREAWIDTH = 80

class Road:
	var from:Vector2
	var to: Vector2
	
	func getTiles() -> PoolVector2Array:
		var to_x = to.x
		if (to_x == from.x):
			to_x += 1
		
		var to_y = to.y
		if (to_y == from.y):
			to_y +=1
		
		var response = PoolVector2Array()
		for x in range(from.x,to_x):
			for y in range(from.y,to_y):
				response.append(Vector2(x,y))
				
		return response
	
	func isVertical()->bool:
		return (from.x == to.x)
	
	func _init(from:Vector2, to:Vector2):
		self.from=from
		self.to=to

		
class House:
	var center_coords:Vector2
	func _init(center_coords:Vector2):
		self.center_coords = center_coords

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
	placeHouses(newHousesNeeded,roads,houses)
	return AccomodatePopulationResponse.new(newTotalPop,roads,houses)

func _getFreeBlocks(road:Road, houses:Array)->PoolVector2Array:
	var tiles = road.getTiles()
	var response = PoolVector2Array()
	for tile in tiles:
		if road.isVertical():
			var is_house_found = false
			for house in houses:
				var center = house.ce
				pass
			response.append(Vector2(tile.x-1,tile.y))
			response.append(Vector2(tile.x+1, tile.y))
		else:
			response.append(Vector2(tile.x,tile.y-1))
			response.append(Vector2(tile.x,tile.y+1))
	return response

func placeHouses(newHouses:int,roads:Array,houses:Array): #returns array of vectors
	for road in roads:
		var freeBlocks = _getFreeBlocks(road,houses)
		for block in freeBlocks:
			print("%s/%s" % [block.x, block.y])
	
func updateTurn(t = turn):
	accomodatePopulation(POPDELTA, totalPopulation, roads, houses)
	drawRoads(roads)
	drawHouses(houses)
	turnLabel.bbcode_text = "[center]Turn %s[/center]" % (t +1)

func drawRoads(roads: Array):
	for road in roads:
		for tile in road.getTiles():
			desertTilemap.set_cell(tile.x,tile.y,1)

func drawHouses(houses: Array):
	for house in houses:
		var node = houseScene.instance() as Node2D
		node.set_position(Vector2((house.center_coords.x + 1) * desertTilemap.cell_size.x,(house.center_coords.y + 1) * desertTilemap.cell_size.y))
		desertTilemap.add_child(node)
		

# Called when the node enters the scene tree for the first time.
func _ready():
	print(houseSizeTiles)
	roads.append(Road.new(Vector2(0,16),Vector2(32,16)))
	houses.append(House.new(Vector2(10,14)))
#	roads.append(Road.new(Vector2(32,16),Vector2(32,24)))
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
	
