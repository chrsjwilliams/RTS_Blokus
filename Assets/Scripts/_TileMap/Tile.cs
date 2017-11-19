﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }
    [SerializeField]
    private Sprite[] sprites;
    public Coord coord { get; private set; }
    public BoxCollider2D boxCol { get; private set; }
    private SpriteRenderer sr;
    private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Structure occupyingStructure { get; private set; }
    public SuperDestructorResource occupyingResource { get; private set; }
    private Color prevColor;
    private Color targetColor_;
    private Color targetColor
    {
        get { return targetColor_; }
        set
        {
            prevColor = sr.color;
            targetColor_ = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private float colorChangeTimeElapsed;
    [SerializeField]
    private float colorChangeDuration;
    private bool changingColor;
    private SpriteRenderer maskSr;

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        glow = GetComponent<SpriteGlow>();
        maskSr = GetComponentInChildren<SpriteMask>().gameObject.GetComponent<SpriteRenderer>();
        glow.OutlineWidth = 0;
        transform.position = new Vector3(coord.x, coord.y, 0);
        if ((coord.x + coord.y) % 2 == 0)
        {
            sr.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            sr.color = Services.GameManager.MapColorScheme[1];
        }
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        sr.sortingOrder += 5;
    }

	public void OnRemove(){
        //Destroy(this);
	}

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        sr.color = color;
    }

    public void ShiftColor(Color color)
    {
        targetColor = color;
    }

    public void ToggleAltColor(bool useAlt)
    {
        if (useAlt) sr.color = pieceParent.owner.ActiveTilePrimaryColors[1];
        else sr.color = pieceParent.owner.ActiveTilePrimaryColors[0];
    }

    public void SetGlowOutLine(int i)
    {
        glow.OutlineWidth = i;
    }

    public void SetGlowColor(Color color)
    {
        glow.GlowColor = color;
    }

    public void SetMaskSrAlpha(float alpha)
    {
        maskSr.color = new Color(maskSr.color.r, maskSr.color.g, maskSr.color.b, alpha);
    }

    public void ActivateTile(Player player, BuildingType buildingType)
    {
        _isActive = true;
        if (player == null) sr.color = Services.GameManager.SuperDestructorResourceColor;
        else if(buildingType == BuildingType.NONE) sr.color = player.ActiveTilePrimaryColors[0];
        else sr.color = player.ActiveTilePrimaryColors[1];
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public void SetOccupyingResource(SuperDestructorResource resource)
    {
        occupyingResource = resource;
    }

    public void SetOccupyingStructure(Structure structure)
    {
        occupyingStructure = structure;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfStructure()
    {
        return occupyingStructure != null;
    }

    public bool HasResource()
    {
        return occupyingResource != null;
    }

    public Polyomino OccupiedBy()
    {
        if(IsOccupied())
        {
            return occupyingPiece;
        }
        else
        {
            return null;
        }
    }

    private void Update()
    {
        if (changingColor) LerpToTargetColor();
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        sr.color = Color.Lerp(prevColor, targetColor, colorChangeTimeElapsed / colorChangeDuration);
        if(colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }


    public void SetSprite(int spriteIndex)
    {
        sr.sprite = sprites[spriteIndex];
    }

    public void SetAlpha(float alpha)
    {
        targetColor = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }

    public void IncrementSortingOrder(int inc)
	{
		sr.sortingOrder += inc;
	}
}
