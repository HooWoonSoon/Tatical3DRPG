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
    private int currentRound = 0;
    private int currentTurn = 0;
    private CharacterBase currentCharacter;

    [SerializeField] private UITransitionToolkit uITransitionToolkit;
    public event Action confirmCTTimeline;

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
        currentRound = 0;
        currentTurn = 0;
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
        currentCharacter = currentCTRound.cTTimelineQueue[currentTurn];
        CTTurnUIManager.instance.TargetCurrentCTTurnUI(currentCTRound, currentTurn);
    }
    private void NextNumber()
    {
        CTTurnUIManager.instance.RecourdPastTurnUI(currentCTRound, currentTurn);

        if (currentTurn < currentCTRound.cTTimelineQueue.Count - 1)
        {
            currentTurn++;
            //Debug.Log($"currentNumber: {currentTurnIndex}");
        }
        else
        {
            if (currentRound < cTRounds.Count - 1)
            {
                currentRound++;
                currentCTRound = cTRounds[currentRound];
                currentTurn = 0;
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
    public List<CTRound> GetAllTurnHistory() => cTRounds;
    public CTRound GetCurrentCTTurn() => currentCTRound;
    public int GetCurrentTurn() => currentTurn;
    public int GetCurrentRound() => currentRound;
    public CharacterBase GetCurrentCharacter() => currentCharacter;
    public Dictionary<CharacterBase, CharacterTacticsTime> GetBattleCharacter() => battleCharacter;
}

