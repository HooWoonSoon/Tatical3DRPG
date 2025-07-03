using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UITransitionToolkit : MonoBehaviour
{
    public UITransitionMove[] uITransitionMove;

    [Serializable]
    public class UITransitionMove
    {
        public GameObject moveObject;
        public Vector2 fromPosition;
        public Vector2 targetPosition;
        public float transitionDuration = 0.2f;
    }

    private void OnEnable()
    {
        TargetUIShowUp();
    }

    private void TargetUIShowUp()
    {
        for (int i = 0; i < uITransitionMove.Length; i++)
        {
            RectTransform rectTransform = uITransitionMove[i].moveObject.GetComponent<RectTransform>();
            StartCoroutine(TransitionMove(rectTransform, uITransitionMove[i].fromPosition, uITransitionMove[i].targetPosition, uITransitionMove[i].transitionDuration));
        }
    }

    private IEnumerator TransitionMove(RectTransform rectTransform, Vector2 from, Vector2 target, float duration)
    {
        if (rectTransform == null) { rectTransform = GetComponent<RectTransform>(); }

        float elapsedTime = 0f;
        rectTransform.anchoredPosition = from; 

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(from, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }
}
