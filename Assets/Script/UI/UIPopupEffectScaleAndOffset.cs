using System.Collections;
using UnityEngine;

public static class UIPopupEffectScaleAndOffset
{
    public static void ApplyAnimation(MonoBehaviour mono, UILinkTooltip UILinkTooltip, float duration, bool useElastic)
    {
        mono.StartCoroutine(Animate(UILinkTooltip, duration, useElastic));
    }

    private static IEnumerator Animate(UILinkTooltip UILinkTooltip, float duration, bool useElastic)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            float adjustedT;
            if (useElastic)
            {
                adjustedT = t * t * (3f - 2f * t); 
            }
            else
            {
                adjustedT = t * t; 
            }

            foreach (var option in UILinkTooltip.options)
            {
                Vector2 extraOffset = Vector2.zero;
                float extraScale = 1f;

                if (useElastic)
                {
                    extraOffset = new Vector2(5f, 0f);
                    extraScale = 1.1f; 
                }

                option.rectTransform.anchoredPosition = Vector2.Lerp(option.initializeOffset, option.targetOffset + extraOffset, adjustedT);
                option.rectTransform.localScale = Vector2.Lerp(option.initializeScale, option.targetScale * extraScale, adjustedT);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //  Summary
        //      Ensure the final position and scale is exactly the target
        foreach (var option in UILinkTooltip.options)
        {
            option.rectTransform.anchoredPosition = option.targetOffset;
            option.rectTransform.localScale = option.targetScale;
        }
    }
}