using System;
using System.Collections.Generic;
using UnityEngine;

public class MapTransitionManger : Entity
{
    public GameObject deploymentNotificationPanel;
    public Action onConfrimCallback;
    public Action<MapData> onRequireDeployment;

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
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancel();
        }
    }

    public void RequestMapTransition(MapData mapData, Vector3 teleportPos, 
        PlayerCharacter character, Action action = null)
    {
        List<PlayerCharacter> playerCharacters = FindAllTeamMember(character);

        if (!mapData.requireDeployment)
        {
            deploymentNotificationPanel.SetActive(false);
            ExecuteSwitchMapAndTeleport(mapData, teleportPos, playerCharacters);
            action?.Invoke();
        }
        if (mapData.requireDeployment)
        {
            deploymentNotificationPanel.SetActive(true);
        }

        onConfrimCallback = () =>
        {
            deploymentNotificationPanel.SetActive(false);
            ExecuteSwitchMap(mapData);
            action?.Invoke();
            onRequireDeployment?.Invoke(mapData);
        };
    }

    public void ExecuteSwitchMap(MapData mapData)
    {
        MapManager.instance.SwitchMap(mapData.mapDataPath, mapData.mapModel);
    }

    public void ExecuteSwitchMapAndTeleport(MapData mapData, Vector3 teleportPos, List<PlayerCharacter> playerCharacters)
    {
        ExecuteSwitchMap(mapData);
        GameNode targetNode = world.GetNode(teleportPos);
        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            playerCharacter.TeleportToNode(targetNode);
        }
    }

    private void OnEnter()
    {
        onConfrimCallback?.Invoke();
        onConfrimCallback = null;
    }

    private void OnCancel()
    {
        deploymentNotificationPanel.SetActive(false);
        onConfrimCallback = null;
    }

    private List<PlayerCharacter> FindAllTeamMember(PlayerCharacter player)
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
