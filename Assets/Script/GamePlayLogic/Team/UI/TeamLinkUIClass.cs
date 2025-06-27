using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeamLinkUIClass
{
    public CharacterBase character { get; private set; }
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
    public GameObject imageObject;
    private Image image;
    #endregion

    #region BroadCasting
    private void BroadCastingLeaderChanged()
    {
        TeamEvent.OnLeaderChanged?.Invoke();
    }

    private void BroadCastingTeamSortExchange()
    {
        TeamEvent.OnTeamSortExchange?.Invoke();
    }
    #endregion

    #region TeamLink UI Management
    public void Initialize(CharacterBase character, int index)
    {
        this.character = character;
        imageObject = character.imageObject;
        image = imageObject.GetComponent<Image>();
        image.rectTransform.anchoredPosition = rectPosition;

        ID = character.data.ID;
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

    public bool Swap(TeamLinkUIClass other)
    {
        if (other == null) return false;

        bool changed = false;

        if (rectPosition != other.rectPosition)
        {
            SwapPosition(other);
            changed = true;
        }

        if (Index != other.Index)
        {
            SwapIndex(other);
            changed = true;
        }

        if (changed) 
        { 
            BroadCastingLeaderChanged();
            BroadCastingTeamSortExchange();
        }

        return changed;
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
    #endregion
}