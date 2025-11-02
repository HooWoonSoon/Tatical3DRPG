using UnityEngine;
using TMPro;
using System.Collections;

public class UniversalUIManager : MonoBehaviour
{
    public GameObject universalPanel;
    public static UniversalUIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void CreateCountText(CharacterBase character, int value)
    {
        string damageText = value.ToString();
        TextMeshProUGUI damangeTextUI = Utils.CreateCanvasText(damageText, universalPanel.transform, character.transform.position + new Vector3(0, 1, 0), 35, Color.white, TextAlignmentOptions.Center);
        StartCoroutine(UIFadeCoroutine(damangeTextUI, 0f, 1f, 0.2f, false));
        StartCoroutine(UIFadeCoroutine(damangeTextUI, 1f, 0f, 1.5f, true));
        Debug.Log("Generated Text");
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

