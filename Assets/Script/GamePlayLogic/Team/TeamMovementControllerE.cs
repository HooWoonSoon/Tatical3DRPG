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
        CheckHandlePathFinding(ref inputX, ref inputZ);

        Character leader = teamSystem.currentLeader;
        leader.SetVelocity(inputX, inputZ);
    }

    private void CheckHandlePathFinding(ref float inputX, ref float inputZ)
    {
        if (TeamFollowPathFinding.instance.isActivePathFinding)
        {
            inputX = 0;
            inputZ = 0;
            Debug.Log("Active PathFinding");
        }
    }
}