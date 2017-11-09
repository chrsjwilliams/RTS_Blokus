﻿using UnityEngine;
using System.Collections;

public class Mine : Blueprint
{
    public Mine(Player player_) : base(BuildingType.MINE, player_)
    {
        baseResourceIncrementPeriod = 3f;
        baseResourcesPerIncrement = 5;
    }

    public override void Update()
    {
        UpdateResourceMeter();
    }
}
