﻿using UnityEngine;
using System.Collections;

public class ShieldedPieces : TechBuilding
{
    public const float ShieldDuration = 6f;

    public ShieldedPieces() : base(0)
    {
        buildingType = BuildingType.SHIELDEDPIECES;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleShieldedPieces(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleShieldedPieces(false);
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Shielded Pieces";
    }

    protected override string GetDescription()
    {
        return "Newly placed pieces gain a temporary shield. They can't be destroyed until " +
            ShieldDuration + " seconds after placement.";
    }
}
