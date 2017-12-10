﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : Polyomino
{
    protected static int[,,] factory = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,0,1,1,0 },
                { 0,1,1,1,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 }
            }
    };
    protected static int[,,] mine = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,1,1,1,1 },
                { 0,1,1,1,1 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
    };
    protected static int[,,] bombFactory = new int[1, 5, 5]
    {       //  These hashes represent what the piece will look like
            //    #
            //   ###
            //   ###
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,1,0 },
                { 0,1,1,1,0 },
                { 0,0,0,0,0 }
            }
    };

public Blueprint(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
    }

    public Blueprint(BuildingType _buildingType, Player _player) : base(0, 0, _player)
    {
        buildingType = _buildingType;
        owner = _player;

        switch (buildingType)
        {
            case BuildingType.FACTORY:
                holderName = "FactoryHolder";
                piece = factory;
                break;
            case BuildingType.MINE:
                holderName = "MineHolder";
                piece = mine;
                break;
            case BuildingType.BOMBFACTORY:
                holderName = "BombFactoryHolder";
                piece = bombFactory;
                break;
            default:
                break;
        }

    }

    protected bool PiecesShareOwner(Tile tile)
    {
        if (tile.occupyingPiece == null) return false;
        if (tile.occupyingPiece is Structure) return false;
        return owner == tile.occupyingPiece.owner;
    }

    public override bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //  CONDITIONS:
        //      Blueprint tiles are contained in the map
        //      All tiles in blueprint are placed on tiles that belong to a single player
        //      All pieces a blueprint will cover do not belong to another blueprint
        //      There is a piece on the tile the blueprint will be placed on
        bool isLegal = false;
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!PiecesShareOwner(Services.MapManager.Map[tile.coord.x, tile.coord.y])) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].PartOfExistingBlueprint()) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied())
            {
                isLegal = true;
            }
            else
            {
                return false;
            }
        }

        return isLegal;
    }

    public override void MakePhysicalPiece(bool isViewable)
    {
        holder = GameObject.Instantiate(Services.Prefabs.PieceHolder,
            Services.GameScene.transform).transform;
        holder.gameObject.name = holderName;
        holderSr = holder.gameObject.GetComponent<SpriteRenderer>();
        spriteOverlay = holder.GetComponentsInChildren<SpriteRenderer>()[2];
        costText = holder.gameObject.GetComponentInChildren<TextMesh>();
        ToggleCostUIStatus(false);
        tooltips = new List<Tooltip>();

        if (piece == null) return;
        tileRelativeCoords = new Dictionary<Tile, Coord>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile, holder);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newpiece.Init(myCoord, this);
                    tileRelativeCoords[newpiece] = myCoord;

                    string pieceName = newpiece.name.Replace("(Clone)", "");
                    newpiece.name = pieceName;
                    newpiece.ActivateTile(owner, buildingType);
                    newpiece.SetAlpha(0.75f);
                    newpiece.IncrementSortingOrder(5);
                    tiles.Add(newpiece);
                }
            }
        }

        SetVisible(!isViewable);
        SetSprites();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        switch (buildingType)
        {
            case BuildingType.FACTORY:
                iconSr.sprite = Services.UIManager.factoryIcon;
                break;
            case BuildingType.MINE:
                iconSr.sprite = Services.UIManager.mineIcon;
                break;
            case BuildingType.BOMBFACTORY:
                iconSr.sprite = Services.UIManager.bombFactoryIcon;
                break;
            case BuildingType.NONE:
                iconSr.enabled = false;
                break;
            default:
                break;
        }
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        switch (buildingType)
        {
            case BuildingType.FACTORY:
                spriteOverlay.sprite = Services.UIManager.factoryOverlay;
                break;
            case BuildingType.MINE:
                spriteOverlay.sprite = Services.UIManager.mineOverlay;
                break;
            case BuildingType.BOMBFACTORY:
                spriteOverlay.sprite = Services.UIManager.bombFactoryOverlay;
                break;
            case BuildingType.NONE:
                spriteOverlay.enabled = false;
                break;
            default:
                break;
        }
    }

    public override void PlaceAtCurrentLocation()
    {
        placed = true;
        OnPlace();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.occupyingBlueprint == null)
            {
                mapTile.occupyingPiece.AddOccupyingBlueprint(this);
                mapTile.SetOccupyingBlueprint(this);
            }
        }
        owner.OnPiecePlaced(this);
    }

    public override void Remove()
    {
        //switch (buildingType)
        //{
        //    case BuildingType.FACTORY:
        //        owner.ToggleFactoryCount(-1);
        //        break;
        //    case BuildingType.MINE:
        //        owner.ToggleMineCount(-1);
        //        break;
        //    default:
        //        break;
        //}
        List<Polyomino> constituentPieces = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            tile.OnRemove();
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (!constituentPieces.Contains(mapTile.occupyingPiece))
                constituentPieces.Add(mapTile.occupyingPiece);
            mapTile.SetOccupyingBlueprint(null);
        }
        for (int i = 0; i < constituentPieces.Count; i++)
        {
            constituentPieces[i].RemoveOccupyingBlueprint(this);
        }
        owner.OnPieceRemoved(this);
        GameObject.Destroy(holder.gameObject);
        HideFromInput();
    }

    protected override void OnPlace()
    {
        //switch (buildingType)
        //{
        //    case BuildingType.FACTORY:
        //        owner.ToggleFactoryCount(1);
        //        Services.AudioManager.CreateTempAudio(Services.Clips.FactoryPlaced, 1);
        //        break;
        //    case BuildingType.MINE:
        //        owner.ToggleMineCount(1);
        //        Services.AudioManager.CreateTempAudio(Services.Clips.FactoryPlaced, 1);
        //        break;
        //    default:
        //        break;
        //}
        Services.AudioManager.CreateTempAudio(Services.Clips.BlueprintPlaced, 1);
        foreach(Tile tile in tiles)
        {
            if(Services.MapManager.Map[tile.coord.x, tile.coord.y].occupyingPiece.connected)
            {
                TogglePieceConnectedness(true);
                break;
            }
        }
        ListenForInput();
    }

    public override void OnInputDown()
    {
        base.OnInputDown();
        if (!Services.UIManager.IsTouchMakingTooltipAlready(touchID))
        {
            if (placed || owner.playerNum == 2)
            {
                Tooltip tooltipLeft = GameObject.Instantiate(Services.Prefabs.Tooltip,
                    Services.UIManager.canvas).GetComponent<Tooltip>();
                tooltipLeft.Init(GetName(), GetDescription(), 90,
                    Services.GameManager.MainCamera.WorldToScreenPoint(
                    GetCenterpoint()), !placed);
                tooltips.Add(tooltipLeft);
            }
            if (placed || owner.playerNum == 1)
            {
                Tooltip tooltipRight = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
                tooltipRight.Init(GetName(), GetDescription(), -90,
                    Services.GameManager.MainCamera.WorldToScreenPoint(
                    GetCenterpoint()), !placed);
                tooltips.Add(tooltipRight);
            }

            Services.GameEventManager.Register<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

            Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);

            Services.UIManager.OnTooltipCreated(touchID);
        }
    }

    public override void OnInputDrag(Vector3 inputPos)
    {
        base.OnInputDrag(inputPos);
        DestroyTooltips();
    }

    public override void OnInputUp()
    {
        if (!placed)
        {
            DestroyTooltips();
            Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
            Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);

            Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
            Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
            if (IsPlacementLegal() && !owner.gameOver)
            {
                PlaceAtCurrentLocation();
                ListenForInput();
            }
            else
            {
                owner.CancelSelectedBlueprint();
                EnterUnselectedState();
            }
        }
    }
}
