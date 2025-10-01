using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BattleUIController : MonoBehaviour
{
    public Canvas canvas;
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
