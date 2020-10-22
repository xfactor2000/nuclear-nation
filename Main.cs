using Godot;
using System;
using Godot.Collections;
using nuclearnation;

public class Main : Node2D
{
	private int turn = 0;
	private int totalPopulation = 0;
	private PackedScene houseScene = (PackedScene) ResourceLoader.Load("res://scenes/settlements/House2D.tscn");
	
	private const int Popdelta = 100;
	private const int Popsperhouse = 10;
	private const int Tilemapcellsize = 32;

	private const int Areaheight = 80;
	private const int Areawidth = 80;

	private Array<Road> roads = new Array<Road>();

	Panel cheatMenu;
	RichTextLabel turnLabel;
	TileMap desertTilemap;
	
	public override void _Ready()
	{
		cheatMenu = GetNode("CheatPanel") as Panel;
		turnLabel = GetNode("TurnLabel") as RichTextLabel;
		desertTilemap = GetNode("DesertTileMap") as TileMap;
		roads.Add(new Road(new Vector2(0, 16), new Vector2(32, 16)));
		UpdateTurn(ref turn);
	}

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
	  if (Input.IsActionJustPressed("ui_accept"))
	  {
		  UpdateTurn(ref turn);
	  }

	  if (Input.IsActionJustPressed("ui_show_cheat_menu"))
	  {
		  if (cheatMenu.Visible)
		  {
			  cheatMenu.Hide();
		  }
		  else
		  {
			  cheatMenu.Show(); 
		  }
			  
	  }
	  
  }

  private void UpdateTurn(ref int t)
  {
	  t += 1;
	  // accomodatePopulation(POPDELTA, totalPopulation, roads, houses)
	  DrawRoads(roads);
	  // drawHouses(houses)
	  turnLabel.BbcodeText = $"[center]Turn {t}[/center]";
  }

  private void DrawRoads(Array<Road> roads)
  {
	  foreach (var road in roads)
	  {
		  foreach (var tile in road.getTiles())
		  {
			  desertTilemap.SetCell((int)tile.x, (int)tile.y, 1);
		  }
	  }
  }
}




//
// var houseSizeTiles = (houseScene.instance().get_node("ActiveHouse") as Sprite).texture.get_size() / TILEMAPCELLSIZE
//
// var area  = Array()

//

// 	
// 	func _init(from:Vector2, to:Vector2):
// 		self.from=from
// 		self.to=to
//
// 		
// class House:
// 	var center_coords:Vector2
// 	func _init(center_coords:Vector2):
// 		self.center_coords = center_coords
//
// class AccomodatePopulationResponse:
// 	var newTotalPopulation: int
// 	var newRoads: Array
// 	var newHouses: Array
// 	func _init(newTotalPopulation:int, newRoads: Array,newHouses:Array):
// 		self.newTotalPopulation = newTotalPopulation
// 		self.newRoads = newRoads
// 		self.newHouses = newHouses
//
// var roads = Array()
// var houses = Array()
//
// func accomodatePopulation(pop:int,totalPopulation:int,roads:Array,houses:Array)->AccomodatePopulationResponse:
// 	var newTotalPop = totalPopulation + pop
// 	var existingHouseCount = houses.size()
// 	var newHousesNeeded = newTotalPop / POPSPERHOUSE - existingHouseCount
// 	placeHouses(newHousesNeeded,roads,houses)
// 	return AccomodatePopulationResponse.new(newTotalPop,roads,houses)
//
// func _getFreeBlocks(road:Road, houses:Array)->PoolVector2Array:
// 	var tiles = road.getTiles()
// 	var response = PoolVector2Array()
// 	for tile in tiles:
// 		if road.isVertical():
// 			var is_house_found = false
// 			for house in houses:
// 				var center = house.ce
// 				pass
// 			response.append(Vector2(tile.x-1,tile.y))
// 			response.append(Vector2(tile.x+1, tile.y))
// 		else:
// 			response.append(Vector2(tile.x,tile.y-1))
// 			response.append(Vector2(tile.x,tile.y+1))
// 	return response
//
// func placeHouses(newHouses:int,roads:Array,houses:Array): #returns array of vectors
// 	for road in roads:
// 		var freeBlocks = _getFreeBlocks(road,houses)
// 		for block in freeBlocks:
// 			print("%s/%s" % [block.x, block.y])
// 	
// func updateTurn(t = turn):
// 	accomodatePopulation(POPDELTA, totalPopulation, roads, houses)
// 	drawRoads(roads)
// 	drawHouses(houses)
// 	turnLabel.bbcode_text = "[center]Turn %s[/center]" % (t +1)
//
// func drawRoads(roads: Array):
// 	for road in roads:
// 		for tile in road.getTiles():
// 			desertTilemap.set_cell(tile.x,tile.y,1)
//
// func drawHouses(houses: Array):
// 	for house in houses:
// 		var node = houseScene.instance() as Node2D
// 		node.set_position(Vector2((house.center_coords.x + 1) * desertTilemap.cell_size.x,(house.center_coords.y + 1) * desertTilemap.cell_size.y))
// 		desertTilemap.add_child(node)
// 		
//
// # Called when the node enters the scene tree for the first time.
// func _ready():
// 	print(houseSizeTiles)
// 	roads.append(Road.new(Vector2(0,16),Vector2(32,16)))
// 	houses.append(House.new(Vector2(10,14)))
// #	roads.append(Road.new(Vector2(32,16),Vector2(32,24)))
// 	updateTurn()


	
