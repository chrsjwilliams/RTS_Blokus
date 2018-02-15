﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }
    [SerializeField]
    private Sprite[] sprites;
    [SerializeField]
    private Sprite[] destructorSprites;
    [SerializeField]
    private Sprite[] shieldSprites;
    public Coord coord { get; private set; }
    public BoxCollider2D boxCol { get; private set; }
    public SpriteRenderer sr { get; private set; }
    //private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
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
            baseColor = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private Color baseColor;
    private float colorChangeTimeElapsed;
    [SerializeField]
    private float colorChangeDuration;
    private bool changingColor;
    public SpriteRenderer highlightSr { get; private set; }
    private SpriteRenderer bombOverlay;
    private SpriteRenderer moltenLines;
    private SpriteRenderer shieldSr;
    private bool bombSettling;
    private float bombSettleTimeElapsed;
    private const float bombSettleDuration = 0.4f;
    private float currentBombAlpha;
    private float currentNormalAlpha;
    private SortingGroup sortingGroup;
    private float redPulseTimer;
    private const float redPulsePeriod = 0.6f;
    private static Color redPulseColor = new Color(0.5f, 0, 0);
    private bool toRed;
    //public SpriteMask mask { get; private set; }

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        sortingGroup = GetComponentInChildren<SortingGroup>();
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        highlightSr = srs[0];
        sr = srs[1];
        bombOverlay = srs[2];
        moltenLines = srs[3];
        shieldSr = srs[4];
        shieldSr.enabled = false;
        highlightSr.enabled = false;
        if (!(pieceParent != null && pieceParent is Destructor))
        {
            //bombOverlay.enabled = false;
            moltenLines.enabled = false;
        }
        bombOverlay.enabled = false;
        
        transform.position = new Vector3(coord.x, coord.y, 0);

        sr.color = Services.GameManager.MapColorScheme[0];
        baseColor = sr.color;
        toRed = true;
        if (pieceParent == null) IncrementSortingOrder(-5000);

    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
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
        bombOverlay.color = color;
        baseColor = color;
        targetColor = color;
    }

    public Color GetColor() { return sr.color; }

    public void ShiftColor(Color color)
    {
        targetColor = color;
    }

    public void SetGlowOutLine(int i)
    {
        //glow.OutlineWidth = i;
    }

    public void SetGlowColor(Color color)
    {
        //glow.GlowColor = color;
    }

    public void SetHighlightAlpha(float alpha)
    {
        highlightSr.color = new Color(highlightSr.color.r, highlightSr.color.g, highlightSr.color.b, alpha);
    }

    public void SetHighlightStatus(bool status)
    {
        highlightSr.enabled = status;
    }

    public void SetHighlightColor(Color color)
    {
        highlightSr.color = color;
    }

    public void ActivateTile(Player player, BuildingType buildingType)
    {
        _isActive = true;
        if (player == null) sr.color = Services.GameManager.NeutralColor;
        //else if(buildingType == BuildingType.NONE)
         else sr.color = player.ColorScheme[0];
        //else sr.color = player.ColorScheme[1];
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public void SetOccupyingResource(SuperDestructorResource resource)
    {
        occupyingResource = resource;
    }

    public void SetOccupyingBlueprint(Blueprint blueprint)
    {
        occupyingBlueprint = blueprint;
    }

    public void SetOccupyingStructure(Structure structure)
    {
        occupyingStructure = structure;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfExistingBlueprint()
    {
        return occupyingBlueprint != null;
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
        if (bombSettling) SettleToNormalSprite();
        if (pieceParent != null && pieceParent is Destructor && !pieceParent.placed)
            PulseRed();
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        sr.color = Color.Lerp(prevColor, targetColor, 
            colorChangeTimeElapsed / colorChangeDuration);
        baseColor = sr.color;
        bombOverlay.color = new Color(sr.color.r, sr.color.g, sr.color.b, bombOverlay.color.a);
        if(colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }

    void SettleToNormalSprite()
    {
        bombSettleTimeElapsed += Time.deltaTime;
        //sr.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 0),
        //    new Color(sr.color.r, sr.color.g, sr.color.b, 1),
        //    EasingEquations.Easing.QuadEaseOut(bombSettleTimeElapsed / bombSettleDuration));
        bombOverlay.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 1),
            new Color(sr.color.r, sr.color.g, sr.color.b, 0),
            EasingEquations.Easing.QuadEaseIn(bombSettleTimeElapsed / bombSettleDuration));
        moltenLines.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 1),
            new Color(sr.color.r, sr.color.g, sr.color.b, 0),
            EasingEquations.Easing.QuadEaseIn(bombSettleTimeElapsed / bombSettleDuration));
        if (bombSettleTimeElapsed >= bombSettleDuration) bombSettling = false;
    }


    public void SetSprite(int spriteIndex)
    {
        bombOverlay.sprite = destructorSprites[spriteIndex];
        sr.sprite = sprites[spriteIndex];
        shieldSr.sprite = shieldSprites[spriteIndex];
    }

    public void ShiftAlpha(float alpha)
    {
        targetColor = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }

    public void SetAlpha(float alpha)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        targetColor = sr.color;
    }

    public void IncrementSortingOrder(int inc)
	{
		highlightSr.sortingOrder += inc;
        sortingGroup.sortingOrder += inc;
        shieldSr.sortingOrder += inc;
        //maskSr.sortingOrder += inc;
        //bombOverlay.sortingOrder += inc;
        //moltenLines.sortingOrder += inc;
	}

    public void SetSortingOrder(int sortingOrder)
    {
        int maskDiff = sortingGroup.sortingOrder - highlightSr.sortingOrder;
        int shieldDiff = sortingGroup.sortingOrder - shieldSr.sortingOrder;
        //int bombDiff = sr.sortingOrder - bombOverlay.sortingOrder;
        //int moltenDiff = sr.sortingOrder - moltenLines.sortingOrder;
        //sr.sortingOrder = sortingOrder;
        //highlightSr.sortingOrder = sortingOrder - maskDiff;
        sortingGroup.sortingOrder = sortingOrder;
        //shieldSr.sortingOrder = sortingOrder - shieldDiff;
        //maskSr.sortingOrder = sortingOrder - maskDiff;
        //bombOverlay.sortingOrder = sortingOrder - bombDiff;
        //moltenLines.sortingOrder = sortingOrder - moltenDiff;
    }

    public void SortOnSelection(bool selected)
    {
        if (selected) sortingGroup.sortingLayerName = "SelectedPiece";
        else sortingGroup.sortingLayerName = "Default";
    }

    public void PrintCoord()
    {
        Debug.Log("X: " + coord.x + ", Y: " + coord.y);
    }

    public void StartSettlingToNormalPiece()
    {
        bombSettling = true;
        bombSettleTimeElapsed = 0;
    }

    public void SetShieldStatus(bool status)
    {
        shieldSr.enabled = status;
    }

    public void SetShieldAlpha(float alpha)
    {
        shieldSr.color = new Color(shieldSr.color.r, shieldSr.color.g, shieldSr.color.b,
            alpha);
    }

    void PulseRed()
    {
        redPulseTimer += Time.deltaTime;
        Color redColor = new Color(redPulseColor.r, redPulseColor.g, redPulseColor.b, baseColor.a);
        if (toRed)
        {
            sr.color = Color.Lerp(baseColor, redColor, EasingEquations.Easing.QuadEaseIn(
                redPulseTimer / redPulsePeriod));
        }
        else
        {
            sr.color = Color.Lerp(redColor, baseColor, EasingEquations.Easing.QuadEaseOut(
                redPulseTimer / redPulsePeriod));
        }

        if (redPulseTimer >= redPulsePeriod)
        {
            toRed = !toRed;
            redPulseTimer = 0;
        }
    }
}
