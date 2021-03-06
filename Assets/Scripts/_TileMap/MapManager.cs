﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{ 
	[SerializeField] private int _mapWidth;
    public int MapWidth
    {
        get { return _mapWidth; }
    }

  	[SerializeField] private int _mapHeight;
    public int MapHeight
    {
        get { return _mapHeight; }
    }

	private MapTile[,] _map;
    public MapTile[,] Map
    {
        get { return _map; }
    }

    [SerializeField] private static IntVector2 _center;
    public static IntVector2 Center
    {
        get { return _center; }
    }

    public List<TechBuilding> structuresOnMap { get; private set; }
    public List<Polyomino> terrainOnMap { get; private set; }
    public List<Coord> structureCoords { get; private set; }
    [SerializeField]
    private int structDistMin;
    [SerializeField]
    private int structRadiusMin;
    [SerializeField]
    private int resourceBorderMin;
    [SerializeField]
    private int startingStructCount;
    [SerializeField]
    private int procGenTriesMax;
    [SerializeField]
    private Level[] levels;
    public Level[] campaignLevels;
    public Level currentLevel
    {
        get
        {
            return Services.GameManager.levelSelected;
        }
    }
    [SerializeField]
    private Level[] eloLevelPool;
    [SerializeField]
    private List<Level> dungeonRunLevelPool;

    public void Init()
    {
        _center = Services.MapManager.CenterIndexOfGrid();

       
    }

    public void PopulateDungeonRunLevels()
    {
        dungeonRunLevelPool = new List<Level>();
        foreach (LevelData data in LevelManager.levelInfo.dungeonLevels.Values)
        {
            Level level = data.CreateLevel();
            if (!dungeonRunLevelPool.Contains(level))
            {
                dungeonRunLevelPool.Add(level);
            }
        }
    }

    public void GenerateMap(bool newEditLevel = false)
    {
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Challenge)
        {
            Level level = eloLevelPool[Random.Range(0, eloLevelPool.Length)];
            Services.GameManager.SetCurrentLevel(level);
        }
        if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            Level level = dungeonRunLevelPool[Random.Range(0, dungeonRunLevelPool.Count)];
            Services.GameManager.SetCurrentLevel(level);
        }
        if (Services.GameManager.levelSelected != null)
        {
            Level level = currentLevel;
            _mapWidth = level.width;
            _mapHeight = level.height;
        }
        else
        {
            _mapWidth = 20;
            _mapHeight = 20;
        }
        Services.GameData.totalMapTiles = MapWidth * MapHeight;

        for (int i = 0; i < 2; i++)
        {
            Services.GameData.filledMapTiles[i] = 0;
            Services.GameData.distancesToOpponentBase[i] = MapWidth + MapHeight - 8;
        }

        Services.CameraController.SetPosition(
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, -10));
        Services.GameScene.backgroundImage.transform.position =
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, 0);


        _map = new MapTile[MapWidth, MapHeight];
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapHeight; j++)
            {
                MapTile tile = Instantiate(Services.Prefabs.MapTile, 
                    Services.GameScene.tileMapHolder).GetComponent<MapTile>();
                Coord tileCoord = new Coord(i, j);
                tile.Init(tileCoord);
                _map[i, j] = tile;
                tile.name = "Tile [X: " + i + ", Y: " + j + "]";
                tile.sr.color = new Color(125f / 256f, 125f / 256f, 125f / 256f);
            }
        }
        structuresOnMap = new List<TechBuilding>();
        structureCoords = new List<Coord>();
        terrainOnMap = new List<Polyomino>();

        

        if (Services.GameManager.levelSelected == null || newEditLevel)
        {
            GenerateStructures(null);
        }
        else
        {
            GenerateStructures(Services.GameManager.levelSelected);
        }

        
        foreach (MapTile mapTile in Map)
        {
            mapTile.gameObject.SetActive(false);
        }
        foreach (TechBuilding structure in structuresOnMap)
        {
            structure.holder.gameObject.SetActive(false);
        }


        foreach (Polyomino piece in terrainOnMap)
        {
            piece.ScaleHolder(Vector3.one);
            piece.SetAlpha(1);
            piece.holder.gameObject.SetActive(false);
        }

        TaskQueue boardAnimationTasks;

        boardAnimationTasks = new TaskQueue(new List<Task>() {
            new Wait(0.3f),
            new BoardEntryAnimation(),
            new InitialBuildingEntryAnimation(),
            new ScrollReadyBanners(Services.UIManager.UIBannerManager.readyBanners, true)
            });
        Services.GameScene.tm.Do(boardAnimationTasks);




    }

    TechBuilding GenerateStructure(BuildingType type)
    {
        return GenerateStructure(type, GenerateValidStructureCoord());
    }

    TechBuilding GenerateStructure(BuildingType type, Coord structCoord)
    {
        if (structCoord != new Coord(-1, -1))
        {
            TechBuilding techBuilding;
            switch (type)
            {
                case BuildingType.BASE:
                    techBuilding = new Base();
                    break;
                case BuildingType.DYNAMO:
                    techBuilding = new Dynamo();
                    break;
                case BuildingType.SUPPLYBOOST:
                    techBuilding = new SupplyBoost();
                    break;
                case BuildingType.UPSIZE:
                    techBuilding = new Upsize();
                    break;
                case BuildingType.ATTACKUPSIZE:
                    techBuilding = new AttackUpsize();
                    break;
                case BuildingType.COMBUSTION:
                    techBuilding = new Combustion();
                    break;
                case BuildingType.SHIELDEDPIECES:
                    techBuilding = new ShieldedPieces();
                    break;
                case BuildingType.ARMORY:
                    techBuilding = new Armory();
                    break;
                case BuildingType.FISSION:
                    techBuilding = new Fission();
                    break;
                case BuildingType.RECYCLING:
                    techBuilding = new Recycling();
                    break;
                case BuildingType.CROSSSECTION:
                    techBuilding = new CrossSection();
                    break;
                case BuildingType.REPAINT:
                    techBuilding = new Annex();
                    break;
                case BuildingType.RETALIATE:
                    techBuilding = new Retaliate();
                    break;
                case BuildingType.RECOUP:
                    techBuilding = new Recoup();
                    break;
                case BuildingType.PLUNDER:
                    techBuilding = new Plunder();
                    break;
                case BuildingType.EDITMODE:
                    techBuilding = new EditModeBuilding(Services.GameManager.Players[0]);
                    break;
                default:
                    return null;
            }
            techBuilding.MakePhysicalPiece();
            techBuilding.PlaceAtLocation(structCoord);
            return techBuilding;
        }
        return null;
    }

    void GenerateTerrain()
    {
        if (Services.GameManager.levelSelected == null ||
            Services.GameManager.levelSelected.impassibleCoords == null)
            return;
        
        foreach(Coord coord in Services.GameManager.levelSelected.impassibleCoords)
        {
            Polyomino impassiblePiece = new Polyomino(1, 0, null, true, false);
            impassiblePiece.MakePhysicalPiece();
            impassiblePiece.PlaceAtLocation(coord, false , true);
            terrainOnMap.Add(impassiblePiece);
        }

        foreach (Coord coord in Services.GameManager.levelSelected.destructibleTerrainCoords)
        {
            Polyomino destructiblePiece = new Polyomino(1, 0, null, true);
            destructiblePiece.MakePhysicalPiece();
            destructiblePiece.PlaceAtLocation(coord, false, true);
            terrainOnMap.Add(destructiblePiece);
        }
    }

    void GetStructureCoords()
    {
        foreach (TechBuilding structure in Services.MapManager.structuresOnMap)
        {
            foreach (Tile tile in structure.tiles)
            {
                if (!structureCoords.Contains(tile.coord))
                {
                    structureCoords.Add(tile.coord);
                }
            }
        }
    }

    public void RemoveStructure(TechBuilding tech)
    {
        List<Coord> structCoordsToRemove = new List<Coord>();
        foreach (Tile tile in tech.tiles)
        {
            Map[tile.coord.x, tile.coord.y].SetMapSprite(false);
            Map[tile.coord.x, tile.coord.y].SetOccupyingPiece(null);
            if (structureCoords.Contains(tile.coord) && !structCoordsToRemove.Contains(tile.coord))
            {
                structCoordsToRemove.Add(tile.coord);
            }
        }

        foreach (Coord coord in structCoordsToRemove)
        {
            structureCoords.Remove(coord);
        }

        if (structuresOnMap.Contains(tech))
        {
            structuresOnMap.Remove(tech);
        }
    }

    Coord GenerateValidStructureCoord(bool mirrored = false)
    {
        Coord nullCoord = new Coord(-1, -1);
        for (int i = 0; i < procGenTriesMax; i++)
        {
            Coord candidateCoord = GenerateRandomCoord();
            Coord mirroredCoord = MirroredCoord(candidateCoord);
            if (IsStructureCoordValid(candidateCoord) &&
                (!mirrored || (IsStructureCoordValid(mirroredCoord) &&
                candidateCoord.Distance(mirroredCoord) >= structDistMin)))
                return candidateCoord;
        }
        return nullCoord;
    }

    Coord MirroredCoord(Coord coord)
    {
        return new Coord((MapWidth - 1) - coord.x, (MapHeight - 1) - coord.y);
    }

    bool IsStructureCoordValid(Coord candidateCoord)
    {
        if (candidateCoord.Distance(new Coord(0, 0)) < structRadiusMin ||
            candidateCoord.Distance(new Coord(MapWidth - 1, MapHeight - 1))
            < structRadiusMin)
            return false;
        for (int i = 0; i < structuresOnMap.Count; i++)
        {
            if (candidateCoord.Distance(structuresOnMap[i].centerCoord) < structDistMin)
                return false;
        }
        return true;
    }

    List<BuildingType> InitStructureTypeList()
    {
        return new List<BuildingType>(TechBuilding.techTypes);
    }


    public void MakeExpansions()
    {
        for (int j = 0; j < 2; j++)
        {
            Base cornerBase = new Base();
            cornerBase.MakePhysicalPiece();
            Coord location;
            if (j == 0)
            {
                location = new Coord(MapWidth - 2, 1);
            }
            else
            {
                location = new Coord(0, MapHeight - 1);
            }
            cornerBase.PlaceAtLocation(location);
            structuresOnMap.Add(cornerBase);

        }
    }

    void GenerateStructures(Level level)
    {

        List<BuildingType> structureTypes = InitStructureTypeList();
       //GenerateTerrain();
        if ((Services.GameManager.mode == TitleSceneScript.GameMode.Edit ||
            Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit) && level == null) return;

        if (level == null || level.cornerBases)
        {
            MakeExpansions();   
        }

        if (level == null) // use procedural generation if no supplied level
        {
            for (int i = 0; i < startingStructCount; i++)
            {
                BuildingType type;
                if (structureTypes.Count == 0) structureTypes = InitStructureTypeList();
                type = structureTypes[Random.Range(0, structureTypes.Count)];
                structureTypes.Remove(type);
                TechBuilding structure = GenerateStructure(type);
                if (structure == null)
                {
                    break;
                }
                structuresOnMap.Add(structure);
            }
            
        }
        else GenerateLevel(level);

        GetStructureCoords();
    }

    List<BuildingType> RemoveBuildingTypesFromTechPool(BuildingType[] availableTech, List<BuildingType> techToRemove)
    {
        List<BuildingType> currentList = new List<BuildingType>(availableTech);

        foreach(BuildingType type in techToRemove)
        {
            if (currentList.Contains(type))
            {
                currentList.Remove(type);
            }
        }

        return currentList;
    }

    void GenerateLevel(Level level)
    {
        List<BuildingType> structureTypes;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            structureTypes = RemoveBuildingTypesFromTechPool(level.availableStructures,
                                                                     DungeonRunManager.dungeonRunData.currentTech);
        }
        else 
        {
            structureTypes = new List<BuildingType>(level.availableStructures);
        }

        for (int i = 0; i < level.structCoords.Length; i++)
        {
            BuildingType type;
            if (structureTypes.Count == 0)
                structureTypes = new List<BuildingType>(level.availableStructures);

            if (Services.GameManager.mode != TitleSceneScript.GameMode.Edit && Services.GameManager.mode != TitleSceneScript.GameMode.DungeonEdit)
            {
                type = structureTypes[Random.Range(0, structureTypes.Count)];
                structureTypes.Remove(type);
            }
            else
            {
                type = BuildingType.EDITMODE;
            }
            TechBuilding structure = GenerateStructure(type, level.structCoords[i]);
            
            structuresOnMap.Add(structure);
        }

        GenerateTerrain();
    }

    Coord GenerateRandomCoord()
    {
        return new Coord(Random.Range(resourceBorderMin, MapWidth - resourceBorderMin),
            Random.Range(resourceBorderMin, MapHeight - resourceBorderMin));
    }

    public IntVector2 CenterIndexOfGrid()
    {
        return new IntVector2(MapWidth / 2, MapHeight / 2);
    }

    public void CreateMainBase(Player player, Coord coord)
    {
        Base playerBase = new Base(player, true);
        playerBase.MakePhysicalPiece();
        playerBase.PlaceAtLocation(coord);
        playerBase.TogglePieceConnectedness(true);
        player.AddBase(playerBase);
    }

    public MapTile GetRandomTile()
    {
        return _map[Random.Range(0, MapWidth), Random.Range(0, MapHeight)];
    }

    public MapTile GetTile(int i, int j)
    {
        if (((i < MapWidth) && (j < MapHeight)) && (i >= 0) && (j >= 0))
        {
            return _map[i, j];
        }

        return null;
    }

    public MapTile GetRandomEmptyTile()
    {
        MapTile randomTile = GetRandomTile();

        return randomTile;
    }

    public bool IsCoordContainedInMap(Coord coord)
    {
        return coord.x >= 0 && coord.x < MapWidth &&
                coord.y >= 0 && coord.y < MapHeight;
    }

    public bool ValidateTile(Tile tile, Player owner)
    {
        foreach(Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = tile.coord.Add(direction);
            if (IsCoordContainedInMap(adjacentCoord))
            {
                if (Map[adjacentCoord.x, adjacentCoord.y].IsOccupied() &&
                    Map[adjacentCoord.x, adjacentCoord.y].occupyingPiece.owner == owner)
                    return true;
            }
        }
        return false;
    }

    public bool ValidateEyeProperty(Tile tile, Player player)
    {
        bool isValidEye = false;
        foreach(Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = tile.coord.Add(direction);
            if(IsCoordContainedInMap(adjacentCoord))
            {
                MapTile adjacentTile = Map[adjacentCoord.x, adjacentCoord.y];
                if (adjacentTile.IsOccupied() &&
                    adjacentTile.occupyingPiece.buildingType != BuildingType.BASE &&
                    adjacentTile.occupyingPiece.owner == player)
                {
                    isValidEye = true;
                }
                else
                {
                    return false;
                }
            }
        }

        return isValidEye;
    }

    public void DetermineConnectedness(Player player)
    {
        Base mainBase = player.mainBase;
        List<Polyomino> connectedPieces = new List<Polyomino>();
        HashSet<Polyomino> frontier = new HashSet<Polyomino>();
        connectedPieces.Add(mainBase);
        frontier.UnionWith(mainBase.GetAdjacentPolyominos());
        int iterationCount = 0;
        while (frontier.Count > 0)
        {
            iterationCount += 1;
            if (iterationCount > 1000) return;
            HashSet<Polyomino> frontierQueue = new HashSet<Polyomino>();
            Polyomino piece = frontier.First();
            if ((piece is TechBuilding && (piece.owner == player || piece.owner == null)) || 
                piece.owner == player)
            {
                connectedPieces.Add(piece);
                List<Polyomino> adjacentPieces = piece.GetAdjacentPolyominos();
                for (int j = 0; j < adjacentPieces.Count; j++)
                {
                    Polyomino adjacentPiece = adjacentPieces[j];
                    if (!connectedPieces.Contains(adjacentPiece))
                    {
                        frontierQueue.Add(adjacentPiece);
                    }
                }
            }
            frontier.Remove(piece);
            frontier.UnionWith(frontierQueue);
        }

        for (int i = player.boardPieces.Count - 1; i >= 0; i--)
        {
            Polyomino piece = player.boardPieces[i];
            if (!connectedPieces.Contains(piece) && !(piece is Blueprint))
            {
                if(piece is TechBuilding)
                {
                    TechBuilding structure = piece as TechBuilding;
                    structure.OnClaimLost();
                }
                else
                {
                    piece.TogglePieceConnectedness(false);
                }
            }
        }
        int minDistToOpponentBase = int.MaxValue;
        Coord opposingBaseCoord = Services.GameManager.Players[player.playerNum % 2].mainBase.centerCoord;
        for (int i = 0; i < connectedPieces.Count; i++)
        {
            Polyomino piece = connectedPieces[i];
            if ((!piece.connected && !(piece is TechBuilding)) || 
                (piece is TechBuilding && piece.owner == null))
            {
                if(piece is TechBuilding)
                {
                    TechBuilding structure = piece as TechBuilding;
                    structure.OnClaim(player);
                }
                else
                {
                    piece.TogglePieceConnectedness(true);
                }
            }
            int dist = piece.centerCoord.Distance(opposingBaseCoord);
            if (dist < minDistToOpponentBase)
            {
                minDistToOpponentBase = dist;
            }
        }
        Services.GameData.distancesToOpponentBase[player.playerNum - 1] = minDistToOpponentBase;
    }

	public bool ConnectedToBase(Polyomino piece, List<Polyomino> checkedPieces)
    {
        if (piece.buildingType == BuildingType.BASE) return true;

        checkedPieces.Add(piece);

        List<Polyomino> piecesToCheck = new List<Polyomino>();
        foreach (Tile tile in piece.tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    MapTile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() &&
                        adjTile.occupyingPiece.owner == piece.owner &&
                        adjTile.occupyingPiece != piece &&
                        adjTile.occupyingPiece.buildingType == BuildingType.BASE)
                    {
                        return true;
                    }
                    else
                    {
                        if (adjTile.occupyingPiece != null)
                        {
                            if (!checkedPieces.Contains(adjTile.occupyingPiece) &&
                                !piecesToCheck.Contains(adjTile.occupyingPiece) &&
                                adjTile.occupyingPiece.owner == piece.owner)
                            {
                                piecesToCheck.Add(adjTile.occupyingPiece);
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < piecesToCheck.Count; i++)
        {
            if (ConnectedToBase(piecesToCheck[i], checkedPieces)) return true;
        }
        return false;
    }

    public bool CheckForWin(Polyomino piece)
    {
        foreach (Tile tile in piece.tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    MapTile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() &&
                        adjTile.occupyingPiece.owner != null &&
                        adjTile.occupyingPiece.owner != piece.owner &&
                        adjTile.occupyingPiece.buildingType == BuildingType.BASE)
                    {
                        Base occupyingBase = adjTile.occupyingPiece as Base;
                        if(occupyingBase.mainBase)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public Level GetNextLevel()
    {
        Level nextLevel = null;
        for (int i = 0; i < campaignLevels.Length; i++)
        {
            Level level = campaignLevels[i];
            if(level == currentLevel && i < campaignLevels.Length -1)
            {
                nextLevel = campaignLevels[i + 1];
                break;
            }
        }
        return nextLevel;
    }

    public void UpdateMapTileBrightness()
    {
        foreach(MapTile tile in Map)
        {
            if (Services.GameManager.NeonEnabled)
            {
                tile.sr.color = new Color(125 / 255f, 125 / 255f, 125 / 255f);
            }
            else
            {
                tile.sr.color = Color.white;
            }
        }
    }
}
