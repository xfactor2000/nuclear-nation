using System.Collections.Generic;
using Godot;
using System.Linq;
using nuclearnation;
using Godot.Collections;
using nuclearnation.scenes.settlements;


public class Main : MainFs
{
// 	private int turn = 0;
// 	private int totalPopulation = 0;
// 	private readonly PackedScene houseScene = (PackedScene) ResourceLoader.Load("res://scenes/settlements/House2D.tscn");
// 	
// 	private const int Popdelta = 100;
// 	private const int PopsPerHouse = 10;
//
// 	private const int AreaHeight = 80;
// 	private const int AreaWidth = 80;
//
// 	private readonly Array<Road> roads = new Array<Road>();
//
// 	Panel cheatMenu;
// 	RichTextLabel turnLabel;
// 	TileMap desertTilemap;
// 	
// 	public override void _Ready()
// 	{
// 		cheatMenu = GetNode("CheatPanel") as Panel;
// 		turnLabel = GetNode("TurnLabel") as RichTextLabel;
// 		desertTilemap = GetNode("DesertTileMap") as TileMap;
// 		var house = houseScene.Instance() as House;
// 		AddChild(house);
// 		house.Position = new Vector2(2 * desertTilemap.CellSize.x,15*desertTilemap.CellSize.y);
// 		roads.Add(new Road(new Vector2(0, 16), new Vector2(8, 16)));
// 		UpdateTurn(ref turn);
// 	}
//
//   // Called every frame. 'delta' is the elapsed time since the previous frame.
//   public override void _Process(float delta)
//   {
// 	  if (Input.IsActionJustPressed("ui_accept"))
// 	  {
// 		  UpdateTurn(ref turn);
// 	  }
//
// 	  if (Input.IsActionJustPressed("ui_show_cheat_menu"))
// 	  {
// 		  if (cheatMenu.Visible)
// 		  {
// 			  cheatMenu.Hide();
// 		  }
// 		  else
// 		  {
// 			  cheatMenu.Show(); 
// 		  }
// 			  
// 	  }
// 	  
//   }
//
//   private void UpdateTurn(ref int t)
//   {
// 	t += 1;
// 	AccomodatePopulation(Popdelta, ref totalPopulation, roads);
// 	DrawRoads(roads);
// 	roads.Clear();
// 	turnLabel.BbcodeText = $"[center]Turn {t}[/center]";
//   }
//
//   private void AccomodatePopulation(int popDelta, ref int totalPopulation, Array<Road> roads)
//   {
// 	var newTotalPop = totalPopulation + popDelta;
// 	var existingHouseCount = GetChildren().Cast<Node>().Count(c => c is House);
// 	// var newHousesNeeded = newTotalPop / PopsPerHouse - existingHouseCount;
// 	var newHousesNeeded = 1;
// 	for (var i = 0; i < newHousesNeeded; i++)
// 	{
// 		var house = houseScene.Instance() as House;
// 		PlaceBuilding(house, roads);
// 	}
// 	
//   }
//
//   private void PlaceBuilding(Building building, Array<Road> roads)
//   {
// 	  foreach (var road in roads)
// 	  {
// 		  var freeBlocks = _GetFreeBlocks(road,desertTilemap);
// 		  GD.Print("Free cells on the sides of the road");
// 		  foreach (var block in freeBlocks)
// 		  {
// 			  GD.Print($"{block.x}/{block.y}");
// 		  }
// 	  }
//
//   }
//   
//   private List<Vector2> _GetFreeBlocks(Road road, TileMap tileMap)
//   {
// 	  var tiles = road.GetTiles();
// 	  
// 	  //returns list of continuous free cell blocks
// 	  var totalCells = new List<Vector2>();
// 	  var totalCellBlocks = new List<Vector2>();
// 	  var cellList1 = new List<Vector2>(); //left or top
// 	  var cellList2 = new List<Vector2>(); //right or bottom
// 	  
// 	  var existingBuildings = GetChildren().Cast<Node>().Where(n => n is Building).Cast<Building>().ToList();
// 	  foreach (var tile in tiles)
// 	  {
//
// 		  if (road.IsVertical())
// 		  {
// 			  totalCells.Add(new Vector2(tile.x - 1, tile.y));
// 			  totalCells.Add(new Vector2(tile.x + 1, tile.y));
// 		  }
// 		  else
// 		  {
// 			  var topTile = new Vector2(tile.x, tile.y - 1);
// 			  var bottomTile = new Vector2(tile.x, tile.y + 1);
// 			  foreach (var building in existingBuildings)
// 			  {
// 				  var buildingCells = GetCellsTakenByBuilding(building,tileMap);
// 				  if (buildingCells.Contains(topTile))
// 				  {
// 					  totalCellBlocks.AddRange(cellList1);
// 					  cellList1 = new List<Vector2>();
// 				  }
// 				  cellList1.Add(topTile);
// 				  if (buildingCells.Contains(bottomTile))
// 				  {
// 					  totalCellBlocks.AddRange(cellList2);
// 					  cellList2 = new List<Vector2>();
// 				  }
// 				  cellList2.Add(bottomTile);
// 			  }
// 		  }
//
// 		  // totalCells = totalCells.Except(cellsTakenByBuilding).ToList();
// 	  }
// 	  
// 	  GD.Print("Free cells after subtracting existing buildings");
// 	  totalCells.ForEach(c =>
// 	  {
// 		  GD.Print(c);
// 	  });
// 	  
// 	  return totalCells;
//   }
//
//   private List<Vector2> GetCellsTakenByBuilding(Building building, TileMap tileMap)
//   {
// 	  var buildingSizeInTiles = building.GetSizeInTiles((int) tileMap.CellSize.x);
// 	  var buildingPositionInTiles = building.Position / (int) tileMap.CellSize.x;
// 	  var response = new List<Vector2>();
// 	  for (var x = buildingPositionInTiles.x - buildingSizeInTiles.x / 2;
// 		  x < buildingPositionInTiles.x + buildingSizeInTiles.x / 2;
// 		  x++)
// 	  {
// 		  for (var y = buildingPositionInTiles.y - buildingSizeInTiles.y / 2;
// 			  y < buildingPositionInTiles.y + buildingSizeInTiles.y / 2;
// 			  y++)
// 		  {
// 			  response.Add(new Vector2(x,y));
// 		  }
// 	  }
//
// 	  return response;
//   }
//
//   private void DrawRoads(Array<Road> roads)
//   {
// 	  foreach (var road in roads)
// 	  {
// 		  foreach (var tile in road.GetTiles())
// 		  {
// 			  desertTilemap.SetCell((int)tile.x, (int)tile.y, 1);
// 		  }
// 	  }
//   }
}










	
