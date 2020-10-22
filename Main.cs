using Godot;
using System.Linq;
using Godot.Collections;
using nuclearnation;

public class Main : Node2D
{
	private int turn = 0;
	private int totalPopulation = 0;
	private readonly PackedScene houseScene = (PackedScene) ResourceLoader.Load("res://scenes/settlements/House2D.tscn");
	
	private const int Popdelta = 100;
	private const int PopsPerHouse = 10;
	private const int Tilemapcellsize = 32;

	private const int Areaheight = 80;
	private const int Areawidth = 80;

	private readonly Array<Road> roads = new Array<Road>();

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
	AccomodatePopulation(Popdelta, ref totalPopulation, roads);
	DrawRoads(roads);
	roads.Clear();
	turnLabel.BbcodeText = $"[center]Turn {t}[/center]";
  }

  private void AccomodatePopulation(int popDelta, ref int totalPopulation, Array<Road> roads)
  {
	var newTotalPop = totalPopulation + popDelta;
	var existingHouseCount = GetChildren().Cast<Node>().Count(c => c is House);
	var newHousesNeeded = newTotalPop / PopsPerHouse - existingHouseCount;
	PlaceHouses(newHousesNeeded, roads);
  }

  private void PlaceHouses(int newHousesNeeded, Array<Road> array)
  {
	  foreach (var road in roads)
	  {
		  var freeBlocks = _GetFreeBlocks(road);
		  foreach (var block in freeBlocks)
		  {
			  GD.Print($"{block.x}/{block.y}");
		  }
	  }

  }

  private Array<Vector2> _GetFreeBlocks(Road road)
  {
	  var tiles = road.GetTiles();
	  var response = new Array<Vector2>();
	  foreach (var tile in tiles)
	  {
		  if (road.IsVertical())
		  {
			  response.Add(new Vector2(tile.x - 1, tile.y));
			  response.Add(new Vector2(tile.x + 1, tile.y));
		  }
		  else
		  {
			  response.Add(new Vector2(tile.x, tile.y - 1));
			  response.Add(new Vector2(tile.x, tile.y + 1));
		  }
	  }
	  return response;
  }

  private void DrawRoads(Array<Road> roads)
  {
	  foreach (var road in roads)
	  {
		  foreach (var tile in road.GetTiles())
		  {
			  desertTilemap.SetCell((int)tile.x, (int)tile.y, 1);
		  }
	  }
  }
}










	
