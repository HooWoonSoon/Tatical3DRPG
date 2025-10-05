using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public Canvas canvas;
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
        if (characterSkillList != null)
        {
            SkillUIManager.instance.Initialize(characterSkillList, character);
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

    public void CreateCountText(CharacterBase character, int value)
    {
        string damageText = value.ToString();
        TextMeshProUGUI damangeTextUI = Utils.CreateCanvasText(damageText, canvas.transform, character.transform.position, Quaternion.identity, 25, Color.white, TextAlignmentOptions.Center);
        StartCoroutine(UIFadeCoroutine(damangeTextUI, 0f, 1f, 0.2f, false));
        StartCoroutine(UIFadeCoroutine(damangeTextUI, 1f, 0f, 1.5f, true));
    }

    private IEnumerator UIFadeCoroutine(TextMeshProUGUI textUI, float startAlpha, float endAlpha, float duration, bool destroyOnComplete = false)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            textUI.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        textUI.alpha = endAlpha;

        if (destroyOnComplete)
        {
            Destroy(textUI.gameObject);
        }
    }
}
