using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TeamFollowSystem : MonoBehaviour
{
    public TeamDeployment teamDeployment;
    public List<TeamFollower> teamFollowers;
    private List<UnitCharacter> unlinkCharacters = new List<UnitCharacter>();

    [SerializeField] private float spacingDistance = 2f;
    [SerializeField] private int historyLimit = 15;

    public static TeamFollowSystem instance;

    private void OnEnable()
    {
        TeamEvent.OnLeaderChanged += SetTeamFollowerLeader;
        TeamEvent.OnTeamSortExchange += SortTeamFollower;
        TeamEvent.OnTeamSortExchange += ClearAllHistory;
    }

    private void OnDisable()
    {
        TeamEvent.OnLeaderChanged -= SetTeamFollowerLeader;
        TeamEvent.OnTeamSortExchange -= SortTeamFollower;
        TeamEvent.OnTeamSortExchange -= ClearAllHistory;
    }

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        SetTeamFollowerLeader();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            FindCharacterNewSpacingPosition(teamFollowers);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            //  Debug
            TeamFollowPathFinding.instance.TeamFollowPathFindingRange(Utils.RoundXZFloorYInt(teamFollowers[1].unitCharacter.transform.position), 4);
            TeamFollowPathFinding.instance.CheckWithinRange(teamFollowers[0].unitCharacter);
            TeamFollowPathFinding.instance.FindClosetRange(teamFollowers[0].unitCharacter);
        }
        
        if (!TeamMovementControllerE.instance.IsLeaderMove(teamFollowers[0].unitCharacter)) { return; }

        for (int i = 0; i < teamFollowers.Count; i++)
        {
            teamFollowers[i].unitCharacter.UpdateHistory();
            FollowWithNearIndexMember(teamFollowers[i].unitCharacter, teamFollowers[i].targetToFollow);
        }
    }

    #region Initialize
    private void Initialize()
    {
        instance = this;
        InitializeTeamFollower();
    }

    //  Summary
    //      Initialize the team follower list by setting the target to follow for each unit character.
    private void InitializeTeamFollower()
    {
        for (int i = 0; i < teamDeployment.teamCharacter.Length; i++)
        {
            if (i == 0)
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], null);
            else
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], teamDeployment.teamCharacter[i - 1]);

            teamFollowers[i].unitCharacter.historyLimit = historyLimit;
        }
    }
    #endregion

    #region Manage team follower
    //  Summary
    //      Sort the team follower list by the index of the unit character.
    private void SortTeamFollower()
    {
        List<TeamFollower> sortedList = new List<TeamFollower>();
        List<TeamFollower> unsortList = new List<TeamFollower>(teamFollowers);

        if (unsortList.Count == 0) return;

        while (unsortList.Count > 0)
        {
            TeamFollower minFollower = unsortList[0];
            for (int i = 0; i < unsortList.Count; i++)
            {
                if (unsortList[i].unitCharacter.index < minFollower.unitCharacter.index)
                {
                    minFollower = unsortList[i];
                }
            }

            sortedList.Add(minFollower);
            unsortList.Remove(minFollower);
        }

        teamFollowers = sortedList;
        RefreshTeamFollower();
        SetTeamFollowerLeader();
    }

    //  Summary
    //      Refresh the team follower list by setting the target to follow for each unit character.
    private void RefreshTeamFollower()
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            if (i == 0)
                teamFollowers[i].Initialize(teamFollowers[i].unitCharacter, null);
            else
                teamFollowers[i].Initialize(teamFollowers[i].unitCharacter, teamFollowers[i - 1].unitCharacter);
        }
    }

    //  Summary
    //      Add a new character to the team follower list and remove it from the unlink character.
    public void InsertTeamFollower(UnitCharacter unitCharacter)
    {
        TeamFollower teamFollower = new TeamFollower();
        teamFollower.unitCharacter = unitCharacter;
        unlinkCharacters.Remove(unitCharacter);

        teamFollowers.Add(teamFollower);
        SortTeamFollower();
    }

    //  Summary
    //      Set the leader of the team by checking the index of each unit character.
    //      The first character in the list is set as the leader.
    public void SetTeamFollowerLeader()
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            UnitCharacter unitCharacter = teamFollowers[i].unitCharacter;

            if (teamFollowers[i].unitCharacter.index == 0) { unitCharacter.isLeader = true; }
            else { unitCharacter.isLeader = false; }
        }
    }
    #endregion

    #region External call manage team follower
    // Summary
    //      External call to remove the character from the team follower list
    //      and add it to the unlink character list.
    public void RemoveUnlinkCharacterFromTeam(UnitCharacter unitCharacter)
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            if (teamFollowers[i].unitCharacter == unitCharacter)
            {
                teamFollowers.RemoveAt(i);
                RefreshTeamFollower();
                break;
            }
        }
    }

    //  Summary
    //      External call to add a character to the unlink character list.
    public void AddCharacterToUnlinkList(UnitCharacter unitCharacter)
    {
        if (!unlinkCharacters.Contains(unitCharacter)) { unlinkCharacters.Add(unitCharacter); }
    }
    #endregion

    private void ClearAllHistory()
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            teamFollowers[i].unitCharacter.CleanAllHistory();
        }
    }

    #region Logic handle team follower
    //  Summary
    //      Follow the target character with the nearest index member.
    private void FollowWithNearIndexMember(UnitCharacter unitCharacter, UnitCharacter targetToFollow)
    {
        if (unitCharacter.isLink == false || unitCharacter.isLeader) return;
        GetFollowTargetDirection(unitCharacter, targetToFollow, out Vector3 direciton);
        if (direciton != Vector3.zero)
        {
            TeamMovementControllerE.instance.Move(direciton.x, direciton.z, unitCharacter);
            TeamMovementControllerE.instance.UpDownHill(direciton.x, direciton.z, unitCharacter);
        }
    }

    //  Summary
    //      Get the direction to follow the target character.
    private void GetFollowTargetDirection(UnitCharacter unitCharacter, UnitCharacter targetToFollow, out Vector3 direction)
    {
        direction = Vector3.zero;

        if (unitCharacter == null || targetToFollow.positionHistory.Count < 2) return;

        List<Vector3> history = targetToFollow.positionHistory;

        for (int i = history.Count - 1; i > 0; i--)
        {
            float distance = Vector3.Distance(unitCharacter.transform.position, targetToFollow.transform.position);
            if (distance >= spacingDistance)
            {
                Vector3 targetPosition = history[i];
                direction = (targetPosition - unitCharacter.transform.position).normalized;
                return;
            }
        }
    }
    #endregion


    private void FindCharacterNewSpacingPosition(List<TeamFollower> teamFollowers)
    {
        List<Vector3> newPositions = new List<Vector3>();
        Vector3 forwardDirection = Vector3.back;

        if (teamFollowers.Count > 1)
        {
            forwardDirection = (teamFollowers[0].unitCharacter.transform.position -
                                teamFollowers[1].unitCharacter.transform.position).normalized;
        }
        newPositions.Add(Utils.RoundXZFloorYInt(teamFollowers[0].unitCharacter.transform.position));

        for (int i = 1; i < teamFollowers.Count; i++)
        {
            Vector3 prevPosition = newPositions[i - 1];
            newPositions.Add(prevPosition - forwardDirection * spacingDistance);
        }

        TeamFollowPathFinding.instance.TeamMemberRefinding(teamFollowers, newPositions, out List<Vector3> pathTargetPos);
        
        //  Debug
        StartCoroutine(DelayedSetNewSpacingPosition(teamFollowers, pathTargetPos));
    }

    private IEnumerator DelayedSetNewSpacingPosition(List<TeamFollower> teamFollowers, List<Vector3> newPositions)
    {
        yield return new WaitForSeconds(5f);
        SetNewSpacingPosition(teamFollowers, newPositions);
    }

    private void SetNewSpacingPosition(List<TeamFollower> teamFollowers, List<Vector3> newPositions)
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            teamFollowers[i].unitCharacter.transform.position = newPositions[i];
            if (i == 0) continue;
            float spacing = Vector3.Distance(teamFollowers[i].unitCharacter.transform.position, teamFollowers[i].targetToFollow.transform.position);
            Debug.Log($"{teamFollowers[i].unitCharacter.index} to {teamFollowers[i].targetToFollow.index} spacing position = {spacing}");
        }
    }
}
