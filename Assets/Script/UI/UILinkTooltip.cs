using TMPro;
using UnityEngine;

public class UILinkTooltip : MonoBehaviour
{
    public TeamLinkTooltipClass[] options;
    [SerializeField] private float spacing = 3f;

    private void OnValidate()
    {
        if (options == null || options.Length == 0) return;
        for (int i = 0; i < options.Length; i++)
        {
            options[i].Initialize();
        }
        SetUI();
    }

    private void Start()
    {
        if (options == null || options.Length == 0) return;
        for (int i = 0; i < options.Length; i++)
        {
            options[i].Initialize();
            options[i].UnEnable();
        }
    }

    private void SetUI()
    {
        //  Summary
        //      Calculate every button height
        float totalHeight = 0;

        for (int i = 0; i < options.Length; i++)
        {
            totalHeight += options[i].rectTransform.rect.height;
            options[i].button.gameObject.name = "UI Team Tooltip" + "-" + options[i].optionName;
        }
        totalHeight += spacing * (options.Length - 1);

        float startY = totalHeight * 0.5f;

        //  Summary
        //      For loop every single option and update UI position and text
        for (int i = 0; i < options.Length; i++)
        {
            float buttonHeight = options[i].rectTransform.rect.height;
            float currentY = startY - (buttonHeight * 0.5f);

            options[i].rectTransform.anchoredPosition = new Vector2(options[i].rectTransform.anchoredPosition.x, currentY);
            options[i].textUI.text = options[i].optionName;

            startY -= (buttonHeight + spacing);
        }
    }
}