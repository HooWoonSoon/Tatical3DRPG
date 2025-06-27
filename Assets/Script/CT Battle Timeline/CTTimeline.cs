using System;
using System.Collections.Generic;
using UnityEngine;

public class CTTimeline : MonoBehaviour
{
    [Serializable]
    public class CTTurnHistory
    {
        public List<PlayerCharacter> cTTimelineQueue = new List<PlayerCharacter>();
        public int turnCount = 0;

        public CTTurnHistory(List<PlayerCharacter> cTTimelineQueue, int turnCount)
        {
            this.cTTimelineQueue = cTTimelineQueue;
            this.turnCount = turnCount;
        }
    }

    public class UnitCTTimelineClass
    {
        public PlayerCharacter unitCharacter;
        public int increaseValue;
        public int CTValue = 0;
        public int accumulatedTime = 0;
        public bool queue;

        private const int BASE_INCREASE = 2;

        public UnitCTTimelineClass(PlayerCharacter character)
        {
            this.unitCharacter = character;
            increaseValue = character.data.speed + BASE_INCREASE;
            queue = false;
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
            queue = true;
        }

        public void Reset()
        {
            CTValue = 0;
            increaseValue = unitCharacter.data.speed + BASE_INCREASE;
            accumulatedTime = 0;
            queue = false;
        }
    }

    private List<UnitCTTimelineClass> unitBattleDeployment = new List<UnitCTTimelineClass>();
    public List<CTTurnHistory> cTTurnhistory = new List<CTTurnHistory>();
    private const int MAX_TURNS = 3;

    public event Action confirmCTTimeline;
    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private bool CheckAllCharacterQueue()
    {
        for (int i = 0; i < unitBattleDeployment.Count; i++)
        {
            if (!unitBattleDeployment[i].queue)
            {
                return false;
            }
        }
        return true;
    }

    private void SetupTimeline()
    {
        if (unitBattleDeployment.Count == 0) { return; }

        for (int i = 0; i < MAX_TURNS; i++)
        {
            CTTurnHistory turnHistory = new CTTurnHistory(GetCalculateCTQueue(), i);
            cTTurnhistory.Add(turnHistory);
            RestCTQueue();
        }
        confirmCTTimeline?.Invoke();
    }

    private void RestCTQueue()
    {
        for (int i = 0; i < unitBattleDeployment.Count; i++)
        {
            unitBattleDeployment[i].Reset();
        }
    }

    private List<PlayerCharacter> GetCalculateCTQueue()
    {
        List<PlayerCharacter> cTTimelineQueue = new List<PlayerCharacter>();

        while (!CheckAllCharacterQueue())
        {
            for (int i = 0; i < unitBattleDeployment.Count; i++)
            {
                unitBattleDeployment[i].IncreaseCT();
                if (unitBattleDeployment[i].CTValue >= 100)
                {
                    cTTimelineQueue.Add(unitBattleDeployment[i].unitCharacter);
                    unitBattleDeployment[i].CompleteCT();
                }
            }
        }
        return cTTimelineQueue;
    }
    public void ReceiveBattleJoinedUnit(List<PlayerCharacter> unitCharacters)
    {
        for (int i = 0; i < unitCharacters.Count; i++)
        {
            unitCharacters[i].isBattle = true;
            unitBattleDeployment.Add(new UnitCTTimelineClass(unitCharacters[i]));
        }
        SetupTimeline();
    }

    public List<CTTurnHistory> GetAllTurnHistory() => cTTurnhistory;
}

