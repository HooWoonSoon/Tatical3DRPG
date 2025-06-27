using System.Collections.Generic;
using UnityEngine;

public class PlayerTeamSystem : Entity
{
    public TeamDeployment teamDeployment;
    public List<TeamFollower> linkMembers;
    private List<CharacterBase> unlinkMember = new List<CharacterBase>();
    public CharacterBase currentLeader { get; private set; }
    private List<PathRoute> teamPathRoutes = new List<PathRoute>();

    public int spacingDistance = 2;
    [SerializeField] private int historyLimit = 15;

    public TeamStateMachine stateMachine;
    public TeamFreeControlState teamFreeControlState { get; private set; }
    public TeamSortPathFindingState teamSortPathFindingState { get; private set; }

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
        stateMachine = new TeamStateMachine();
        
        teamFreeControlState = new TeamFreeControlState(stateMachine, this);
        teamSortPathFindingState = new TeamSortPathFindingState(stateMachine, this);
    }

    protected override void Start()
    {
        base.Start();
        SetTeamFollowerLeader();
        stateMachine.Initialize(teamFreeControlState);
    }

    private void Update()
    {
        stateMachine.currentPlayerTeamState.Update();
        
        Utils.GetMovementInput(out float inputX, out float inputZ);
        currentLeader.SetVelocity(inputX, inputZ);

        for (int i = 0; i < linkMembers.Count; i++)
        {
            FollowWithNearIndexMember(linkMembers[i].character, linkMembers[i].targetToFollow);
            if (linkMembers[i].character.xInput != 0 || linkMembers[i].character.zInput != 0)
            {
                linkMembers[i].character.UpdateHistory();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            stateMachine.ChangeState(teamSortPathFindingState);
        }

        Vector3 currentPosition = DDADectector.GetDDAWorldPosition(64, world.loadedNodes);
        //Debug.Log(currentPosition);
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int targetPosition = Utils.RoundXZFloorYInt(currentPosition);
            if (targetPosition == new Vector3Int(-1, -1, -1)) return;
            Debug.Log(world.loadedNodes[(targetPosition.x, targetPosition.y, targetPosition.z)].isWalkable);

            currentLeader.SetMovePosition(currentLeader, targetPosition);
        }
    }

    #region Initialize
    private void Initialize()
    {
        InitializeTeamFollower();
    }

    //  Summary
    //      Initialize the team follower list by setting the target to follow for each unit character.
    private void InitializeTeamFollower()
    {
        for (int i = 0; i < teamDeployment.teamCharacter.Count; i++)
        {
            if (i == 0)
                linkMembers[i].Initialize(teamDeployment.teamCharacter[i], null);
            else
                linkMembers[i].Initialize(teamDeployment.teamCharacter[i], teamDeployment.teamCharacter[i - 1]);

            linkMembers[i].character.historyLimit = historyLimit;
        }
    }
    #endregion

    #region Manage team follower
    //  Summary
    //      Sort the team follower list by the index of the unit character.
    private void SortTeamFollower()
    {
        List<TeamFollower> sortedList = new List<TeamFollower>();
        List<TeamFollower> unsortList = new List<TeamFollower>(linkMembers);

        if (unsortList.Count == 0) return;

        while (unsortList.Count > 0)
        {
            TeamFollower minFollower = unsortList[0];
            for (int i = 0; i < unsortList.Count; i++)
            {
                if (unsortList[i].character.index < minFollower.character.index)
                {
                    minFollower = unsortList[i];
                }
            }

            sortedList.Add(minFollower);
            unsortList.Remove(minFollower);
        }

        linkMembers = sortedList;
        RefreshTeamFollower();
        SetTeamFollowerLeader();
    }

    //  Summary
    //      Refresh the team follower list by setting the target to follow for each unit character.
    private void RefreshTeamFollower()
    {
        for (int i = 0; i < linkMembers.Count; i++)
        {
            if (i == 0)
                linkMembers[i].Initialize(linkMembers[i].character, null);
            else
                linkMembers[i].Initialize(linkMembers[i].character, linkMembers[i - 1].character);
        }
    }

    //  Summary
    //      Add a new character to the team follower list and remove it from the unlink character.
    public void InsertTeamFollower(CharacterBase unitCharacter)
    {
        TeamFollower teamFollower = new TeamFollower();
        teamFollower.character = unitCharacter;
        unlinkMember.Remove(unitCharacter);

        linkMembers.Add(teamFollower);
        SortTeamFollower();
    }

    //  Summary
    //      Set the leader of the team by checking the index of each unit character.
    //      The first character in the list is set as the leader.
    public void SetTeamFollowerLeader()
    {
        currentLeader = null;

        for (int i = 0; i < linkMembers.Count; i++)
        {
            CharacterBase unitCharacter = linkMembers[i].character;

            if (linkMembers[i].character.index == 0) 
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
    public void RemoveUnlinkCharacterFromTeam(CharacterBase unitCharacter)
    {
        for (int i = 0; i < linkMembers.Count; i++)
        {
            if (linkMembers[i].character == unitCharacter)
            {
                linkMembers.RemoveAt(i);
                RefreshTeamFollower();
                break;
            }
        }
    }

    //  Summary
    //      External call to add a character to the unlink character list.
    public void AddCharacterToUnlinkList(CharacterBase unitCharacter)
    {
        if (!unlinkMember.Contains(unitCharacter)) { unlinkMember.Add(unitCharacter); }
    }
    #endregion

    private void ClearAllHistory()
    {
        for (int i = 0; i < linkMembers.Count; i++)
        {
            linkMembers[i].character.CleanAllHistory();
        }
    }

    #region Logic handle team follower
    //  Summary
    //      Follow the target character with the nearest index member.
    private void FollowWithNearIndexMember(CharacterBase member, CharacterBase follower)
    {
        if (member.isLink == false || follower == null) return;
        GetFollowTargetDirection(member, follower, out Vector3 direciton);

        member.SetVelocity(direciton.x, direciton.z);
    }

    //  Summary
    //      Get the direction to follow the target character.
    private void GetFollowTargetDirection(CharacterBase member, CharacterBase follower, out Vector3 direction)
    {
        direction = Vector3.zero;

        if (!currentLeader.isMoving) { return; }
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

    #region Team Sort Path Finding
    public void EnableTeamPathFinding()
    {
        this.teamPathRoutes.Clear();

        List<PathRoute> teamPathRoutes = GetTeamSortPath(linkMembers, spacingDistance);
        if (IsTeamSortPathAvaliable(teamPathRoutes))
        {
            this.teamPathRoutes = teamPathRoutes;
            for (int i = 0; i < teamPathRoutes.Count; i++)
            {
                teamPathRoutes[i].character.pathRoute = teamPathRoutes[i];
            }
            for (int i = 0; i < linkMembers.Count; i++)
            {
                CharacterBase character = linkMembers[i].character;
                character.stateMachine.ChangeSubState(character.movePathStateExplore);
            }
        }
    }
    public List<PathRoute> GetTeamSortPath(List<TeamFollower> linkMembers, int spacing)
    {
        List<PathRoute> teamPathRoute = new List<PathRoute>();

        Vector3Int lastTargetPosition = Utils.RoundXZFloorYInt(linkMembers[0].character.transform.position);

        for (int i = 1; i < linkMembers.Count; i++)
        {
            Vector3Int fromPosition = Utils.RoundXZFloorYInt(linkMembers[i].character.transform.position);

            if (IsWithinFollowRange(fromPosition, lastTargetPosition))
            {
                List<Vector3Int> unitRange = world.GetManhattas3DRange(lastTargetPosition, 2);
                teamPathRoute.Add(new PathRoute
                {
                    targetRangeList = unitRange,
                    character = linkMembers[i].character,
                });
            }
            else
            {
                Debug.Log($"Target {lastTargetPosition} is too far from {fromPosition}");
                return null;
            }

            bool foundPath = IsClosestTargetExist(fromPosition, teamPathRoute[i - 1]);
            if (!foundPath)
            {
                Debug.Log($"No path found from {fromPosition} to {lastTargetPosition} break!");
                return null;
            }
            lastTargetPosition = teamPathRoute[i - 1].targetPosition.Value;
        }
        return teamPathRoute;
    }

    //  Summary
    //      Sort the followerVectorRange based on distance to unit position
    private List<Vector3Int> SortTargetRangeByDistance(Vector3Int from, List<Vector3Int> targets)
    {
        var sorted = new List<Vector3Int>(targets);
        sorted.Sort((a, b) => Vector3.Distance(from, a).CompareTo(Vector3.Distance(from, b)));
        return sorted;
    }

    private bool IsWithinFollowRange(Vector3Int fromPosition, Vector3Int targetPosition, float maxDistance = 16f)
    {
        return Vector3.Distance(fromPosition, targetPosition) <= maxDistance;
    }
    private bool IsClosestTargetExist(Vector3Int fromPosition, PathRoute pathRoute)
    {
        if (pathRoute.targetRangeList == null || pathRoute.targetRangeList.Count == 0)
            return false;

        var sortedTarget = SortTargetRangeByDistance(fromPosition, pathRoute.targetRangeList);

        for (int i = 0; i < sortedTarget.Count; i++)
        {
            List<Vector3> pathVectorList = pathFinding.GetPathRoute(fromPosition, sortedTarget[i]).pathRouteList;

            bool existSameTarget = IsTargetPositionExist(pathRoute, sortedTarget[i]);
            if (existSameTarget == true)
            {
                Debug.Log($"Target {sortedTarget[i]} is already exist");
                continue;
            }

            if (pathVectorList.Count != 0)
            {
                pathRoute.pathRouteList = pathVectorList;
                pathRoute.targetPosition = sortedTarget[i];
                pathRoute.pathIndex = 0;
                Debug.Log($" {fromPosition} to target {pathRoute.targetPosition}");
                return true;
            }
        }
        return false;
    }
    private bool IsTargetPositionExist(PathRoute pathRoutes, Vector3 targetPosition)
    {
        if (pathRoutes.targetPosition.HasValue &&
            targetPosition == pathRoutes.targetPosition.Value)
        {
            return true;
        }
        return false;
    }
    private bool IsTeamSortPathAvaliable(List<PathRoute> teamPathRoutes)
    {
        if (teamPathRoutes.Count == 0) return false;

        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (!teamPathRoutes[i].targetPosition.HasValue)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (teamPathRoutes.Count == 0) return;
        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (teamPathRoutes[i].targetPosition.Value == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawCube(teamPathRoutes[i].targetPosition.Value, Vector3.one);
        }
    }
}
