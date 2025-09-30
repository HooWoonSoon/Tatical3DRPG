using System;
using System.Collections.Generic;
using UnityEngine;

public class CTTimeline : MonoBehaviour
{
    [Serializable]
    public class CharacterTacticsTime
    {
        public CharacterBase character;
        public int increaseValue;
        public int CTValue = 0;
        public int accumulatedTime = 0;
        public bool isQueue;

        private const int BASE_INCREASE = 2;

        public CharacterTacticsTime(CharacterBase character)
        {
            this.character = character;
            increaseValue = character.data.speed + BASE_INCREASE;
            CTValue = 0;
            accumulatedTime = 0;
            isQueue = false;
        }
        public void IncreaseCT()
        {
            CTValue += increaseValue;
        }
        public void CompleteCT()
        {
            accumulatedTime++;
            increaseValue /= (accumulatedTime + 1);
            CTValue -= 100;
            isQueue = true;
        }
        public void Reset()
        {
            increaseValue = character.data.speed + BASE_INCREASE;
            CTValue = 0;
            accumulatedTime = 0;
            isQueue = false;
        }
    }

    public Dictionary<CharacterBase, CharacterTacticsTime> battleCharacter = new Dictionary<CharacterBase, CharacterTacticsTime>();
    private HashSet<CharacterBase> lastBattleCharacter = new HashSet<CharacterBase>();
    public List<CTTurn> cTTurn = new List<CTTurn>();
    private const int MAX_TURNS = 3;

    private CTTurn currentTurn;
    private int currentTurnIndex = 0;
    private int currentNumberIndex = 0;
    private CharacterBase currentCharacter;

    [SerializeField] private CTTurnUIGenerator cTTurnUIGenerator;
    [SerializeField] private UITransitionToolkit uITransitionToolkit;
    public event Action confirmCTTimeline;
    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NextCharacter();
        }
    }
    public void SetJoinedBattleUnit(List<CharacterBase> characters)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            InsertCharacter(characters[i]);
        }
    }
    private void InsertCharacter(CharacterBase character)
    {
        if (!battleCharacter.ContainsKey(character))
        {
            CharacterTacticsTime tactics = new CharacterTacticsTime(character);
            battleCharacter.Add(character, tactics);
        }
    }
    public void SetupTimeline()
    {
        if (battleCharacter.Count == 0) { return; }

        HashSet<CharacterBase> characters = new HashSet<CharacterBase>(battleCharacter.Keys);
        if (lastBattleCharacter.SetEquals(characters)) { return; }

        lastBattleCharacter = characters;
        cTTurn.Clear();

        for (int i = 0; i < MAX_TURNS; i++)
        {
            List<CharacterBase> completeQueue = GetCalculateCTQueue();
            CTTurn turnHistory = new CTTurn(completeQueue, i);
            cTTurn.Add(turnHistory);

            //  Reset all battle character last turn accumulated value
            foreach (var tactics in battleCharacter.Values)
            {
                tactics.Reset();
            }
        }
        currentTurn = cTTurn[0];
        currentCharacter = currentTurn.cTTimelineQueue[0];
        confirmCTTimeline?.Invoke();
    }
    private bool IsAllCharacterQueue(List<CharacterTacticsTime> tacticsList)
    {
        foreach (CharacterTacticsTime tactics in tacticsList)
        {
            if (!tactics.isQueue) { return false; }
        }
        return true;
    }
    private List<CharacterBase> GetCalculateCTQueue()
    {
        List<CharacterBase> cTTimelineQueue = new List<CharacterBase>();
        List<CharacterTacticsTime> tacticsList = new List<CharacterTacticsTime>(battleCharacter.Values);

        while (!IsAllCharacterQueue(tacticsList))
        {
            foreach (CharacterTacticsTime tactics in tacticsList)
            {
                tactics.IncreaseCT();
                if (tactics.CTValue >= 100)
                {
                    cTTimelineQueue.Add(tactics.character);
                    tactics.CompleteCT();
                }
            }
        }
        return cTTimelineQueue;
    }
    public void NextCharacter()
    {
        if (currentTurn == null) { return; }
        NextNumber();
        currentCharacter = currentTurn.cTTimelineQueue[currentNumberIndex];
    }
    private void NextNumber()
    {
        if (currentNumberIndex < currentTurn.cTTimelineQueue.Count - 1)
        {
            currentNumberIndex++;
            Debug.Log($"currentNumber: {currentNumberIndex}");
        }
        else
        {
            if (currentTurnIndex < cTTurn.Count - 1)
            {
                currentTurnIndex++;
                currentTurn = cTTurn[currentTurnIndex];
                currentNumberIndex = 0;
                Debug.Log($"currentTurn: {currentTurnIndex}, currentNumber: {currentNumberIndex}");
            }
        }
    }
    private void TargetCharacterUIUpdate()
    {
        uITransitionToolkit.ResetUIFormToTargetPos(1);
    }
    public List<CTTurn> GetAllTurnHistory() => cTTurn;
    public CTTurn GetCurrentTurn() => currentTurn;
    public CharacterBase GetCurrentCharacter() => currentCharacter;
    public Dictionary<CharacterBase, CharacterTacticsTime> GetBattleCharacter() => battleCharacter;
}

