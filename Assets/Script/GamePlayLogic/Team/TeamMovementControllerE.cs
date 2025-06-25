using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamMovementControllerE : MonoBehaviour
{
    private TeamSystem teamSystem;

    private void Start()
    {
        teamSystem = TeamSystem.instance;
    }

    private void Update()
    {
        Utils.GetMovementInput(out float inputX, out float inputZ);

        Character leader = teamSystem.currentLeader;
        leader.SetVelocity(inputX, inputZ);
    }
}