using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UITransitionMove : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField] private Vector2 fromPosition;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float transitionDuration = 0.2f;

    private void OnEnable()
    {
        TargetUIShowUp();
    }

    private void TargetUIShowUp()
    {
        StartCoroutine(TransitionMove(fromPosition, targetPosition, transitionDuration));
    }

    private IEnumerator TransitionMove(Vector2 from, Vector2 target, float duration)
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
