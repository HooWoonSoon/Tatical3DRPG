using System.Collections.Generic;
using UnityEngine;

public class TeamFollowSystem : MonoBehaviour
{
    public TeamDeployment teamDeployment;
    public TeamFollower[] teamFollowers;

    [SerializeField] private float characterSpacing = 2f;
    [SerializeField] private int historyLimit = 15;
    public float speed = 5f;

    public UnitCharacter leader;
    public List<Vector3> leaderPositionHistory = new List<Vector3>();
    public static TeamFollowSystem instance;

    private bool isLeaderMoving = false;

    private void OnEnable()
    {
        TeamEvent.OnLeaderChanged += SetLeader;
        CharacterTestingEMove.OnMovementStatusChanged += OnMovementStatusChanged;
    }

    private void OnDisable()
    {
        TeamEvent.OnLeaderChanged -= SetLeader;
        CharacterTestingEMove.OnMovementStatusChanged -= OnMovementStatusChanged;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeTeamFollower();
        SetLeader();
    }

    private void Update()
    {
        if (isLeaderMoving)
        {
            for (int i = 0; i < teamFollowers.Length; i++)
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
        for (int i = 0; i < teamFollowers.Length; i++)
        {
            if (i == 0)
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], null);
            else
                teamFollowers[i].Initialize(teamDeployment.teamCharacter[i], teamDeployment.teamCharacter[i - 1]);
        }
    }

    public void SetLeader()
    {
        for (int i = 0; i < teamFollowers.Length; i++)
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
            if (accumulateDistance >= characterSpacing)
            {
                Vector3 targetPosition = history[i - 1];
                Vector3 direction = (targetPosition - unitCharacter.transform.position).normalized;
                
                return direction;
            }
        }

        return Vector3.zero;
    }
}
