using System;
using System.Collections.Generic;
using UnityEngine;

public class CTTimeline : MonoBehaviour
{
    [Serializable]
    public class CTTurnHistory
    {
        public List<CharacterBase> cTTimelineQueue = new List<CharacterBase>();
        public int turnCount = 0;

        public CTTurnHistory(List<CharacterBase> cTTimelineQueue, int turnCount)
        {
            this.cTTimelineQueue = cTTimelineQueue;
            this.turnCount = turnCount;
        }
    }

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
    public List<CTTurnHistory> cTTurnhistory = new List<CTTurnHistory>();
    private const int MAX_TURNS = 3;

    private CTTurnHistory currentTurnHistory;
    private int currentTurnIndex = 0;
    private int currentNumberIndex = 0;

    public event Action confirmCTTimeline;
    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    public void InsertCharacter(CharacterBase character)
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
        cTTurnhistory.Clear();

        for (int i = 0; i < MAX_TURNS; i++)
        {
            List<CharacterBase> completeQueue = GetCalculateCTQueue();
            CTTurnHistory turnHistory = new CTTurnHistory(completeQueue, i);
            cTTurnhistory.Add(turnHistory);

            //  Reset all battle character last turn accumulated value
            foreach (var tactics in battleCharacter.Values)
            {
                tactics.Reset();
            }
        }
        currentTurnHistory = cTTurnhistory[0];
        //confirmCTTimeline?.Invoke();
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
    public void NextNumber()
    {
        if (currentNumberIndex < currentTurnHistory.cTTimelineQueue.Count - 1)
        {
            currentNumberIndex++;
            Debug.Log($"currentNumber: {currentNumberIndex}");
        }
        else
        {
            if (currentTurnIndex < cTTurnhistory.Count - 1)
            {
                currentTurnIndex++;
                currentTurnHistory = cTTurnhistory[currentTurnIndex];
                currentNumberIndex = 0;
                Debug.Log($"currentTurn: {currentTurnIndex}, currentNumber: {currentNumberIndex}");
            }
        }
    }
    public List<CTTurnHistory> GetAllTurnHistory() => cTTurnhistory;
    public CTTurnHistory GetCurrentTurn() => currentTurnHistory;
    public CharacterBase GetCurrentCharacter()
    {
        return currentTurnHistory.cTTimelineQueue[currentNumberIndex];
    }
}

