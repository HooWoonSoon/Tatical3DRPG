using System.Collections.Generic;
using UnityEngine;

public class GridSystem : Entity
{
    public List<CharacterBase> characters = new List<CharacterBase>();
    private bool updateCharacter = true;
    protected override void Start()
    {
        base.Start();
    }
    private void Update()
    {
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
    private void OnCharacterChanged(object sender, GameNode.OnWorldNodesChange e)
    {
        updateCharacter = true;
    }
    private void UpdateGridCharacter(CharacterBase character)
    {
        Vector3Int position = Utils.RoundXZFloorYInt(character.transform.position);
        SetGridObject(position, character);
    }
    public void SetGridObject(Vector3Int characterPos, CharacterBase character)
    {
        GameNode gameNode = world.GetNode(characterPos);
        if (gameNode != null)
        {
            gameNode.SetUnitGridCharacter(character);
            gameNode.onWorldNodesChange += OnCharacterChanged;  
        }
    }
    public void ResetAllGridCharacter()
    {
        for (int x = 0; x < world.worldMaxX; x++)
        {
            for (int y = 0; y < world.worldHeight; y++)
            {
                for (int z = 0; z < world.worldMaxZ; z++)
                {
                    GameNode gameNode = world.GetNode(x, y, z);
                    if (gameNode != null)
                    {
                        gameNode.character = null;
                    }
                }
            }
        }
    }
}
