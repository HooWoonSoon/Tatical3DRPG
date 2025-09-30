using System;
using System.Collections;
using UnityEngine;

public class BattleUIController : MonoBehaviour
{
    public GameObject battleStatePanel;

    [Header("Battle Event Display UI")]
    public GameObject battleEventDisplayUI;
    public float duration = 2.0f;

    [Header("Battle Set UI")]
    public GameObject skillUI;
    public GameObject cTTimelineUI;

    public event Action OnBattleUIFinish;
    public static BattleUIController instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        battleStatePanel.SetActive(false);
        battleEventDisplayUI.SetActive(false);
        cTTimelineUI.SetActive(false);
        skillUI.SetActive(false);
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
        Debug.Log("UIFinish");
        battleEventDisplayUI.SetActive(false);
    }

    public void UpdateSkillUI(CharacterBase characterBase)
    {

    }

    public void OpenSkillUI()
    {
        skillUI.SetActive(true);
    }

    public void OpenUpdateSkillUI(CharacterBase characterBase)
    {
        UpdateSkillUI(characterBase);
    }

    public void CloseSkillUI()
    {
        skillUI.SetActive(false);
    }
}
