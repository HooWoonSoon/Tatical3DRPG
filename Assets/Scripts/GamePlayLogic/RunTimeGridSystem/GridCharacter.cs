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
    private void SubscribeAllNodes()
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
        Vector3Int position = Utils.RoundXZFloorYInt(character.transform.position);
        SetGridCharacter(position, character);
    }
    public void SetGridCharacter(Vector3Int characterPos, CharacterBase character)
    {
        GameNode gameNode = world.GetNode(characterPos);
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
}
