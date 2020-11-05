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
    
    let getCellsTakenByRoads() =
        Seq.toList(seq{
            for road in getRoads() do
                for tile in road.GetTiles() do
                    yield(Vector2(tile.x,tile.y))
        })
            
    let drawRoads() =
        for road in getRoads() do
            for tile in road.GetTiles() do
                do desertTilemap.Value.SetCell(int tile.x, int tile.y, 1)
    
   
    
    /// <summary>
    /// This function returns Building object in case it was placed (and rotated), None if nothing
    /// </summary>
    /// <returns>returns Building object in case it was placed (and rotated), None if nothing</returns>
    let tryPutBuildingOnMap(building:BuildingFs):Option<BuildingFs> =
        let freeTiles(road:Road) = road.GetFreeTilesAlong()
        let cellsTakenByBuildings = getCellsTakenByAllBuildings(desertTilemap.Value)
        let cellsTakenByRoads = getCellsTakenByRoads()
        
        let findPlacesToBuildAlongRoad (road:Road,building:BuildingFs,cellsTakenByBuildings:List<Vector2>,cellsAlongRoad:List<Vector2>):List<List<Vector2>> =
            let existsCheck(list1:List<Vector2>,list2:List<Vector2>):bool = not(list2.Except(list1).Any())
            let rec tailRecursiveFindPlaces(building:BuildingFs,cellsAlongRoad:List<Vector2>,acc:List<List<Vector2>>):List<List<Vector2>> =
                let buildingWidth = int(building.GetSizeInTiles(int desertTilemap.Value.CellSize.x).x) 
                let buildingHeight = int(building.GetSizeInTiles(int desertTilemap.Value.CellSize.y).y) 
                let getHypotheticalBuildingCellsWithMargins(road:Road, suggestedCellsAlongsideRoad: List<Vector2>):Option<List<Vector2>>=
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
                            let startX:int = int(suggestedCellsAlongsideRoad.Head.x)
                            let stopX:int = int((suggestedCellsAlongsideRoad |> List.last).x)
                            (startX,stopX)
                    let (yFrom,yTo) =
                       if road.IsVertical() then
                            let startY = int(suggestedCellsAlongsideRoad.Head.y)
                            let stopY = int((suggestedCellsAlongsideRoad |> List.last).y)
                            (startY,stopY)
                       else
                            let startY = int(suggestedCellsAlongsideRoad.Head.y)
                            let stopY = int(suggestedCellsAlongsideRoad.Head.y)+(directionToBuildCells*(buildingHeight-1))
                            (startY,stopY)
                            
                    let startCellX = if xFrom < xTo then xFrom else xTo
                    let stopCellX = if xFrom < xTo then xTo else xFrom        
                    let startCellY = if yFrom < yTo then yFrom else yTo
                    let stopCellY = if yFrom < yTo then yTo else yFrom
                    
                    let startCellXWithMargins =
                        if road.IsVertical() then startCellX else startCellX-1
                    
                    let stopCellXWithMargins =
                        if road.IsVertical() then stopCellX else stopCellX+1
                        
                    let startCellYWithMargins =
                        if road.IsVertical() then startCellY-1 else startCellY
                    
                    let stopCellYWithMargins =
                        if road.IsVertical() then stopCellY+1 else stopCellY
                   
                    //if the building turns out to be outside of the map - do not place it
                    if startCellX<0 || stopCellX>AreaWidthCells || startCellY <0 || stopCellY>AreaHeightCells then
                        None 
                    else
                        let cells =
                            seq{
                                for x in [startCellXWithMargins..stopCellXWithMargins] do
                                    for y in [startCellYWithMargins..stopCellYWithMargins] do
                                        yield Vector2(float32 x,float32 y)
                                
                            }
                            
                        Some(Seq.toList(cells))
                    
                match cellsAlongRoad with
                    | [] -> acc
                    | head::tail ->
                        let (startX,endX) =
                            if (road.IsVertical()) then
                                (int head.x, int head.x)
                            else
                                (int head.x,int head.x + (buildingWidth-1))
                        let (startY,endY) =
                            if (road.IsVertical()) then
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
                        let hypotheticalBuildingCellsIncludingBorders = getHypotheticalBuildingCellsWithMargins(road,suggestedCellsAlongRoad)
                        match hypotheticalBuildingCellsIncludingBorders with
                            | None ->  tailRecursiveFindPlaces(building,tail,acc)
                            | Some hCells ->
                                    if (existsCheck(cellsAlongRoad,suggestedCellsAlongRoad) && (hCells.Count() = (hypotheticalBuildingCellsIncludingBorders.Value |> List.except (cellsTakenByBuildings)).Count())) then
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
            let (freeTiles1,freeTiles2) = freeTiles road
            let isVertical = road.IsVertical()
            //removing first and last elements to free up space for possible intersection. This seems wasteful but turns out it's not that bad:
            //https://stackoverflow.com/questions/29100251/how-to-take-sublist-without-first-and-last-item-with-f
            let sortAndTrimTiles(tiles:List<Vector2>) =
                tiles
                    |> List.sortBy (fun(c)->(if isVertical then c.x else c.y))
                    |> List.tail
                    |> List.rev
                    |> List.tail
                    |> List.rev
            
            let allTilesAlongRoad = (sortAndTrimTiles freeTiles1) @ (sortAndTrimTiles freeTiles2)
           
            let allAvailableCells =
                allTilesAlongRoad
                |> List.except cellsTakenByBuildings
                |> List.except cellsTakenByRoads
                |> List.sortBy (fun(c)->(if isVertical then c.x else c.y))
                
            let placesToBuild = findPlacesToBuildAlongRoad(road,building,cellsTakenByBuildings,Seq.toList(allAvailableCells))
            let randomPlace = getRandomBuildingPlace placesToBuild
            let buildingCoords: Option<Tuple<BuildingFs,Vector2>> = 
                match randomPlace with
                    | Some place ->
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
                    do this.AddChild(building)
                    do building.Position <- coords
                    Some(coords)
                | None -> None        
        
        let tryPlacingBuildingOnRandomRoad(building:BuildingFs):Option<Tuple<BuildingFs,Road>> =
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
                None
        
                        
        //take random road
        //find a random free spot on this road
        //pick a random destination (up/down/left/right/continue)
        //check if a minimal length road (let's say 8 tiles) can be generated
            //if yes then
                //try to add random amount of tiles
                //check if you hit the building or moved beyond the map boundaries
                //if yes then
                    //work backwards until you no longer hit the building
                //place the road
            //if no then
                //pick another spot
        let createRoadForBuilding(building:BuildingFs):Option<Road> =
            let roads = getRoads()
            let cellsTakenByBuildings = getCellsTakenByAllBuildings(desertTilemap.Value)
            //shuffling the roads array to pick a random one each time
            let roads =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(roads, roads.Length))
            let rec tryCreateRoadForBuilding(roads:List<Road>,building:BuildingFs):Option<Road> =
                 //accepts a road and a shuffled list of its tiles. Goes over each one one by one
                 let rec tryFindFreeSpot(road:Road, shuffledTiles:List<MapTile>)=
                     match shuffledTiles with
                        | [] -> None
                        | tile::tail ->
                            //returns one or two tiles to start building the road from
                            let tryFreeStartRoadTiles(tile:MapTile):Tuple<Option<MapTile>,Option<MapTile>> = 
                                if (road.IsVertical()) then
                                    let tile1 =
                                            let tryCell = MapTile(tile.x-1.0f,tile.y)
                                            if (cellsTakenByBuildings.Contains(tryCell)) then
                                                None
                                            else
                                                Some(tryCell)
                                       
                                    let tile2 =
                                            let tryCell = MapTile(tile.x+1.0f,tile.y)
                                            if (cellsTakenByBuildings.Contains(tryCell)) then
                                                None
                                            else
                                                Some(tryCell)
                                    (tile1,tile2)
                                else
                                    let tile1 =
                                            let tryCell = MapTile(tile.x,tile.y-1.0f)
                                            if (cellsTakenByBuildings.Contains(tryCell)) then
                                                None
                                            else
                                                Some(tryCell)
                                       
                                    let tile2 =
                                            let tryCell = MapTile(tile.x,tile.y+1.0f)
                                            if (cellsTakenByBuildings.Contains(tryCell)) then
                                                None
                                            else
                                                Some(tryCell)
                                    (tile1,tile2)
                            
                            let (tile1,tile2) = tryFreeStartRoadTiles(tile)
                            //we found at least one possibility to build a new road from this spot
                            if (tile1.IsSome || tile2.IsSome ) then
                                Some(tile)
                            else tryFindFreeSpot(road,tail)
                 
                 //tries to build a new road based on a free tile provided
                 //returns new road built
                 let tryBuildRoad(road,freeTileOnRoad):Option<Road> =
                     //TODO - implement random lengths for roads
                     //TODO - check that map bounds are not violated
                     let minRoadLengthTiles = 8.0f
                     let directions = [-1.0f;1.0f] //up/bottom or left/right
                     let shuffledDirections =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(directions, directions.Length))
                     let rec tryBuildRoad(road:Road,freeTileOnRoad:MapTile,shuffledDirections) =
                        match shuffledDirections with
                            | [] -> None
                            | direction::tailDirections ->
                                let xFrom = freeTileOnRoad.x
                                let yFrom = freeTileOnRoad.y
                                let xTo =
                                    if road.IsVertical() then xFrom + (xFrom+minRoadLengthTiles) * direction else xFrom
                                let yTo =
                                    if road.IsVertical() then yFrom else yFrom + (yFrom+minRoadLengthTiles) * direction
                                
                                let finalXFrom = if xFrom < xTo then xFrom else xTo
                                let finalXTo = if xFrom < xTo then xTo else xFrom
                                let finalYFrom = if yFrom < yTo then yFrom else yTo
                                let finalYTo = if yFrom < yTo then yTo else yFrom
                                
                                
                                let roadCandidate = new Road(MapTile(finalXFrom,finalYFrom),MapTile(finalXTo,finalYTo))
                                let tiles = road.GetTiles()
                                if tiles.Except(cellsTakenByBuildings).Count() = tiles.Count() then
                                    Some(roadCandidate)
                                else
                                    roadCandidate.QueueFree()
                                    tryBuildRoad(road,freeTileOnRoad,tailDirections)
                                    
                     
                     tryBuildRoad(road,freeTileOnRoad,shuffledDirections)
                         
                 
                 match roads with
                    | [] -> None
                    | road::tail ->
                        let tiles = road.GetTiles()
                        let shuffledTiles =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(tiles, tiles.Count()))
                        let freeSpot:Option<MapTile> = tryFindFreeSpot(road,shuffledTiles)
                        match freeSpot with
                            | None -> tryCreateRoadForBuilding(tail,building)
                            | Some tile -> tryBuildRoad(road,tile)
            
            tryCreateRoadForBuilding(roads,building)
           

            
        match tryPlacingBuildingOnRandomRoad(building) with
            | Some(building,_) -> Some(building)
            | None ->
                let road = createRoadForBuilding(building)
                match road with
                    | None -> None
                    | Some road ->
                        do this.AddChild(road)
                        Some(building)
        
        
    override this._Ready() =                
        let house = houseScene.Instance() :?> HouseFs;
        this.AddChild(house);
        house.Position <- Vector2(float32 2 * desertTilemap.Value.CellSize.x,float32 15*desertTilemap.Value.CellSize.y)
//        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 40, float32 16)))
//        this.AddChild(new Road(Vector2(float32 20, float32 10), Vector2(float32 20, float32 30)))
//        this.AddChild(new Road(Vector2(float32 30, float32 10), Vector2(float32 30, float32 30)))
        this.AddChild(new Road(Vector2(float32 0, float32 16), Vector2(float32 4, float32 16)))
//        this.AddChild(new Road(Vector2(float32 4, float32 17), Vector2(float32 4, float32 20)))
        turn <- updateTurn turn

    override this._Process(_) =
        turn <-
//            if Input.IsActionJustPressed("ui_accept") then
                let result = tryPutBuildingOnMap(houseScene.Instance() :?> HouseFs)
                match result with 
                    | Some building ->
                        let message = sprintf "Placed house %f %f" building.Position.x building.Position.y
                        GD.Print(message)
                    | None -> GD.Print("No place left")
                updateTurn turn
//            else
//                turn

        turnLabel.Value.BbcodeText <- sprintf "[center]Turn %d [/center]" turn
        
        drawRoads()
        
        if Input.IsActionJustPressed("ui_show_cheat_menu") then
            if (cheatMenu.Value.Visible) then
                cheatMenu.Value.Hide()
            else
                cheatMenu.Value.Show()


