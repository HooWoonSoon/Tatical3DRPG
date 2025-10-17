using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class MapDeploymentManager : Entity
{
    public TeamDeployment teamDeployment;
    private List<GameNode> deployableNodes = new List<GameNode>();
    private HashSet<GameNode> occupiedNodes = new HashSet<GameNode>();

    public GridCursor gridCursor;
    public CharacterBase selectedCharacter;

    public Action onDeploymentTrigger;
    public static MapDeploymentManager instance;

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        MapTransitionManger.instance.onRequireDeployment += StartDeployment;
    }

    public void StartDeployment(MapData mapData)
    {
        List<GameNode> deployableNode = FindDeployableNodes(mapData);
        this.deployableNodes = deployableNode;

        GridTilemapVisual.instance.SetAllTileSprite(world, GameNode.TilemapSprite.None);
        GridTilemapVisual.instance.SetTilemapSprites(this.deployableNodes, GameNode.TilemapSprite.TinyBlue);
        DeploymentCharacterRandom();
        CasualPutGridCursorAtLoadedMap();

        onDeploymentTrigger?.Invoke();
    }


    //  Summary
    //      Find all deployable nodes from original map data
    private List<GameNode> FindDeployableNodes(MapData mapData)
    {
        List<GameNode> deployableNodes = new List<GameNode>();

        string mapDataPath = mapData.mapDataPath;
        string fullPath = Path.Combine(Application.persistentDataPath, mapDataPath);
        string json = File.ReadAllText(fullPath);
        List<GameNodeData> nodeDataList = JsonConvert.DeserializeObject<List<GameNodeData>>(json);
        foreach (GameNodeData nodeData in nodeDataList)
        {
            if (nodeData.isDeployable)
            {
                GameNode node = world.GetNode(nodeData.x, nodeData.y, nodeData.z);
                deployableNodes.Add(node);
            }
        }
        return deployableNodes;
    }

    public void RandomDeploymentCharacter(CharacterBase character)
    {
        if (character == null)
        {
            Debug.LogWarning("Character is null, cannot deploy!");
            return;
        }

        if (deployableNodes == null || deployableNodes.Count == 0)
        {
            Debug.LogWarning("No deployable nodes found!");
            return;
        }
        List<GameNode> availableNodes = deployableNodes.FindAll(n => !occupiedNodes.Contains(n) && n.isWalkable);
        if (availableNodes.Count == 0)
        {
            Debug.LogWarning("No available nodes to deploy the character!");
            return;
        }

        GameNode randomNode = availableNodes[UnityEngine.Random.Range(0, availableNodes.Count)];
        character.SetSelfToNode(randomNode, new Vector3(0, 0.5f, 0));
        occupiedNodes.Add(randomNode);

        Debug.Log($"Deployed {character.name} at {randomNode.x},{randomNode.y},{randomNode.z}");
    }

    public void DeploymentCharacterRandom()
    {
        List<CharacterBase> characters = new List<CharacterBase>();
        if (teamDeployment != null)
        {
            characters = teamDeployment.teamCharacter;
        }

        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning("No characters available for deployment!");
            return;
        }

        if (deployableNodes == null || deployableNodes.Count == 0)
        {
            Debug.LogWarning("No deployable nodes found!");
            return;
        }

        foreach (CharacterBase character in characters)
        {
            List<GameNode> availableNodes = deployableNodes.FindAll(n => !occupiedNodes.Contains(n) && n.isWalkable);
            if (availableNodes.Count == 0)
            {
                Debug.LogWarning("No more available nodes for deployment!");
                break;
            }

            GameNode randomNode = availableNodes[UnityEngine.Random.Range(0, availableNodes.Count)];
            character.SetSelfToNode(randomNode, new Vector3(0, 0.5f, 0));
            occupiedNodes.Add(randomNode);

            Debug.Log($"Deployed {character.name} at {randomNode.x},{randomNode.y},{randomNode.z}");
        }
    }


    private void CasualPutGridCursorAtLoadedMap()
    {
        if (deployableNodes.Count > 0)
        {
            GameNode node = deployableNodes[0];
            SetGridCursorAt(node);
        }
        ActivateMoveCursorAndHide(false, true);
    }

    public void SetGridCursorAt(GameNode target)
    {
        gridCursor.HandleGridCursor(target);
    }

    public void ActivateMoveCursorAndHide(bool active, bool hide)
    {
        gridCursor.ActivateMoveCursor(active, hide);
    }

    public List<GameNode> GetSelectableNodes()
    {
        return deployableNodes;
    }
}

