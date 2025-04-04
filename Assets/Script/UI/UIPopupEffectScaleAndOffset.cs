using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class UIPopupEffectScaleAndOffset
{
    public static void ApplyAnimation(MonoBehaviour mono, TeamLinkTooltipClass UILinkTooltipClass, 
        Vector2 startPosition, Vector2 endPosition, Vector2 startScale, Vector2 endScale, float duration, 
        bool useElastic, UnityEvent onComplete)
    {
        mono.StartCoroutine(Animate(UILinkTooltipClass, startPosition, endPosition, 
            startScale, endScale, duration, useElastic, onComplete));
    }

    private static IEnumerator Animate(TeamLinkTooltipClass UILinkTooltipClass, 
        Vector2 startPosition, Vector2 endPosition, Vector2 startScale, Vector2 endScale, float duration, 
        bool useElastic, UnityEvent onComplete)
    {
        float elapsedTime = 0f;

        Vector2 extraOffset = Vector2.zero;
        float extraScale = 1f;

        if (useElastic)
        {
            extraOffset = new Vector2(5f, 0f);
            extraScale = 1.1f;
        }

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float adjustedT;

            if (useElastic)
                adjustedT = t * t * (3f - 2f * t); 
            else
                adjustedT = t * t;

            UILinkTooltipClass.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition + extraOffset, adjustedT);
            UILinkTooltipClass.rectTransform.localScale = Vector2.Lerp(startScale, endScale * extraScale, adjustedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //  Summary
        //      Ensure the final position and scale is exactly the target
        UILinkTooltipClass.rectTransform.anchoredPosition = endPosition;
        UILinkTooltipClass.rectTransform.localScale = endScale;

        onComplete?.Invoke();
    }
}