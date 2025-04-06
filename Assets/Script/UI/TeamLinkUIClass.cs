using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeamLinkUIClass
{
    public UnitCharacter character;

    public Vector2 rectPosition;
    public Image image;
    public GameObject gameObject;
    public int ID { get; set; } // temporarily useless
    public int index {  get; set; }

    public void Initialize(int index)
    {
        image.rectTransform.anchoredPosition = rectPosition;
        ID = character.characterID;
        this.index = index;
        LinkCharacter();
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

    public void Swap(TeamLinkUIClass other)
    {
        SwapPosition(other);
        SwapIndex(other);
    }

    private void SwapPosition(TeamLinkUIClass other)
    {
        Vector2 tempPosition = other.rectPosition;
        other.UpdatePosition(rectPosition);
        UpdatePosition(tempPosition);
    }

    private void SwapIndex(TeamLinkUIClass other)
    {
        int tempIndex = other.index;
        other.index = index;
        index = tempIndex;
    }

    public void ResetPosition()
    {
        if (image != null)
            image.rectTransform.anchoredPosition = rectPosition;
    }

    public void UnlinkCharacter()
    {
        character.isLink = false;
    }

    public void LinkCharacter()
    {
        character.isLink = true;
    }
    public bool CheckForUnitCharacter()
    {
        return character.isLink;
    }
}