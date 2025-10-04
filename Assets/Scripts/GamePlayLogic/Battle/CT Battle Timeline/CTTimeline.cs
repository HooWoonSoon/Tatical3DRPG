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
    private List<CTTurn> cTTurn = new List<CTTurn>();
    private const int MAX_TURNS = 3;

    private CTTurn currentCTTurn;
    private int currentTurn = 0;
    private int currentTurnIndex = 0;
    private CharacterBase currentCharacter;

    [SerializeField] private UITransitionToolkit uITransitionToolkit;
    public event Action confirmCTTimeline;
    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
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
        currentCTTurn = cTTurn[0];
        currentCharacter = currentCTTurn.cTTimelineQueue[0];
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
        if (currentCTTurn == null) { return; }
        Debug.Log($"{currentCharacter} end this turn");
        NextNumber();
        currentCharacter = currentCTTurn.cTTimelineQueue[currentTurnIndex];
        CTTurnUIManager.instance.TargetCurrentCTTurnUI(currentCTTurn, currentTurnIndex);
    }
    private void NextNumber()
    {
        if (currentTurnIndex < currentCTTurn.cTTimelineQueue.Count - 1)
        {
            currentTurnIndex++;
            //Debug.Log($"currentNumber: {currentTurnIndex}");
        }
        else
        {
            if (currentTurn < cTTurn.Count - 1)
            {
                currentTurn++;
                currentCTTurn = cTTurn[currentTurn];
                currentTurnIndex = 0;
                //Debug.Log($"currentTurn: {currentTurnIndex}, currentNumber: {currentNumberIndex}");
            }
        }
    }
    private void TargetCharacterUIUpdate()
    {
        uITransitionToolkit.ResetUIFormToTargetPos(1);
    }
    public List<CTTurn> GetAllTurnHistory() => cTTurn;
    public CTTurn GetCurrentCTTurn() => currentCTTurn;
    public int GetCurrentTurnIndex() => currentTurnIndex;
    public int GetCurrentTurn() => currentTurn;
    public CharacterBase GetCurrentCharacter() => currentCharacter;
    public Dictionary<CharacterBase, CharacterTacticsTime> GetBattleCharacter() => battleCharacter;
}

