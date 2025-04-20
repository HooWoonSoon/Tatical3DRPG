using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TeamFollowSystem : MonoBehaviour
{
    public TeamDeployment teamDeployment;
    public List<TeamFollower> teamFollowers;
    private List<UnitCharacter> unlinkCharacters = new List<UnitCharacter>();

    [SerializeField] private float distanceThreshold = 2f;
    [SerializeField] private int historyLimit = 15;
    public float speed = 5f;

    public List<Vector3> leaderPositionHistory = new List<Vector3>();
    public static TeamFollowSystem instance;

    private bool isLeaderMoving = false;

    private void OnEnable()
    {
        TeamEvent.OnLeaderChanged += SetLeader;
        TeamEvent.OnTeamSortExchange += SortTeamFollower;
        TeamEvent.OnTeamSortExchange += ClearAllHistory;
        TeamMovementControllerE.OnMovementStatusChanged += OnMovementStatusChanged;
        CharacterTestingEMove.OnMovementStatusChanged += OnMovementStatusChanged;
    }

    private void OnDisable()
    {
        TeamEvent.OnLeaderChanged -= SetLeader;
        TeamEvent.OnTeamSortExchange -= SortTeamFollower;
        TeamEvent.OnTeamSortExchange -= ClearAllHistory;
        TeamMovementControllerE.OnMovementStatusChanged -= OnMovementStatusChanged;
        CharacterTestingEMove.OnMovementStatusChanged -= OnMovementStatusChanged;
    }

    private void Awake()
    {
        instance = this;
        InitializeTeamFollower();
    }

    private void Start()
    {
        SetLeader();
    }

    private void Update()
    {
        if (isLeaderMoving)
        {
            for (int i = 0; i < teamFollowers.Count; i++)
            {
                teamFollowers[i].unitCharacter.UpdateHistory();

                if (i > 0)
                {
                    FollowWithLeader(teamFollowers[i].unitCharacter, teamFollowers[i].targetToFollow);
                }
            }
        }
    }

    public void InitializeTeamFollower()
    {
        for (int i = 0; i < teamDeployment.teamCharacter.Length; i++)
        {
            if (i == 0)
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], null);
            else
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], teamDeployment.teamCharacter[i - 1]);
        }
    }

    private void ClearAllHistory()
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            teamFollowers[i].unitCharacter.CleanAllHistory();
        }
    }

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
        SetLeader();
    }
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

    public void SetLeader()
    {
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            UnitCharacter unitCharacter = teamFollowers[i].unitCharacter;

            if (teamFollowers[i].unitCharacter.index == 0) { unitCharacter.isLeader = true; }
            else { unitCharacter.isLeader = false; }
        }
    }

    private void OnMovementStatusChanged(bool isMoving)
    {
        isLeaderMoving = isMoving;
    }

    private void FollowWithLeader(UnitCharacter unitCharacter, UnitCharacter targetToFollow)
    {
        if (unitCharacter.isLink == false) return;

        Vector3 direciton = GetFollowTargetDirection(unitCharacter, targetToFollow);
        if (direciton != Vector3.zero)
        {
            unitCharacter.FacingDirection(direciton);
            unitCharacter.transform.position = Vector3.MoveTowards(unitCharacter.transform.position, unitCharacter.transform.position + direciton, speed * Time.deltaTime);
        }
    }

    private Vector3 GetFollowTargetDirection(UnitCharacter unitCharacter, UnitCharacter targetToFollow)
    {
        if (unitCharacter == null) return Vector3.zero;
        if (targetToFollow.positionHistory.Count < 2) return Vector3.zero;

        List<Vector3> history = targetToFollow.positionHistory;
        float accumulateDistance = 0;

        for (int i = history.Count - 1; i > 0; i--)
        {
            accumulateDistance += Vector3.Distance(history[i], history[i - 1]);
            if (accumulateDistance >= distanceThreshold)
            {
                Vector3 targetPosition = history[i - 1];
                Vector3 direction = (targetPosition - unitCharacter.transform.position).normalized;
                
                return direction;
            }
        }

        return Vector3.zero;
    }

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

    public void AddCharacterToUnlinkList(UnitCharacter unitCharacter)
    {
        if (!unlinkCharacters.Contains(unitCharacter))
        {
            unlinkCharacters.Add(unitCharacter);
        }
    }

    public void InsertCharcterToTeam(UnitCharacter unitCharacter)
    {
        TeamFollower teamFollower = new TeamFollower();
        teamFollower.unitCharacter = unitCharacter;
        unlinkCharacters.Remove(unitCharacter);

        teamFollowers.Add(teamFollower);
        SortTeamFollower();
    }
}
