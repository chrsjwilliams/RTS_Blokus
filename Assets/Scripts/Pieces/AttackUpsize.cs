﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttackUpsize : TechBuilding
{
    public AttackUpsize() : base(0)
    {
        label = "Attack Upsize";

        buildingType = BuildingType.ATTACKUPSIZE;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleBiggerBombs(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleBiggerBombs(false);
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        OnClaimEffect(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Attack Upsize";
    }

    protected override string GetDescription()
    {
        return "Newly drawn <color=red>Attack</color> pieces are 4 blocks rather than 3";
    }
}
