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
    private int resourceBorderMin;
    [SerializeField]
    private int resourceSizeMin;
    [SerializeField]
    private int resourceSizeMax;
    [SerializeField]
    private int startingResourceCount;
    [SerializeField]
    private int procGenTriesMax;
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
            }
        }
        GenerateStructures();
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

    Coord MirroredCoord(Coord coord)
    {
        return new Coord((MapWidth - 1) - coord.x, (MapLength - 1) - coord.y);
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

    void GenerateStructures()
    {
        structuresOnMap = new List<Structure>();
        Structure structure = new Structure(7, 0);
        structure.MakePhysicalPiece(true);
        structure.PlaceAtLocation(new Coord(4,4));
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

    public void CreateBase(Player player, Coord coord)
    {
        Base playerBase = new Base(9, 0, player);
        playerBase.MakePhysicalPiece(true);
        playerBase.PlaceAtLocation(coord);
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

	public bool ConnectedToBase(Polyomino piece, List<Polyomino> checkedPieces, int count)
    {
        int calls = count;

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
            calls++;
            if (ConnectedToBase(piecesToCheck[i], checkedPieces, calls)) return true;
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
                        adjTile.occupyingPiece.owner != piece.owner &&
                        adjTile.occupyingPiece.buildingType == BuildingType.BASE)
                        return true;
                }
            }
        }
        return false;
    }

    public void CheckForFortification(Polyomino piece, List<Tile> emptyTiles, bool isBeingDestroyed)
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
                        adjacentTile.occupyingPiece.buildingType != BuildingType.STRUCTURE)
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

        if(piecesToFortify.Count > 0 && !piecesToFortify.Contains(piece))
        {
            piecesToFortify.Add(piece);
        }

        for (int i = 0; i < piecesToFortify.Count; i++)
        {
            Debug.Log("fortifying piece at " + piecesToFortify[i].centerCoord.x + "," +
                piecesToFortify[i].centerCoord.y);
        }

        Player owner = piece.owner;

        foreach(Polyomino polyomino in piecesToFortify)
        {
            List<Tile> tempTiles = polyomino.tiles;
            polyomino.Remove(true);

            foreach(Tile tile in tempTiles)
            {
                Polyomino fortifiedMonomino = new Polyomino(1, 0, owner);
                Debug.Log("placing monomino at " + tile.coord.x + "," + tile.coord.y);
                fortifiedMonomino.isFortified = true;
                fortifiedMonomino.MakePhysicalPiece(true);
                fortifiedMonomino.PlaceAtLocation(tile.coord, true);
                fortifiedMonomino.ToggleAltColor(true);
            }
        }
    }
}
