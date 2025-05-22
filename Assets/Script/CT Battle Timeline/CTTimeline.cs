using System.Collections.Generic;
using UnityEngine;
public class CTTimeline : MonoBehaviour
{
    private List<UnitBatttleDeployment> unitBattleDeployment = new List<UnitBatttleDeployment>();
    private List<UnitCharacter> cTTimelineQueue = new List<UnitCharacter>();

    public class UnitBatttleDeployment
    {
        public UnitCharacter unitCharacter;
        public int increaseValue;
        public int CTValue = 0;
        public int accumulatedTime = 0;
        public bool queue;

        public UnitBatttleDeployment(UnitCharacter unitCharacter)
        {
            this.unitCharacter = unitCharacter;
            increaseValue = unitCharacter.speed;
            queue = false;
        }

        public void increaseCT()
        {
            CTValue += increaseValue;
        }
     
        public void completeCT()
        {
            accumulatedTime++;
            increaseValue /= (accumulatedTime + 1);
            CTValue -= 100;
        }

        public void resetCT()
        {
            CTValue = 0;
            increaseValue = 0;
            accumulatedTime = 0;
        }
    }

    public List<UnitBatttleDeployment> timelineDataList = new List<UnitBatttleDeployment>();
    public static CTTimeline instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private bool CheckAllCharacterQueue()
    {
        for (int i = 0; i < unitBattleDeployment.Count; i++)
        {
            if (unitBattleDeployment[i].queue == false)
            {
                return false;
            }
        }
        return true;
    }
    private void SetupTimeline()
    {
        int run = 0;
        while (!CheckAllCharacterQueue())
        {
            if (unitBattleDeployment.Count == 0) { return; }

            for (int i = 0; i < unitBattleDeployment.Count; i++)
            {
                unitBattleDeployment[i].increaseCT();
                if (unitBattleDeployment[i].CTValue >= 100)
                {
                    unitBattleDeployment[i].queue = true;
                    cTTimelineQueue.Add(unitBattleDeployment[i].unitCharacter);
                    unitBattleDeployment[i].completeCT();
                }
                run++;
            }
        }
        Debug.Log("run time: " + run);
        Debug.Log("CTTimelineQueue Count: " + cTTimelineQueue.Count);
    }

    public void ReceiveBattleJoinedUnit(List<UnitCharacter> unitCharacters)
    {
        for (int i = 0; i < unitCharacters.Count; i++)
        {
            unitBattleDeployment.Add(new UnitBatttleDeployment(unitCharacters[i]));
        }

        SetupTimeline();
    }
}

