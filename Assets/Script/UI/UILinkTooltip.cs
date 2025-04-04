using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class UILinkTooltip : MonoBehaviour
{
    public TeamLinkTooltipClass[] options;
    [SerializeField] private float spacing = 3f;
     
    private RectTransform rectTransform;

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

        rectTransform = GetComponent<RectTransform>();
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

    public void PopOut(Vector2 popUpPos, UnityEvent onComplete = null)
    {
        rectTransform.anchoredPosition = popUpPos;

        for (int i = 0; i < options.Length; i++)
        {
            UIPopupEffectScaleAndOffset.ApplyAnimation(this, options[i], options[i].offsetPosLeft, 
                options[i].initialPos, new Vector2(0, 0), options[i].initialScale, 0.3f, true, onComplete);
        }
    }

    public void PopIn(Vector2 popInPos, UnityEvent onComplete = null)
    {
        rectTransform.anchoredPosition = popInPos;

        for (int i = 0; i < options.Length; i++)
        {
            UIPopupEffectScaleAndOffset.ApplyAnimation(this, options[i], options[i].initialPos,
                options[i].offsetPosLeft, options[i].initialScale, new Vector2(0, 0), 0.1f, true, onComplete);
        }
    }
}