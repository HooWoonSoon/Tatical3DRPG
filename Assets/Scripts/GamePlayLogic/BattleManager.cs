using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleManager : Entity
{
    public static BattleManager instance { get; private set; }

    public List<CharacterBase> joinedBattleUnits = new List<CharacterBase>();

    public CTTurnUIGenerator ctTurnUIGenerator;

    [Header("Battle Cursor")]
    [SerializeField] private GameObject cursor;
    [SerializeField] private float heightOffset = 2.5f;
    private bool activateCursor = false;
    private float keyPressTimer;
    private float intervalPressTimer;

    public event Action onInfoTargetChanged;

    private void Awake()
    {
        instance = this;
    }
    protected override void Start()
    {
        base.Start();
        CTTimeline.instance.confirmCTTimeline += HandleBattle;
        cursor.SetActive(false);
    }
    private void Update()
    {
        if (activateCursor)
        {
            HandleInput(KeyCode.W, Vector3Int.forward, 0.25f, 0.025f);
            HandleInput(KeyCode.S, Vector3Int.back, 0.25f, 0.025f);
            HandleInput(KeyCode.A, Vector3Int.left, 0.25f, 0.025f);
            HandleInput(KeyCode.D, Vector3Int.right, 0.25f, 0.025f);
        }
    }

    private void HandleInput(KeyCode keyCode, Vector3Int direction, float initialTimer, float interval)
    {
        if (Input.GetKeyDown(keyCode))
        {
            keyPressTimer = 0;
            TryMove(direction);
        }
        if (Input.GetKey(keyCode))
        {
            keyPressTimer += Time.deltaTime;
            if (keyPressTimer > initialTimer)
            {
                intervalPressTimer += Time.deltaTime;
                if (intervalPressTimer >= interval)
                {
                    TryMove(direction);
                    intervalPressTimer = 0;
                }
            }
        }
    }
    private void TryMove(Vector3Int direction)
    {
        Vector3Int nodePos = Utils.RoundXZFloorYInt(cursor.transform.position);
        GameNode gameNode = world.GetHeightNodeWithCube(nodePos.x + direction.x, nodePos.z + direction.z);
        if (gameNode != null)
        {
            cursor.transform.position = gameNode.GetGameNodeVector() + new Vector3(0, heightOffset);
            CharacterBase character = gameNode.GetUnitGridCharacter();
            if (character != null)
            {
                ctTurnUIGenerator.TargetCursorCharacterUI(character);
            }
        }
    }
    public void HandleBattle()
    {
        CharacterBase character = CTTimeline.instance.GetCurrentCharacter();
        ctTurnUIGenerator.TargetCursorCharacterUI(character);
        cursor.SetActive(true);
        cursor.transform.position = Utils.RoundXZFloorYInt(character.transform.position);
        activateCursor = true;
    }

    public void SetJoinedBattleUnit(List<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits;
    }
}

