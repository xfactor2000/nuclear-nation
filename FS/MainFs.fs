namespace nuclearnation

open System
open FS.Settlement
open Godot
open System.Linq

type MainFs() as this = 
    inherit Node()
    
    [<Literal>] 
    let PopsPerHouse = 10
    
    [<Literal>]
    let AreaHeightCells = 32;
    [<Literal>]
    let AreaWidthCells = 48;
    
    let turnLabel = lazy(this.GetNode(new NodePath("TurnLabel")) :?> RichTextLabel)
    let desertTilemap = lazy(this.GetNode(new NodePath("DesertTileMap")) :?> TileMap)
    let houseScene = ResourceLoader.Load("res://scenes/settlements/House2D.tscn") :?> PackedScene
    let cheatMenu = lazy(this.GetNode(new NodePath("CheatPanel")) :?> Panel)
    let mutable turn = 0
    
    let mutable totalPopulation = 0
    
    let updateTurn turn =
        turn + 1
    
    let getRoads() =
        Seq.toList(this.GetChildren().Cast<Node>().Where(fun (n)-> n :? Road).Cast<Road>())
        
    let getBuildings() =
         Seq.toList(this.GetChildren().Cast<Node>().Where(fun (n)-> n :? BuildingFs).Cast<BuildingFs>().ToList())
    
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
        Seq.toList(totalCellsTaken |> Seq.concat)
            
    let drawRoads() =
        for road in getRoads() do
            for tile in road.GetTiles() do
                do desertTilemap.Value.SetCell(int tile.x, int tile.y, 1)
    
   
    let isVerticallyDirected (cells:List<Vector2>) =
            let xsLength = List.length (cells |> List.distinctBy(fun(c)->c.x))
            let ysLength = List.length (cells |> List.distinctBy(fun(c)->c.y))
            ysLength > xsLength
    
    /// <summary>
    /// This function returns Building object in case it was placed (and rotated), None if nothing
    /// </summary>
    /// <returns>returns Building object in case it was placed (and rotated), None if nothing</returns>
    let tryPutBuildingOnMap(building:BuildingFs):Option<BuildingFs> =
        let freeTiles(road:Road) = road.GetFreeTilesAlong()
        let cellsTakenByBuildings = getCellsTakenByAllBuildings(desertTilemap.Value)
        
        let findPlacesToBuildAlongRoad (road:Road,building:BuildingFs,cellsTakenByBuildings:List<Vector2>,cellsAlongRoad:List<Vector2>):List<List<Vector2>> =
            let existsCheck(list1:List<Vector2>,list2:List<Vector2>):bool = not(list2.Except(list1).Any())
            let isVertical = isVerticallyDirected(cellsAlongRoad)
            let rec tailRecursiveFindPlaces(building:BuildingFs,cellsAlongRoad:List<Vector2>,acc:List<List<Vector2>>):List<List<Vector2>> =
                let buildingWidth = int(building.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x) 
                let buildingHeight = int(building.GetSizeInTiles(int desertTilemap.Value.CellSize.y).y) 
                let getHypotheticalBuildingCells(road:Road, suggestedCellsAlongsideRoad: List<Vector2>):Option<List<Vector2>>=
                    let directionToBuildCells = 
                        if road.IsVertical() then
                            if suggestedCellsAlongsideRoad.Head.x< road.from.x then
                                -1
                            else
                                1
                        else
                            if suggestedCellsAlongsideRoad.Head.y< road.from.y then
                                -1
                            else
                                1
                     
                    let (xFrom:int,xTo:int) =
                       if road.IsVertical() then
                            let startX = int(suggestedCellsAlongsideRoad.Head.x)
                            let stopX = int(suggestedCellsAlongsideRoad.Head.x)+(directionToBuildCells*(buildingHeight-1))
                            (startX,stopX)
                       else
                            //we want to leave at least one extra space on both sides of the building to accomodate for roads and such
                            let startX:int = int(suggestedCellsAlongsideRoad.Head.x)-1
                            let stopX:int =  int((suggestedCellsAlongsideRoad |> List.last).x)+1
                            (startX,stopX)
                    let (yFrom,yTo) =
                       if road.IsVertical() then
                            //we want to leave at least one extra space on both sides of the building to accomodate for roads and such
                            let startY = int(suggestedCellsAlongsideRoad.Head.y)-1
                            let stopY = int((suggestedCellsAlongsideRoad |> List.last).y)+1
                            (startY,stopY)
                       else
                            //we want to leave at least one extra space on both sides of the building to accomodate for roads and such
                            let startY = int(suggestedCellsAlongsideRoad.Head.y)
                            let stopY = int(suggestedCellsAlongsideRoad.Head.y)+(directionToBuildCells*(buildingHeight-1))
                            (startY,stopY)
                            
                    let startCellX = if xFrom < xTo then xFrom else xTo
                    let stopCellX = if xFrom < xTo then xTo else xFrom        
                    let startCellY = if yFrom < yTo then yFrom else yTo
                    let stopCellY = if yFrom < yTo then yTo else yFrom
                   
                    //if the building turns out to be outside of the map - do not place it
                    if startCellX<0 || stopCellX>AreaWidthCells || startCellY <0 || stopCellY>AreaHeightCells then
                        None 
                    else
                        let cells =
                            seq{
                                for x in [startCellX..stopCellX] do
                                    for y in [startCellY..stopCellY] do
                                        yield Vector2(float32 x,float32 y)
                                
                            }
                            
                        Some(Seq.toList(cells))
                    
                match cellsAlongRoad with
                    | [] -> acc
                    | head::tail ->
                        let (startX,endX) =
                            if (isVertical) then
                                (int head.x, int head.x)
                            else
                                (int head.x,int head.x + (buildingWidth-1))
                        let (startY,endY) =
                            if (isVertical) then
                                (int head.y, int head.y + (buildingWidth-1)) //we always check width because the building will be rotated when placed alongside vertical roads
                            else
                                (int head.y,int head.y)
                        //need to check that the cells with given xs range exist alonside the road
                            //if yes - add them to accumulator and continue to the next cell
                            //if no - continue to the next cell
                        let suggestedCellsAlongRoad =
                            Seq.toList(seq{
                                for x in [startX..endX] do
                                    for y in [startY..endY] do
                                        yield Vector2(float32 x,float32 y)
                            })
                        let hypotheticalBuildingCells = getHypotheticalBuildingCells(road,suggestedCellsAlongRoad)
                        match hypotheticalBuildingCells with
                            | None ->  tailRecursiveFindPlaces(building,tail,acc)
                            | Some hCells ->
                                    if (existsCheck(cellsAlongRoad,suggestedCellsAlongRoad) && (hCells.Count() = (hypotheticalBuildingCells.Value |> List.except cellsTakenByBuildings).Count())) then
                                        tailRecursiveFindPlaces(building,tail,Seq.toList(acc.Append(suggestedCellsAlongRoad)))
                                    else
                                        tailRecursiveFindPlaces(building,tail,acc)
            tailRecursiveFindPlaces(building,cellsAlongRoad,[])
        
        let getRandomBuildingPlace(availableCells:List<List<Vector2>>):Option<List<Vector2>> =  
            let rand = Random()
            if (availableCells.Length >0) then
                Some(availableCells.[rand.Next(availableCells.Length)])
            else
                None
        //trying to place a house on one side of the road
        let tryPlaceBuilding(building:BuildingFs, road:Road):Option<Vector2> =
            let allTilesAlongRoad = fst (freeTiles road) @ snd (freeTiles road)
            let isVertical = isVerticallyDirected(allTilesAlongRoad)
            let allAvailableCells =
                allTilesAlongRoad
                |> List.except cellsTakenByBuildings
                |> List.sortBy (fun(c)->(if isVertical then c.x else c.y))
            let placesToBuild = findPlacesToBuildAlongRoad(road,building,cellsTakenByBuildings,Seq.toList(allAvailableCells))
            let randomPlace = getRandomBuildingPlace placesToBuild
            let buildingCoords: Option<Tuple<BuildingFs,Vector2>> = 
                match randomPlace with
                    | Some place ->
                        do this.AddChild(building)
                        if (road.IsVertical()) then
                            let yPosition = float32 (place.Head.y + building.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x / 2.0f) * desertTilemap.Value.CellSize.x
                            if (place.Head.x<road.from.x) then
                                do building.Rotate(-3.14159f/2.0f)
                                let xPosition =  float32 place.Head.x * desertTilemap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                            else
                                do building.Rotate(3.14159f/2.0f)
                                let xPosition =  (float32 place.Head.x + 1.0f) * desertTilemap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                        else
                            let xPosition = float32 (place.Head.x + building.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x / 2.0f) * desertTilemap.Value.CellSize.x
                            if (place.Head.y<road.from.y) then
                                let yPosition =  float32 place.Head.y * desertTilemap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                            else
                                let yPosition = float32 (place.Head.y + 1.0f)* desertTilemap.Value.CellSize.y
                                building.Rotate(3.14159f)
                                Some (building,Vector2(xPosition, yPosition))
                            
                            
                    | None -> None
            match buildingCoords with
                | Some (_,coords) ->
                    do building.Position <- coords
                    Some(coords)
                | None -> None        
        
        let tryRoad:Option<Tuple<BuildingFs,Road>> =
            let roads = getRoads()
            let roadsIndices =
                Seq.toList(seq{0..roads.Count()-1})
                |> List.sortBy(fun(_)-> Guid.NewGuid())
            if (roadsIndices.Count() > 0) then
                let randomRoad =
                    roadsIndices
                    |> List.tryPick(fun(index)->
                        //Here, it must return "Some" if the house was placed or "None" if it was not placed
                        let coords = tryPlaceBuilding(building,roads.[index])
                        match coords with
                            | Some _ -> Some(building,roads.[index])
                            | None->None
                    )
                randomRoad
            else
                //TODO - if there are no roads, one must be generated
                Some(building,roads.First())
        
        match tryRoad with
            | Some(building,_) -> Some(building)
            | None -> None
        
        
    override this._Ready() =
        let house = houseScene.Instance() :?> HouseFs;
        this.AddChild(house);
        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y);
        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 32, float32 16)))
        this.AddChild(new Road(Vector2(float32 32, float32 17), Vector2(float32 32, float32 30)))
        drawRoads()
        turn <- updateTurn turn

    override this._Process(_) =
        turn <-
            if Input.IsActionJustPressed("ui_accept") then
                let result = tryPutBuildingOnMap(houseScene.Instance() :?> HouseFs)
                match result with 
                    | Some building ->
                        let message = sprintf "Placed house %f %f" building.Position.x building.Position.y
                        GD.Print(message)
                    | None -> GD.Print("No place left")
                updateTurn turn
            else
                turn

        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


