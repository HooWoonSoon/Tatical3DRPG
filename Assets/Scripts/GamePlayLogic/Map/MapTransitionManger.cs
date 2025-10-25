using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransitionSnapShot
{
    public MapData mapData;
    public Transform lastRememberNode;
    public PlayerCharacter player;
}

public class MapTransitionManger : Entity
{
    public GameObject deploymentNotificationPanel;

    public event Action onConfrimCallback;
    public event Action onCancelCallback;

    public Action<MapData> onRequireDeployment;
    private TransitionSnapShot transitionSnapShot;

    public static MapTransitionManger instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        deploymentNotificationPanel.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnEnter();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            OnCancel();
        }
    }

    public void SaveSnapShot(MapData mapData, Transform returnTransfrom, PlayerCharacter player)
    {
        transitionSnapShot = new TransitionSnapShot()
        {
            mapData = mapData,
            lastRememberNode = returnTransfrom,
            player = player
        };
    }

    public void RequestMapTransition(string mapID, Vector3 teleportPos, Vector3 returnPos, 
        PlayerCharacter player, Action confirmAction = null, Action cancelAction = null)
    {
        List<PlayerCharacter> playerCharacters = FindAllTeamMembers(player);

        MapData mapData = MapManager.instance.GetMapData(mapID);

        if (!mapData.requireDeployment)
        {
            deploymentNotificationPanel.SetActive(false);
            ExecuteSwitchMapAndTeleport(mapData, teleportPos, playerCharacters);
            confirmAction?.Invoke();
        }
        if (mapData.requireDeployment)
        {
            deploymentNotificationPanel.SetActive(true);
        }

        onConfrimCallback = () =>
        {
            deploymentNotificationPanel.SetActive(false);
            MapManager.instance.SwitchMap(mapData);
            confirmAction?.Invoke();
            onRequireDeployment?.Invoke(mapData);
        };

        onCancelCallback = () =>
        {
            transitionSnapShot = null;
            deploymentNotificationPanel.SetActive(false);
            StartCoroutine(player.MoveToPositionCoroutine(returnPos, () =>
            { 
                cancelAction?.Invoke();
            }));
        };
    }

    private void ExecuteSwitchMapAndTeleport(MapData mapData, Vector3 teleportPos, List<PlayerCharacter> playerCharacters)
    {
        MapManager.instance.SwitchMap(mapData);
        GameNode targetNode = world.GetNode(teleportPos);
        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            playerCharacter.TeleportToNodeFree(targetNode);
        }
    }

    public void RequestReturnPreviousMap(Action confrimAction = null, Action cancelAction = null)
    {
        if (transitionSnapShot == null || transitionSnapShot.mapData == null || transitionSnapShot.lastRememberNode == null)
            return;
        
        PlayerCharacter player = transitionSnapShot.player;
        List<PlayerCharacter> playerCharacters = FindAllTeamMembers(player);
        Debug.Log($"Player character {playerCharacters.Count}");

        onConfrimCallback = () =>
        {
            ExecuteSwitchMapAndTeleport(transitionSnapShot.mapData, transitionSnapShot.lastRememberNode.position, playerCharacters);
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                playerCharacters[i].gameObject.SetActive(true);
            }
            CameraMovement.instance.ChangeFollowTarget(player.transform);
            confrimAction?.Invoke();
        };

        onCancelCallback = () =>
        {
            cancelAction?.Invoke();
        };
    }

    public void ClearEventCallback(Action action = null)
    {
        onConfrimCallback = null;
        onCancelCallback = null;
        action?.Invoke();
    }

    private void OnEnter()
    {
        onConfrimCallback?.Invoke();
        onConfrimCallback = null;
        onCancelCallback = null;
    }

    private void OnCancel()
    {
        onConfrimCallback = null;
        onCancelCallback?.Invoke();
        onCancelCallback = null;
    }

    private List<PlayerCharacter> FindAllTeamMembers(PlayerCharacter player)
    {
        List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
        TeamDeployment team = player.currentTeam;
        foreach (CharacterBase character in team.teamCharacter)
        {
            PlayerCharacter playerCharacter = character.GetComponent<PlayerCharacter>();
            if (playerCharacter == null) { continue; }
            playerCharacters.Add(playerCharacter);
        }
        return playerCharacters;
    }
}
