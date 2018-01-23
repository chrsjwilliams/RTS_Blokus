﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{ 
	[SerializeField] private int _mapWidth;
    public int MapWidth
    {
        get { return _mapWidth; }
    }

  	[SerializeField] private int _mapLength;
    public int MapLength
    {
        get { return _mapLength; }
    }

	[SerializeField] private Tile[,] _map;
    public Tile[,] Map
    {
        get { return _map; }
    }

    [SerializeField] private static IntVector2 _center;
    public static IntVector2 Center
    {
        get { return _center; }
    }

    private List<SuperDestructorResource> resourcesOnMap;
    private List<Structure> structuresOnMap;
    [SerializeField]
    private int resourceDistMin;
    [SerializeField]
    private int resourceRadiusMin;
    [SerializeField]
    private int structDistMin;
    [SerializeField]
    private int structRadiusMin;
    [SerializeField]
    private int resourceBorderMin;
    [SerializeField]
    private int resourceSizeMin;
    [SerializeField]
    private int resourceSizeMax;
    [SerializeField]
    private int startingResourceCount;
    [SerializeField]
    private int startingStructCount;
    [SerializeField]
    private int procGenTriesMax;
    [SerializeField]
    private Level[] levels;

    public void Init()
    {
        _center = Services.MapManager.CenterIndexOfGrid();
    }

    public void GenerateMap()
    {
        _map = new Tile[MapWidth, MapWidth];
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapLength; j++)
            {
                Tile tile = Instantiate(Services.Prefabs.Tile, GameSceneScript.tileMapHolder)
                    .GetComponent<Tile>();
                
                tile.Init(new Coord(i, j));
                _map[i, j] = tile;
                tile.name = "Tile [X: " + i + ", Y: " + j + "]";
                tile.SetMaskSrAlpha(0);
            }
        }

        if (!Services.GameManager.usingStructures && !Services.GameManager.usingMiniBases)
            startingStructCount = 0;
        
        if(Services.GameManager.levelSelected == 0)
        {
            GenerateStructures(null);
        }
        else
        {
            GenerateStructures(levels[Services.GameManager.levelSelected - 1]);
        }
        //GenerateResources();
    }

    void GenerateResources()
    {
        resourcesOnMap = new List<SuperDestructorResource>();
        for (int i = 0; i < startingResourceCount / 2; i++)
        {
            List<SuperDestructorResource> resources = GenerateResourceAndMirroredResource();
            if (resources == null) break;
            foreach (SuperDestructorResource resource in resources)
            {
                resourcesOnMap.Add(resource);
            }
        }
    }

    List<SuperDestructorResource> GenerateResourceAndMirroredResource()
    {
        Coord resourceCoord = GenerateValidResourceCoord();
        Coord mirroredCoord = MirroredCoord(resourceCoord);
        List<SuperDestructorResource> resources = new List<SuperDestructorResource>();
        if (resourceCoord != new Coord(-1, -1))
        {
            int tileCount = Random.Range(resourceSizeMin, resourceSizeMax + 1);
            int shapeTypeCount = Polyomino.pieceTypes[tileCount];
            int tileIndex = Random.Range(0, shapeTypeCount);
            int numRotations = Random.Range(0, 4);
            SuperDestructorResource resource = 
                new SuperDestructorResource(tileCount, tileIndex);
            SuperDestructorResource mirroredResource =
                new SuperDestructorResource(tileCount, tileIndex);
            resource.MakePhysicalPiece(true);
            mirroredResource.MakePhysicalPiece(true);

            for (int i = 0; i < numRotations; i++)
            {
                resource.Rotate();
            }
            for (int i = 0; i < numRotations + 2; i++)
            {
                mirroredResource.Rotate();
            }

            resource.PlaceAtLocation(resourceCoord);
            mirroredResource.PlaceAtLocation(mirroredCoord);
            resources.Add(resource);
            resources.Add(mirroredResource);
            return resources;
        }
        return null;
    }

    Structure GenerateStructure(BuildingType type)
    {
        return GenerateStructure(type, GenerateValidStructureCoord());
    }

    Structure GenerateStructure(BuildingType type, Coord structCoord)
    {
        if (structCoord != new Coord(-1, -1))
        {
            //int numRotations = Random.Range(0, 4);
            Structure structure;
            switch (type)
            {
                case BuildingType.BASE:
                    structure = new Base();
                    break;
                case BuildingType.MININGDRILL:
                    structure = new MiningDrill();
                    break;
                case BuildingType.ASSEMBLYLINE:
                    structure = new AssemblyLine();
                    break;
                case BuildingType.FORTIFIEDSTEEL:
                    structure = new FortifiedSteel();
                    break;
                case BuildingType.BIGGERBRICKS:
                    structure = new BiggerBricks();
                    break;
                case BuildingType.BIGGERBOMBS:
                    structure = new BiggerBombs();
                    break;
                case BuildingType.SPLASHDAMAGE:
                    structure = new SplashDamage();
                    break;
                case BuildingType.SHIELDEDPIECES:
                    structure = new ShieldedPieces();
                    break;
                default:
                    return null;
            }
            structure.MakePhysicalPiece(true);
            //for (int i = 0; i < numRotations; i++)
            //{
            //    structure.Rotate();
            //}
            structure.PlaceAtLocation(structCoord);
            return structure;
        }
        return null;
    }

    Coord GenerateValidResourceCoord()
    {
        Coord nullCoord = new Coord(-1, -1);
        for (int i = 0; i < procGenTriesMax; i++)
        {
            Coord candidateCoord = GenerateRandomCoord();
            Coord mirroredCoord = MirroredCoord(candidateCoord);
            if (IsResourceCoordValid(candidateCoord) &&
                IsResourceCoordValid(mirroredCoord) &&
                candidateCoord.Distance(mirroredCoord) >= resourceDistMin)
                return candidateCoord;
        }
        return nullCoord;
    }

    Coord GenerateValidStructureCoord()
    {
        return GenerateValidStructureCoord(false);
    }

    Coord GenerateValidStructureCoord(bool mirrored)
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
        return new Coord((MapWidth - 1) - coord.x, (MapLength - 1) - coord.y);
    }

    bool IsStructureCoordValid(Coord candidateCoord)
    {
        if (candidateCoord.Distance(new Coord(0, 0)) < structRadiusMin ||
            candidateCoord.Distance(new Coord(MapWidth - 1, MapLength - 1))
            < structRadiusMin)
            return false;
        for (int i = 0; i < structuresOnMap.Count; i++)
        {
            if (candidateCoord.Distance(structuresOnMap[i].centerCoord) < structDistMin)
                return false;
        }
        return true;
    }

    bool IsResourceCoordValid(Coord candidateCoord)
    {
        if (candidateCoord.Distance(new Coord(0, 0)) < resourceRadiusMin ||
            candidateCoord.Distance(new Coord(MapWidth - 1, MapLength - 1))
            < resourceRadiusMin)
            return false;
        for (int i = 0; i < resourcesOnMap.Count; i++)
        {
            if (candidateCoord.Distance(resourcesOnMap[i].centerCoord) < resourceDistMin)
                return false;
        }
        return true;
    }

    List<BuildingType> InitStructureTypeList()
    {
        return new List<BuildingType>()
        {
            BuildingType.MININGDRILL,
            BuildingType.ASSEMBLYLINE,
            BuildingType.FORTIFIEDSTEEL,
            BuildingType.BIGGERBRICKS,
            BuildingType.BIGGERBOMBS,
            BuildingType.SPLASHDAMAGE,
            BuildingType.SHIELDEDPIECES
        };
    }

    void GenerateStructures(Level level)
    {
        structuresOnMap = new List<Structure>();
        List<BuildingType> structureTypes = InitStructureTypeList();

        if (Services.GameManager.usingMiniBases)
        {
            for (int j = 0; j < 2; j++)
            {
                Base cornerBase = new Base();
                cornerBase.MakePhysicalPiece(true);
                Coord location;
                if (j == 0)
                {
                    location = new Coord(MapWidth - 2, 1);
                }
                else
                {
                    location = new Coord(0, MapLength - 1);
                }
                cornerBase.PlaceAtLocation(location);
                structuresOnMap.Add(cornerBase);
            }
        }
        if (Services.GameManager.usingStructures)
        {
            if (level == null) // use procedural generation if no supplied level
            {
                for (int i = 0; i < startingStructCount; i++)
                {
                    //List<Structure> structures = new List<Structure>();
                    BuildingType type;
                    if (structureTypes.Count == 0) structureTypes = InitStructureTypeList();
                    type = structureTypes[Random.Range(0, structureTypes.Count)];
                    structureTypes.Remove(type);
                    Structure structure = GenerateStructure(type);
                    if (structure == null)
                    {
                        Debug.Log("stopping short after " + structuresOnMap.Count + "structures");
                        break;
                    }
                    //foreach (Structure structure in structures)
                    //{
                    structuresOnMap.Add(structure);
                    //}
                }
            }
            else GenerateLevel(level);
        }
    }

    void GenerateLevel(Level level)
    {
        List<BuildingType> structureTypes = new List<BuildingType>(level.availableStructures);
        for (int i = 0; i < level.structCoords.Length; i++)
        {
            BuildingType type;
            if (structureTypes.Count == 0)
                structureTypes = new List<BuildingType>(level.availableStructures);
            type = structureTypes[Random.Range(0, structureTypes.Count)];
            structureTypes.Remove(type);
            Structure structure = GenerateStructure(type, level.structCoords[i]);
            structuresOnMap.Add(structure);
        }
    }

    Coord GenerateRandomCoord()
    {
        return new Coord(Random.Range(resourceBorderMin, MapWidth - resourceBorderMin),
            Random.Range(resourceBorderMin, MapLength - resourceBorderMin));
    }

    public IntVector2 CenterIndexOfGrid()
    {
        return new IntVector2(MapWidth / 2, MapLength / 2);
    }

    public void CreateMainBase(Player player, Coord coord)
    {
        Base playerBase = new Base(player, true);
        player.mainBase = playerBase;
        playerBase.ShiftColor(player.ColorScheme[0]);
        playerBase.MakePhysicalPiece(true);
        playerBase.PlaceAtLocation(coord);
        playerBase.TogglePieceConnectedness(true);
    }

    public Tile GetRandomTile()
    {
        return _map[Random.Range(0, MapWidth), Random.Range(0, MapLength)];
    }

    public Tile GetRandomEmptyTile()
    {
        Tile randomTile = GetRandomTile();

        return randomTile;
    }

    public bool IsCoordContainedInMap(Coord coord)
    {
        return coord.x >= 0 && coord.x < MapWidth &&
                coord.y >= 0 && coord.y < MapLength;
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
                Tile adjacentTile = Map[adjacentCoord.x, adjacentCoord.y];
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
        List<Polyomino> frontier = new List<Polyomino>();
        connectedPieces.Add(mainBase);
        frontier.AddRange(mainBase.GetAdjacentPolyominos());
        while (frontier.Count > 0)
        {
            List<Polyomino> frontierQueue = new List<Polyomino>();
            for (int i = frontier.Count - 1; i >= 0; i--)
            {
                Polyomino piece = frontier[i];
                if (!connectedPieces.Contains(piece) && 
                    ((piece is Structure && 
                    (piece.owner == player || piece.owner == null)) || piece.owner == player))
                {
                    connectedPieces.Add(piece);
                    List<Polyomino> adjacentPieces = piece.GetAdjacentPolyominos();
                    for (int j = 0; j < adjacentPieces.Count; j++)
                    {
                        Polyomino adjacentPiece = adjacentPieces[j];
                        if (!frontier.Contains(adjacentPiece) && 
                            !frontierQueue.Contains(adjacentPiece))
                            frontierQueue.Add(adjacentPiece);
                    }
                }
                frontier.Remove(piece);
            }
            frontier.AddRange(frontierQueue);
        }

        for (int i = player.boardPieces.Count - 1; i >= 0; i--)
        {
            Polyomino piece = player.boardPieces[i];
            if (!connectedPieces.Contains(piece) && !(piece is Blueprint))
            {
                if(piece is Structure)
                {
                    Structure structure = piece as Structure;
                    structure.OnClaimLost();
                }
                else
                {
                    piece.TogglePieceConnectedness(false);
                }
            }
        }
        for (int i = 0; i < connectedPieces.Count; i++)
        {
            Polyomino piece = connectedPieces[i];
            if ((!piece.connected && !(piece is Structure)) || 
                (piece is Structure && piece.owner == null))
            {
                if(piece is Structure)
                {
                    Structure structure = piece as Structure;
                    structure.OnClaim(player);
                }
                else
                {
                    piece.TogglePieceConnectedness(true);
                }
            }
        }
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
                    Tile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
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
                    Tile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
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

    public bool CheckForFortification(Polyomino piece, List<Tile> emptyTiles, bool isBeingDestroyed)
    {
        List<Polyomino> piecesToFortify = new List<Polyomino>();

        foreach (Tile tile in emptyTiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjacentTile = Map[adjacentCoord.x, adjacentCoord.y];

                    if (ValidateEyeProperty(tile, piece.owner) && !piece.isFortified 
                        && !piecesToFortify.Contains(adjacentTile.occupyingPiece) &&
                        !(adjacentTile.occupyingPiece is Structure))
                    {
                        piecesToFortify.Add(adjacentTile.occupyingPiece);
                    }
                    else
                    {
                        if (adjacentTile.IsOccupied() && isBeingDestroyed && adjacentTile.occupyingPiece.owner != piece.owner)
                        {
                            //adjacentTile.occupyingPiece.isFortified = false;
                            //adjacentTile.occupyingPiece.ToggleAltColor(false);
                           // adjacentTile.occupyingPiece.CheckForFortification(false);
                        }
                    }
                }
            }
        }

        if(piecesToFortify.Count > 0 && !piecesToFortify.Contains(piece) && !(piece is Structure))
        {
            piecesToFortify.Add(piece);
        }
        foreach(Polyomino polyomino in piecesToFortify)
        {
            FortifyPiece(polyomino);
        }

        return piecesToFortify.Count > 0;
    }

    public void FortifyPiece(Polyomino piece)
    {
        List<Tile> tempTiles = piece.tiles;
        List<Blueprint> prevOccupyingStructures = piece.occupyingBlueprints;
        piece.Remove(true);

        foreach (Tile tile in tempTiles)
        {
            Polyomino fortifiedMonomino = new Polyomino(1, 0, piece.owner);
            fortifiedMonomino.isFortified = true;
            fortifiedMonomino.MakePhysicalPiece(true);
            fortifiedMonomino.PlaceAtLocation(tile.coord, true);

            for (int i = 0; i < prevOccupyingStructures.Count; i++)
            {
                foreach(Tile structTile in prevOccupyingStructures[i].tiles)
                {
                    if (structTile.coord == tile.coord)
                    {
                        fortifiedMonomino.AddOccupyingBlueprint(prevOccupyingStructures[i]);
                        break;
                    }
                }
            }
            //fortifiedMonomino.ToggleAltColor(true);
        }
    }
}
