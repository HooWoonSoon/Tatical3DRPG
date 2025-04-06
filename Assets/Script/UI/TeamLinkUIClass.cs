using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeamLinkUIClass
{
    private UnitCharacter character;
    public int ID { get; private set; }
    public int index { get; private set; }
    public int Index
    {
        get => character != null ? character.index : index;
        set
        {
            if (character != null)
            {
                character.index = value;
            }
            index = value;
        }
    }

    #region Image
    public Vector2 rectPosition;
    public GameObject imageObject { get; private set; }
    private Image image;
    #endregion

    public void Initialize(UnitCharacter character, int index)
    {
        this.character = character;
        imageObject = character.imageObject;
        image = imageObject.GetComponent<Image>();
        image.rectTransform.anchoredPosition = rectPosition;

        ID = character.ID;
        Index = index;

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
        int tempIndex = other.Index;
        other.Index = Index;
        Index = tempIndex;
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