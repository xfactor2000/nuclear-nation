using Godot;
using System.Linq;
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
		var house = houseScene.Instance() as House;
		AddChild(house);
		house.Position = new Vector2(10 * desertTilemap.CellSize.x,15*desertTilemap.CellSize.y);
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
	AccomodatePopulation(Popdelta, totalPopulation, roads);
	DrawRoads(roads);
	turnLabel.BbcodeText = $"[center]Turn {t}[/center]";
  }

  private void AccomodatePopulation(int popDelta, int totalPopulation, Array<Road> roads)
  {
	var newTotalPop = totalPopulation + popDelta;
	int existingHouseCount = GetChildren().Cast<Node>().Count(c => c is House);
	GD.Print(existingHouseCount);
	// var newHousesNeeded = newTotalPop / POPSPERHOUSE - existingHouseCount
	// placeHouses(newHousesNeeded,roads,houses)
	// return AccomodatePopulationResponse.new(newTotalPop,roads,houses)
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
//



	
