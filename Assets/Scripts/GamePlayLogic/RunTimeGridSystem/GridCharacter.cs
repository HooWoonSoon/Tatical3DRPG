using System.Collections.Generic;
using UnityEngine;

public class GridCharacter : Entity
{
    public List<CharacterBase> characters = new List<CharacterBase>();
    private bool updateCharacter;
    public static GridCharacter instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        SubscribeAllNodes();
    }

    private void LateUpdate()
    {
        if (updateCharacter)
        {
            ResetAllGridCharacter();
            foreach (CharacterBase character in characters)
            {
                UpdateGridCharacter(character);
            }
            updateCharacter = false;
        }
    }
    public void SubscribeAllNodes()
    {
        foreach (var kvp in world.loadedNodes)
        {
            GameNode node = kvp.Value;
            if (node != null)
                node.onWorldNodesChange += OnCharacterChanged;
        }
        updateCharacter = true;
    }
    private void OnCharacterChanged(object sender, GameNode.OnWorldNodesChange e)
    {
        updateCharacter = true;
    }
    private void UpdateGridCharacter(CharacterBase character)
    {
        if (character == null || !character.gameObject.activeSelf)
            return;

        Vector3Int position = Utils.RoundXZFloorYInt(character.transform.position);
        SetGridCharacter(position, character);
    }
    public void SetGridCharacter(Vector3Int characterPos, CharacterBase character)
    {
        if (character == null || !character.gameObject.activeSelf)
            return;

        GameNode gameNode = world.GetNode(characterPos);
        if (gameNode == null) 
        { 
            Debug.Log("Invalid node update"); 
            return; 
        }

        if (gameNode != null)
        {
            gameNode.SetUnitGridCharacter(character);
        }
    }
    private void ResetAllGridCharacter()
    {
        foreach (GameNode node in world.loadedNodes.Values)
        {
            if (node != null)
            {
                node.character = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (world == null) return;

        foreach (GameNode node in world.loadedNodes.Values)
        {
            if (node.character != null)
            {
                Gizmos.color = new Color(0, 0, 0, 0.5f);
                Gizmos.DrawCube(node.GetVector() + Vector3.up, Vector3.one);
            }
        }
    }
}
