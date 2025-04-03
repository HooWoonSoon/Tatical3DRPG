using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeamLinkTooltipClass
{
    public Button button;
    public TextMeshProUGUI textUI;
    public string optionName;

    public RectTransform rectTransform { get; private set; }
    public Vector2 initializeOffset { get; private set; }
    public Vector2 initializeScale { get; private set; }
    public Vector2 targetOffset { get; private set; }
    public Vector2 targetScale { get; private set; }

    public void Initialize()
    {
        if (rectTransform == null)
        {
            rectTransform = button.GetComponent<RectTransform>();
        }

        initializeOffset = rectTransform.anchoredPosition - new Vector2(rectTransform.rect.width / 2, 0);
        initializeScale = Vector2.zero;
        targetOffset = rectTransform.anchoredPosition;
        targetScale = rectTransform.localScale;
    }

    public void UnEnable()
    {
        rectTransform.anchoredPosition = initializeOffset;
        rectTransform.localScale = initializeScale;
    }
}