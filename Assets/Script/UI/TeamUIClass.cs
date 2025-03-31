using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeamUIClass
{
    public Vector2 rectPosition;
    public Image image;
    public GameObject gameObject;

    public void Initialize()
    {
        image.rectTransform.anchoredPosition = rectPosition;
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        rectPosition = newPosition;
        image.rectTransform.anchoredPosition = rectPosition;
    }

    public void AdjustOffsetToPosition(Vector2 newPosition)
    {
        image.rectTransform.anchoredPosition += newPosition;
    }

    public void SwapPosition(TeamUIClass other)
    {
        Vector2 tempPosition = other.rectPosition;
        other.UpdatePosition(rectPosition);
        UpdatePosition(tempPosition);
    }

    public void ResetPosition()
    {
        image.rectTransform.anchoredPosition = rectPosition;
    }
}