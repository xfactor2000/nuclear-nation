module FS.Settlement

open System
open Godot
open System.Linq

[<AbstractClass>]
type BuildingFs() =
    inherit Node2D() 
       abstract member GetSizeInTiles: int->Vector2

type HouseFs() =
    inherit BuildingFs()
    
    override this.GetSizeInTiles(tileSize:int) =
        (this.GetNode(new NodePath("ActiveHouse")) :?> Sprite).Texture.GetSize() / (float32 tileSize);
type MapTile = Vector2

type Road(from:MapTile,``to``:MapTile) =
    inherit Node()
    
    member val from:MapTile = from
    member val ``to``:MapTile = ``to``
    
    member this.IsVertical() =
        int from.x = int ``to``.x
    
    /// <summary>
    /// This function returns two lists of free tiles alongside the road
    /// </summary>
    /// <returns>Two lists of free tiles alongside the road, top/bottom or left/right</returns>
    member this.GetFreeTilesAlong():Tuple<List<MapTile>,List<MapTile>> =
        let tiles = this.GetTiles()
        let tilesList1 =
                if this.IsVertical() then
                    seq{for tile in tiles do yield Vector2(tile.x-1.0f,tile.y)}
                else
                    seq{for tile in tiles do yield Vector2(tile.x,tile.y-1.0f)}
        
        let tilesList2 =
                if this.IsVertical() then
                    seq{for tile in tiles do yield Vector2(tile.x+1.0f,tile.y)}
                else
                    seq{for tile in tiles do yield Vector2(tile.x,tile.y+1.0f)}    
       
        (Seq.toList tilesList1,Seq.toList tilesList2)
    
    member this.GetTiles():List<MapTile> =
        let to_x:int =
            let tmp_to_x = int (if ((int)``to``.x = (int)from.x) then int ``to``.x + 1 else int ``to``.x)
            if (this.IsVertical() = false) then tmp_to_x else tmp_to_x - 1
        let to_y:int =
            let tmp_to_y = int (if ((int)``to``.y = (int)from.y) then int ``to``.y + 1 else int ``to``.y)
            if (this.IsVertical()) then tmp_to_y else tmp_to_y - 1
        
        let xs = seq{for i in int from.x .. to_x do yield i}
        let ys = seq{for i in int from.y .. to_y do yield i}
        
        let tiles = seq {
            for x in xs do
                for y in ys do yield MapTile(float32 x,float32 y)
        }
        
        Seq.toList tiles
        


type DirectionEnum = TopLeft= -1 | BottomRight = 1 | Continue = 0


type Settlement() as this =
    inherit Node()

    [<Literal>] 
    let PopsPerHouse = 10
    
    [<Literal>]
    let AreaHeightCells = 30;
    [<Literal>]
    let AreaWidthCells = 50;


    let houseScene = ResourceLoader.Load("res://scenes/settlements/House2D.tscn") :?> PackedScene
    
    let tileMap =
       lazy(this.GetTree().Root.GetNode(new NodePath("Main/DesertTileMap")) :?> TileMap)
    
    let mutable population = 0
    
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
                    yield(MapTile(tile.x,tile.y))
        })
                    
    
    /// <summary>
    /// This function returns Building object in case it was placed (and rotated), None if nothing
    /// </summary>
    /// <returns>returns Building object in case it was placed (and rotated), None if nothing</returns>
    let tryPutBuildingOnMap(building:BuildingFs):Option<BuildingFs> =
        let freeTiles(road:Road) = road.GetFreeTilesAlong()
        let cellsTakenByBuildings = getCellsTakenByAllBuildings(tileMap.Value)
        let cellsTakenByRoads = getCellsTakenByRoads()
        
        let findPlacesToBuildAlongRoad (road:Road,building:BuildingFs,cellsTakenByBuildings:List<Vector2>,cellsAlongRoad:List<Vector2>):List<List<Vector2>> =
            let existsCheck(list1:List<Vector2>,list2:List<Vector2>):bool = not(list2.Except(list1).Any())
            let rec tailRecursiveFindPlaces(building:BuildingFs,cellsAlongRoad:List<Vector2>,acc:List<List<Vector2>>):List<List<Vector2>> =
                let buildingWidth = int(building.GetSizeInTiles(int tileMap.Value.CellSize.x).x) 
                let buildingHeight = int(building.GetSizeInTiles(int tileMap.Value.CellSize.y).y) 
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
        let tryPlaceBuilding(building:BuildingFs, road:Road):Option<MapTile> =
            let (freeTiles1,freeTiles2) = freeTiles road
            let isVertical = road.IsVertical()
            //removing first and last elements to free up space for possible intersection. This seems wasteful but turns out it's not that bad:
            //https://stackoverflow.com/questions/29100251/how-to-take-sublist-without-first-and-last-item-with-f
            let sortAndTrimTiles(tiles:List<MapTile>) =
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
                            let yPosition = float32 (place.Head.y + building.GetSizeInTiles(int tileMap.Value.CellSize.x).x / 2.0f) * tileMap.Value.CellSize.x
                            if (place.Head.x<road.from.x) then
                                do building.Rotate(-3.14159f/2.0f)
                                let xPosition =  float32 place.Head.x * tileMap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                            else
                                do building.Rotate(3.14159f/2.0f)
                                let xPosition =  (float32 place.Head.x + 1.0f) * tileMap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                        else
                            let xPosition = float32 (place.Head.x + building.GetSizeInTiles(int tileMap.Value.CellSize.x).x / 2.0f) * tileMap.Value.CellSize.x
                            if (place.Head.y<road.from.y) then
                                let yPosition =  float32 place.Head.y * tileMap.Value.CellSize.y
                                Some (building,Vector2(xPosition, yPosition))
                            else
                                let yPosition = float32 (place.Head.y + 1.0f)* tileMap.Value.CellSize.y
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
        //take the spot at the end of the road
        //pick a random destination (up/down/left/right/continue)
        //check if a minimal length road (let's say 8 tiles) can be generated without hitting a building or another road
            //if yes then
                //place the road in that direction
            //if no then
                //choose another direction randomly
                    //if no direction is left then pick another road randomly
        let createRoadForBuilding(building:BuildingFs):Option<Road> =
            //TODO - implement random lengths for roads
            let minRoadLengthTiles = 8.0f
            let roads = getRoads()
            let cellsTakenByBuildings = getCellsTakenByAllBuildings(tileMap.Value)
            let cellsTakenByRoads = getCellsTakenByRoads()
            //shuffling the roads array to pick a random one each time
            let roads =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(roads, roads.Length))
            let roads =
                if roads.Count() =0 then
                    //generating first road
                    let rand = Random()
                    let isVertical:bool = rand.NextDouble() > 0.5
                    let (xFrom,yFrom) =
                        if (isVertical) then
                            (rand.Next(AreaWidthCells-6)+3,0)
                        else
                            (0,rand.Next(AreaHeightCells-6)+3)
                    let (xTo,yTo) =
                        if (isVertical) then
                            (xFrom,yFrom + int minRoadLengthTiles)
                        else
                             (xFrom + int minRoadLengthTiles,yFrom)
                    let road = new Road(MapTile(float32 xFrom,float32 yFrom),MapTile(float32 xTo,float32 yTo))
                    let newRoadsList = [road]
                    this.AddChild(road)
                    newRoadsList
                else
                    roads
            let rec tryCreateRoadForBuilding(roads:List<Road>,building:BuildingFs):Option<Road> =
                 //tries to build a new road based on a free tile provided
                 //returns new road built
                 let tryBuildRoad(sourceRoad:Road,tileToBuildFrom:MapTile):Option<Road> =
                     let directions = [DirectionEnum.TopLeft; DirectionEnum.BottomRight; DirectionEnum.Continue] //up/bottom, left/right or continue
                     let shuffledDirections =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(directions, directions.Length))
                     let rec tryBuildRoad(road:Road,startTileOnRoad:MapTile,shuffledDirections:List<DirectionEnum>) =
                        match shuffledDirections with
                            | [] -> None
                            | direction::tailDirections ->
                                let (xFrom,yFrom) =
                                    (startTileOnRoad.x,startTileOnRoad.y)
                                let (xTo,yTo) =
                                    if road.IsVertical() then
                                        if direction = DirectionEnum.Continue then
                                           (xFrom,yFrom+minRoadLengthTiles)
                                        else
                                            (xFrom + minRoadLengthTiles * float32 direction,yFrom)
                                    else
                                        if direction = DirectionEnum.Continue then
                                           (xFrom+minRoadLengthTiles,yFrom)
                                        else
                                            (xFrom,yFrom + minRoadLengthTiles * float32 direction)
                                
                                let finalXFrom = if xFrom < xTo then xFrom else xTo
                                let finalXTo = if xFrom < xTo then xTo else xFrom
                                let finalYFrom = if yFrom < yTo then yFrom else yTo
                                let finalYTo = if yFrom < yTo then yTo else yFrom
                                   
                                //checking that map bounds are not violated
                                if (
                                       int finalXFrom<0 ||
                                       int finalXTo>AreaWidthCells ||
                                       int finalYFrom<0 ||
                                       int finalYTo>AreaHeightCells 
                                    )
                                then
                                    None
                                else
                                    let roadCandidate = new Road(MapTile(finalXFrom,finalYFrom),MapTile(finalXTo,finalYTo))
                                    let tiles = roadCandidate.GetTiles()
                                    let doNotBorderScreenEdgeCondition:bool =
                                        if roadCandidate.IsVertical() then
                                            roadCandidate.from.x <> 0.0f && int roadCandidate.from.x <> AreaWidthCells
                                        else
                                            roadCandidate.from.y <> 0.0f && int roadCandidate.from.y <> AreaHeightCells
                                    let doNotOverlayOnOtherRoadsCondition:bool =
                                        tiles.Except(cellsTakenByBuildings).Except(cellsTakenByRoads.Where(fun(c)-> c <> startTileOnRoad)).Count() = tiles.Count()
                                    
                                    let buildingPlacedCondition:bool =
                                        doNotBorderScreenEdgeCondition && doNotOverlayOnOtherRoadsCondition && 
                                            let result = tryPlaceBuilding(building,roadCandidate)
                                            match result with
                                                | None -> false
                                                | Some _ -> true
                                        
                                    if  doNotBorderScreenEdgeCondition && doNotOverlayOnOtherRoadsCondition && buildingPlacedCondition then
                                        Some(roadCandidate)
                                    else
                                        roadCandidate.QueueFree()
                                        tryBuildRoad(road,startTileOnRoad,tailDirections)
                                    
                     tryBuildRoad(sourceRoad,tileToBuildFrom,shuffledDirections)
                   
                         
                 match roads with
                    | [] -> None
                    | road::tail ->
                        let startTiles = [road.from;road.``to``]
                        let shuffledStartTiles =  Seq.toList(MoreLinq.MoreEnumerable.RandomSubset(startTiles, startTiles.Length))
                        
                        let rec tryBuildRoadFromTiles(road:Road,tiles:List<MapTile>): Option<Road>=
                            match tiles with
                                | [] -> None
                                | tile::tail ->
                                    let newRoad = tryBuildRoad(road,tile)
                                    match newRoad with
                                        | None -> tryBuildRoadFromTiles(road,tail)
                                        | Some road -> Some(road)
                                    
                        match tryBuildRoadFromTiles(road,shuffledStartTiles) with
                            | Some road -> Some(road)
                            | None -> tryCreateRoadForBuilding(tail,building)
                        
            
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
                        
   
                        
    member this.Population
        with get () = population
        and set (value) = population <- value
        

    
    /// <summary>
    /// This function adds population to the settlement and accomodates it by adding buildings if needed
    /// </summary>
    /// <returns>returns Amount of population it was able to accomodate</returns>
    member this.AddPopulation(extraPopulation: int): int =
        let newHousesNeeded:int = int (ceil(float32 (extraPopulation + population) / float32 PopsPerHouse) - ceil (float32(population) / float32(PopsPerHouse)))
        
        //returns the actual amount of population placed
        let rec tryPlaceHouses(numberOfHouses:int, accHouses: int): int =
            match numberOfHouses with
                | houses when houses > 0 ->
                    let result = tryPutBuildingOnMap(houseScene.Instance() :?> HouseFs)
                    match result with
                      | Some _ -> tryPlaceHouses(numberOfHouses - 1,accHouses + 1 )
                      | None -> tryPlaceHouses(0,accHouses)
                | _ -> accHouses
        
        let housesPlaced = tryPlaceHouses(newHousesNeeded,0)
        
        let populationAdded =
            if (housesPlaced = newHousesNeeded) then
                extraPopulation
            else
                housesPlaced * PopsPerHouse
        
        do this.Population <- this.Population + populationAdded        
        
        populationAdded
        
    member this.GetRoads(): List<Road> = getRoads()
    
    member this.Redraw(): unit =
        for road in this.GetRoads() do
            for tile in road.GetTiles() do
                do tileMap.Value.SetCell(int tile.x, int tile.y, 1) 