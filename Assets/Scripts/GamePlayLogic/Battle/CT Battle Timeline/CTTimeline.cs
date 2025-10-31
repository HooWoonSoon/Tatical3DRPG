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

    private List<CTRound> cTRounds = new List<CTRound>();
    private const int INITIAL_ROUND = 4;

    private CTRound currentCTRound;
    private int currentRoundIndex = 0;
    private int currentTurnIndex = 0;
    private CharacterBase currentCharacter;

    [SerializeField] private UITransitionToolkit uITransitionToolkit;

    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BattleManager.instance.onLoadNextTurn += NextCharacterTurn;
    }

    public void SetJoinedBattleUnit(List<CharacterBase> characters)
    {
        battleCharacter.Clear();
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
        cTRounds.Clear();

        for (int i = 0; i < INITIAL_ROUND; i++)
        {
            List<CharacterBase> completeQueue = GetCalculateCTQueue();
            CTRound turnHistory = new CTRound(completeQueue, i);
            cTRounds.Add(turnHistory);

            //  Reset all battle character last turn accumulated value
            foreach (var tactics in battleCharacter.Values)
            {
                tactics.Reset();
            }
        }
        currentCTRound = cTRounds[0];
        currentRoundIndex = 0;
        currentTurnIndex = 0;
        currentCharacter = currentCTRound.cTTimelineQueue[0];
        CTTurnUIManager.instance.GenerateTimelineUI();;
    }

    private void ExtentTimeline()
    {
        List<CharacterBase> completeQueue = GetCalculateCTQueue();
        CTRound turnHistory = new CTRound(completeQueue, cTRounds.Count);
        cTRounds.Add(turnHistory);

        foreach (var tactics in battleCharacter.Values)
        {
            tactics.Reset();
        }
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
    public void NextCharacterTurn()
    {
        if (currentCTRound == null) { return; }
        Debug.Log($"{currentCharacter} end this turn");
        NextNumber();
        currentCharacter = currentCTRound.cTTimelineQueue[currentTurnIndex];
        CTTurnUIManager.instance.TargetCurrentCTTurnUI(currentCTRound, currentTurnIndex);
    }
    private void NextNumber()
    {
        CTTurnUIManager.instance.RecordPastTurnUI(currentCTRound, currentTurnIndex);

        if (currentTurnIndex < currentCTRound.cTTimelineQueue.Count - 1)
        {
            currentTurnIndex++;
            //Debug.Log($"currentNumber: {currentTurnIndex}");
        }
        else
        {
            if (currentRoundIndex < cTRounds.Count - 1)
            {
                currentRoundIndex++;
                currentCTRound = cTRounds[currentRoundIndex];
                currentTurnIndex = 0;
                ExtentTimeline();
                //Debug.Log($"currentTurn: {currentTurnIndex}, currentNumber: {currentNumberIndex}");
            }
        }
        CTTurnUIManager.instance.AppendTurnUI();
    }
    private void TargetCharacterUIUpdate()
    {
        uITransitionToolkit.ResetUIFormToTargetPos(1);
    }
    public int GetCharacterCurrentQueue(CharacterBase character)
    {
        if (cTRounds.Count == 0) 
        {
            Debug.LogWarning("No Round implemented");
            return -1; 
        }

        int index = 0;
        for (int r = currentRoundIndex; r < cTRounds.Count; r++)
        {
            CTRound searchRound = cTRounds[r];
            List<CharacterBase> allCharacterQueue = searchRound.cTTimelineQueue;

            int startTurn = (r == currentRoundIndex) ? currentTurnIndex : 0;

            for (int t = startTurn; t < allCharacterQueue.Count; t++)
            {
                if (character == allCharacterQueue[t])
                    return index;
                
                index++;
            }
        }
        return -1;
    }
    public List<CTRound> GetAllTurnHistory() => cTRounds;
    public CTRound GetCurrentCTTurn() => currentCTRound;
    public int GetCurrentTurn() => currentTurnIndex;
    public int GetCurrentRound() => currentRoundIndex;
    public CharacterBase GetCurrentCharacter() => currentCharacter;
    public Dictionary<CharacterBase, CharacterTacticsTime> GetBattleCharacter() => battleCharacter;
}

