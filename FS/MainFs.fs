namespace nuclearnation

open FS.Settlement
open Godot
open System.Linq

type MainFs() as this = 
    inherit Node()
    
    [<Literal>]
    let PopsPerHouse = 10
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let desertTilemap = lazy(this.GetNode(new NodePath("DesertTileMap")) :?> TileMap)
    let houseScene = ResourceLoader.Load("res://scenes/settlements/House2D.tscn") :?> PackedScene
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanel")) :?> Panel)
    let mutable turn = 0
    
    let mutable totalPopulation = 0
    
    let updateTurn turn =
        turn + 1
    
    let getRoads() =
        this.GetChildren().Cast<Node>().Where(fun (n)-> n :? Road).Cast<Road>().ToList()
        
    let getBuildings() =
        this.GetChildren().Cast<Node>().Where(fun (n)-> n :? BuildingFs).Cast<BuildingFs>().ToList()
    
    let getCellsTakenByBuilding (building:BuildingFs, tileMap:TileMap) =
        let buildingSizeInTiles = building.GetSizeInTiles(int tileMap.CellSize.x)
        let buildingPositionInTiles = building.Position / (tileMap.CellSize.x)
        
        let xs = seq{for x in buildingPositionInTiles.x - buildingSizeInTiles.x / 2.0f .. buildingPositionInTiles.x + buildingSizeInTiles.x / 2.0f - 1.0f do yield x}
        let ys = seq{for y in buildingPositionInTiles.y - buildingSizeInTiles.y / 2.0f .. buildingPositionInTiles.y + buildingSizeInTiles.y / 2.0f - 1.0f do yield y}
        
        let response = seq {
            for x in xs do
                for y in ys do yield Vector2(float32 x,float32 y)
        }
        Seq.toList response

    
    let getCellsTakenByAllBuildings(tileMap:TileMap) =
        let totalCellsTaken =
            seq{
                for building in getBuildings() do yield getCellsTakenByBuilding(building,tileMap)
            }
        totalCellsTaken |> Seq.concat
            
    let drawRoads() =
        for road in getRoads() do
            for tile in road.GetTiles() do
                desertTilemap.Value.SetCell(int tile.x, int tile.y, 1)
    
   
    
    
    let putHouseOnTheMap() =
        let road = getRoads().First()
        let freeTiles = road.GetFreeTilesAlong()
        let house = houseScene.Instance() :?> HouseFs
        let houseWidth = int (house.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x)
        let cellsTakenByBuildings = getCellsTakenByAllBuildings(desertTilemap.Value)
        
        let findPlacesToBuild (buildingWidth,cells:List<Vector2>):List<List<Vector2>> =
            let existsCheck(list1:List<Vector2>,list2:List<Vector2>):bool = not(list2.Except(list1).Any())
            let rec tailRecursiveFindPlaces(buildingWidth,cells:List<Vector2>,acc:List<List<Vector2>>):List<List<Vector2>> =
                match cells with
                    | [] -> acc
                    | head::tail -> 
                        let startX = int head.x
                        let endX = startX + buildingWidth - 1
                        //need to check that the cells with given xs range exist
                            //if yes - add them to accumulator and continue to the next cell
                            //if no - continue to the next cell
                        let rangeToLookFor =
                            Seq.toList(seq{
                                for x in [startX..endX] do yield Vector2(float32 x,head.y)
                            })
                        if existsCheck(cells,rangeToLookFor) then
                            tailRecursiveFindPlaces(buildingWidth,tail,Seq.toList(acc.Append(rangeToLookFor)))
                        else
                            tailRecursiveFindPlaces(buildingWidth,tail,acc)
            tailRecursiveFindPlaces(buildingWidth,cells,[])
        
        let getRandomBuildingPlace (availableCells:List<List<Vector2>>) =  
            let rand = System.Random()
            if (availableCells.Length >0) then
                availableCells.[rand.Next(availableCells.Length)]
            else
                []
        
        //trying to place a house on one side of the road
        if (road.IsVertical()  <> true) then
            let (topSide,bottomSide) = freeTiles
            let availableCellsTopSide = Seq.toList(topSide.Except(cellsTakenByBuildings).OrderBy(fun c->c.x))
            let availableCellsBottomSide = Seq.toList(bottomSide.Except(cellsTakenByBuildings).OrderBy(fun c->c.x))
            let placesToBuildTopSide = findPlacesToBuild(houseWidth,Seq.toList(availableCellsTopSide))
            let placesToBuildBottomSide = findPlacesToBuild(houseWidth,Seq.toList(availableCellsBottomSide))
            let randomPlace = getRandomBuildingPlace(placesToBuildTopSide @ placesToBuildBottomSide)
            if (not(randomPlace.IsEmpty)) then
                this.AddChild(house)
                let (yPosition,house) =
                    if (randomPlace.Head.y<road.from.y) then
                        (float32 randomPlace.Head.y * desertTilemap.Value.CellSize.y,house)
                    else
                        do house.Rotate(3.14159f)
                        (float32 (randomPlace.Head.y + 1.0f)* desertTilemap.Value.CellSize.y,house)
                house.Position <- Vector2(
                                          float32 (randomPlace.Head.x + house.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x / 2.0f) * desertTilemap.Value.CellSize.x,
                                          yPosition
                                          )
         else
             ()
                
        ()        
        
        
    override this._Ready() =
        let house = houseScene.Instance() :?> HouseFs;
        this.AddChild(house);
        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y);
        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 32, float32 16)))
        drawRoads()
        turn <- updateTurn turn

    override this._Process(_) =
        turn <-
            if Input.IsActionJustPressed("ui_accept") then
                do putHouseOnTheMap()
                updateTurn turn
            else
                turn

        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


