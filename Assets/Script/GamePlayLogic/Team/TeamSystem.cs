using System.Collections.Generic;
using UnityEngine;

public class TeamSystem : MonoBehaviour
{
    public TeamDeployment teamDeployment;
    public List<TeamFollower> teamFollowers;
    private List<Character> unlinkCharacters = new List<Character>();
    public Character currentLeader { get; private set; }

    [SerializeField] private int spacingDistance = 2;
    [SerializeField] private int historyLimit = 15;

    public static TeamSystem instance;

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
        if (Input.GetKeyDown(KeyCode.C))
        {
            //  Debug
            TeamFollowPathFinding.instance.TeamSortPathFinding(teamFollowers, spacingDistance);

            for (int i = 0; i < teamFollowers.Count; i++)
            {
                teamFollowers[i].unitCharacter.EnablePathFinding();
            }
        }
        
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            FollowWithNearIndexMember(teamFollowers[i].unitCharacter, teamFollowers[i].targetToFollow);
            if (teamFollowers[i].unitCharacter.xInput != 0 || teamFollowers[i].unitCharacter.zInput != 0)
            {
                teamFollowers[i].unitCharacter.UpdateHistory();
            }
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
        for (int i = 0; i < teamDeployment.teamCharacter.Count; i++)
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
    public void InsertTeamFollower(Character unitCharacter)
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
        currentLeader = null;

        for (int i = 0; i < teamFollowers.Count; i++)
        {
            Character unitCharacter = teamFollowers[i].unitCharacter;

            if (teamFollowers[i].unitCharacter.index == 0) 
            { 
                unitCharacter.isLeader = true;
                currentLeader = unitCharacter;
            }
            else { unitCharacter.isLeader = false; }
        }
    }
    #endregion

    #region External call manage team follower
    // Summary
    //      External call to remove the character from the team follower list
    //      and add it to the unlink character list.
    public void RemoveUnlinkCharacterFromTeam(Character unitCharacter)
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
    public void AddCharacterToUnlinkList(Character unitCharacter)
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
    private void FollowWithNearIndexMember(Character member, Character follower)
    {
        if (member.isLink == false || member.isLeader) return;
        GetFollowTargetDirection(member, follower, out Vector3 direciton);

        member.SetVelocity(direciton.x, direciton.z);
    }

    //  Summary
    //      Get the direction to follow the target character.
    private void GetFollowTargetDirection(Character member, Character follower, out Vector3 direction)
    {
        direction = Vector3.zero;

        if (member == null || follower.positionHistory.Count < 2) return;

        List<Vector3> history = follower.positionHistory;

        for (int i = history.Count - 1; i > 0; i--)
        {
            float distance = Vector3.Distance(member.transform.position, follower.transform.position);
            if (distance >= spacingDistance)
            {
                Vector3 targetPosition = history[i];
                direction = (targetPosition - member.transform.position).normalized;
                return;
            }
        }
    }
    #endregion
}
