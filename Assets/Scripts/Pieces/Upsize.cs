﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class Upsize : TechBuilding
{
    public Upsize() : base(0)
    {
        label = "Upsize";
        buildingType = BuildingType.UPSIZE;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleBiggerBricks(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleBiggerBricks(false);
    }

    public override void OnClaim(Player player)
    {
        OnClaimEffect(player);
        base.OnClaim(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Upsize";
    }

    public override string GetDescription()
    {
        return "Draw bigger pieces.";
    }
}