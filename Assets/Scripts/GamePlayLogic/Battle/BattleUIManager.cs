using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public GameObject battleStatePanel;

    [Header("Battle Event Display UI")]
    public GameObject battleEventDisplayUI;
    public float duration = 2.0f;

    [Header("Battle Set Skill UI")]
    public GameObject actionOptionPanel;
    public GameObject skillUI;
    public GameObject cTTimelineUI;

    public event Action OnBattleUIFinish;
    public static BattleUIManager instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        battleStatePanel.SetActive(false);
        battleEventDisplayUI.SetActive(false);
        actionOptionPanel.SetActive(false);
        skillUI.SetActive(false);
        cTTimelineUI.SetActive(false);
    }

    public void PrepareBattleUI()
    {
        battleStatePanel.SetActive(true);
        battleEventDisplayUI.SetActive(true);
        cTTimelineUI.SetActive(true);
        StartCoroutine(UIFinish());
    }

    public IEnumerator UIFinish()
    {
        yield return new WaitForSeconds(duration);
        OnBattleUIFinish?.Invoke();
        battleEventDisplayUI.SetActive(false);
    }
    public void OpenUpdateSkillUI(CharacterBase character)
    {
        if (skillUI.activeSelf == true) { return; } 
        skillUI.SetActive(true);
        List<SkillData> characterSkillList = character.skillData;
        TeamDeployment teamDeployment = character.currentTeam;
        List<InventoryData> invetoryList = teamDeployment.inventoryDatas;
        if (characterSkillList != null)
        {
            SkillUIManager.instance.Initialize(characterSkillList, invetoryList, character);
        }
    }
    public void CloseSkillUI()
    {
        SkillUIManager.instance.ResetAll();
        skillUI.SetActive(false);
    }

    public void ActivateActionPanel(bool active)
    {
        actionOptionPanel.SetActive(active);
    }

    public void ActiveAllCharacterInfoTip(bool active)
    {
        foreach (var character in BattleManager.instance.GetBattleUnits())
        {
            SelfCanvasController selfCanvasController = character.selfCanvasController;
            if (selfCanvasController == null)
                Debug.LogWarning($"{character} missing Self Canvas Controller");
            else
            {
                int queue = CTTimeline.instance.GetCharacterCurrentQueue(character);
                selfCanvasController.SetQueue(queue);
                float healthPercentage = character.GetCurrentHealthPercentage();
                selfCanvasController.SetHeathPercetange(healthPercentage);

                Canvas selfCanvas = selfCanvasController.selfCanvas;
                if (selfCanvas == null)
                    Debug.LogWarning($"{character} missing Self Canvas");
                else
                {
                    selfCanvas.gameObject.SetActive(active);
                }
            }
                
        }
    }
}
