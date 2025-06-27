using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TeamFollower
{
    public CharacterBase character;
    public CharacterBase targetToFollow;

    public void Initialize(CharacterBase unitCharacter, CharacterBase targetToFollow)
    {
        this.character = unitCharacter;
        this.targetToFollow = targetToFollow;
    }
}