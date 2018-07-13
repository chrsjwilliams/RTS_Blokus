﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class Dynamo : TechBuilding
{
    private const float resourceGainIncrementMultiplier = 0.15f;

    public Dynamo() : base(0)
    {
        label = "Dynamo";

        buildingType = BuildingType.DYNAMO;
    }

    public override void OnClaimEffect(Player player)
    {
        player.AugmentResourceGainFactor(resourceGainIncrementMultiplier);

    }

    public override void OnLostEffect()
    {
        owner.AugmentResourceGainFactor(-resourceGainIncrementMultiplier);
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
        return "Dynamo";
    }

    public override string GetDescription()
    {
        return "+" + 100 * resourceGainIncrementMultiplier + "%" +
            " Energy Recharge Rate";
    }

}
