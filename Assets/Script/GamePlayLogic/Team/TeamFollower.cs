using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TeamFollower
{
    public Character unitCharacter;
    public Character targetToFollow;

    public void Initialize(Character unitCharacter, Character targetToFollow)
    {
        this.unitCharacter = unitCharacter;
        this.targetToFollow = targetToFollow;
    }
}