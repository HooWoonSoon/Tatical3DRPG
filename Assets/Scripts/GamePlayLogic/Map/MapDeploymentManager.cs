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
    private Dictionary<CharacterBase, GameNode> occupiedNodes = new Dictionary<CharacterBase, GameNode>();

    [SerializeField] private GridCursor gridCursor;
    private GameNode lastNode;

    private CharacterBase lasSelectedCharacter;

    public Material previewMaterial;
    private GameObject previewCharacter;

    public event Action onStartDeployment;
    public event Action onEndDeployment;

    private bool enableEditing = false;
    public static MapDeploymentManager instance { get; private set;}

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        MapTransitionManger.instance.onRequireDeployment += StartDeployment;
    }

    private void Update()
    {
        if (!enableEditing) { return; }

        if (gridCursor.currentNode != lastNode)
        {
            lastNode = gridCursor.currentNode;

            if (previewCharacter != null && gridCursor.currentNode != null)
            {
                Vector3 offset = lasSelectedCharacter.transform.position - lasSelectedCharacter.GetCharacterNodePosition();
                previewCharacter.transform.position = gridCursor.currentNode.GetVector() + offset;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameNode targetNode = gridCursor.currentNode;
            CharacterBase character = targetNode.GetUnitGridCharacter();

            if (lasSelectedCharacter == null)
            {
                if (character != null)
                {
                    lasSelectedCharacter = character;
                    GeneratePreviewCharacter(character);
                }
            }
            else
            {         
                ReplaceCharacterNode(lasSelectedCharacter, targetNode);
                lasSelectedCharacter = null;
                DestroyPreviewModel();
            }
        }
    }

    public void StartDeployment(MapData mapData)
    {
        List<GameNode> deployableNode = FindDeployableNodes(mapData);
        this.deployableNodes = deployableNode;

        GridTilemapVisual.instance.SetAllTileSprite(world, GameNode.TilemapSprite.None);
        GridTilemapVisual.instance.SetTilemapSprites(this.deployableNodes, GameNode.TilemapSprite.TinyBlue);

        CasualPutGridCursorAtLoadedMap();

        onStartDeployment?.Invoke();

        if (mapData.presetTeams == null || mapData.presetTeams.Length == 0) { return; }

        PresetTeam[] presetTeams = mapData.presetTeams;

        for (int i = 0; i < presetTeams.Length; i++)
        {
            PresetUnit[] presetUnits = presetTeams[i].presetUnits;
            List<CharacterBase> teamCharacters = new List<CharacterBase>();

            for (int j = 0; j < presetUnits.Length; j++)
            {
                CharacterBase character = presetUnits[j].character;
                GameNode deploymentNode = world.GetNode(presetUnits[j].deployPos);
                if (deployableNode != null)
                {
                    character.gameObject.SetActive(true);
                    character.TeleportToNode(deploymentNode);
                    teamCharacters.Add(character);
                }
            }
            MapDeploymentUIManager.instance.InsertCharactersInMap(teamCharacters);
            TeamManager.instance.GenerateTeam(teamCharacters, presetTeams[i].teamType);
        }
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

        List<GameNode> availableNodes = deployableNodes.FindAll(n => !occupiedNodes.ContainsValue(n) && n.isWalkable 
        && n.GetUnitGridCharacter() == null);

        if (availableNodes.Count == 0)
        {
            Debug.LogWarning("No available nodes to deploy the character!");
            return;
        }

        GameNode randomNode = availableNodes[UnityEngine.Random.Range(0, availableNodes.Count)];
        character.gameObject.SetActive(true);
        character.SetSelfToNode(randomNode, 0.5f);
        occupiedNodes[character] = randomNode;

        Debug.Log($"Deployed {character.name} at {randomNode.x},{randomNode.y},{randomNode.z}");
    }

    public void RemoveCharacterDeployment(CharacterBase character)
    {
        if (!occupiedNodes.TryGetValue(character, out GameNode node)) return;

        node.SetUnitGridCharacter(null);
        character.gameObject.SetActive(false);
        occupiedNodes.Remove(character);

        if (lasSelectedCharacter == character)
        {
            lasSelectedCharacter = null;
            DestroyPreviewModel();
        }
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
            List<GameNode> availableNodes = deployableNodes.FindAll(n => !occupiedNodes.ContainsValue(n) && n.isWalkable);
            if (availableNodes.Count == 0)
            {
                Debug.LogWarning("No more available nodes for deployment!");
                break;
            }

            GameNode randomNode = availableNodes[UnityEngine.Random.Range(0, availableNodes.Count)];
            character.gameObject.SetActive(true);
            character.SetSelfToNode(randomNode, 0.5f);
            occupiedNodes.Add(character, randomNode);

            Debug.Log($"Deployed {character.name} at {randomNode.x},{randomNode.y},{randomNode.z}");
        }
    }

    #region Edit Character Deploy Placement
    //  Summary
    //      Replace selected character to target node, if target node occupied, exchange their position
    public void ReplaceCharacterNode(CharacterBase selectedCharacter, GameNode targetNode)
    {
        if (selectedCharacter == null || targetNode == null) return;
        if (!deployableNodes.Contains(targetNode)) { return; }

        CharacterBase targetNodeCharacter = targetNode.GetUnitGridCharacter();
        Debug.Log(targetNodeCharacter);
        if (targetNodeCharacter == null)
        {
            ChangeCharacterNode(selectedCharacter, targetNode);
        }
        else
        {
            ExchangeNodeCharacter(selectedCharacter, targetNodeCharacter);
        }
    }

    //  Summary
    //      Move a character to target node and modified its occupied node
    private void ChangeCharacterNode(CharacterBase character, GameNode targetNode)
    {
        GameNode currentNode = occupiedNodes[character];

        character.SetSelfToNode(targetNode, 0.5f);
        targetNode.SetUnitGridCharacter(character);
        occupiedNodes[character] = targetNode;
        currentNode.SetUnitGridCharacter(null);
        Debug.Log($"Moved {character.name} to {targetNode.x},{targetNode.y},{targetNode.z}");
    }

    //  Summary
    //      Swap two characters' positions on the grid and modified their occupied nodes
    private void ExchangeNodeCharacter(CharacterBase character, CharacterBase otherCharacter)
    {
        GameNode currentNode = occupiedNodes[character];
        GameNode otherNode = occupiedNodes[otherCharacter];

        character.SetSelfToNode(otherNode, 0.5f);
        otherCharacter.SetSelfToNode(currentNode, 0.5f);
        occupiedNodes[character] = otherNode;
        occupiedNodes[otherCharacter] = currentNode;
        currentNode.SetUnitGridCharacter(otherCharacter);
        otherNode.SetUnitGridCharacter(character);
        Debug.Log($"Swapped {character.name} <-> {otherCharacter.name}");
    }
    #endregion

    public void GeneratePreviewCharacter(CharacterBase character)
    {
        DestroyPreviewModel();

        Vector3 offset = character.transform.position - character.GetCharacterNodePosition();
        previewCharacter = Instantiate(character.characterModel);
        previewCharacter.transform.position = gridCursor.currentNode.GetVector() + offset;

        if (previewMaterial != null)
        {
            MeshRenderer[] renderers = previewCharacter.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material = previewMaterial;
            }
        }
    }

    public void DestroyPreviewModel()
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
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

    public void EndDeployment()
    {
        deployableNodes.Clear();
        occupiedNodes.Clear();
        lastNode = null;
        lasSelectedCharacter = null;
        EnableEditingMode(false);
        DestroyPreviewModel();
        ActivateMoveCursorAndHide(false, true);
        onEndDeployment?.Invoke();
    }

    public void SetGridCursorAt(GameNode target)
    {
        gridCursor.HandleGridCursor(target);
    }

    public void ActivateMoveCursorAndHide(bool active, bool hide)
    {
        gridCursor.ActivateMoveCursor(active, hide);
    }

    public void EnableEditingMode(bool active)
    {
        if (active) 
            enableEditing = true;
        else
            enableEditing = false;
    }
}

