using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TeamFollower
{
    public UnitCharacter unitCharacter;
    public UnitCharacter targetToFollow;

    public void Initialize(UnitCharacter unitCharacter, UnitCharacter targetToFollow)
    {
        this.unitCharacter = unitCharacter;
        this.targetToFollow = targetToFollow;
    }
}