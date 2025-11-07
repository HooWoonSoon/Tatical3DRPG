using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerTeamLinkUIManager : MonoBehaviour
{
    public TeamDeployment teamDeployment;

    [SerializeField] private Canvas canvas;
    public TeamLinkUI[] teamUIClasses;
    private TeamLinkUI currentTeamLinkUI;
    private TeamLinkUI markedTeamLinkUI;

    public MiniUISetTooltip miniUISetTooltip;
    public Vector2[] miniUIPopUpPos = new Vector2[]
    {
        new Vector2(-720, 460),
        new Vector2(-640, 380),
        new Vector2(-720, 300),
        new Vector2(-800, 380)
    };
    public TeamLinkButton teamLinkButton;

    [SerializeField] private LayerMask layerMask;

    private GameObject currentInteractObject;
    private RectTransform objectRectTransform;
    private Vector2 lastMousePosition;

    private bool isDragging = false;
    private bool isTinyUIPopUp = false;

    private int prevPopUpIndex = -1;
    private GameObject prevInteractObject;

    [Header("Team UI Effect")]
    //[SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Vector2 UIAdjustedOffset = new Vector2(5, 0);
    public static PlayerTeamLinkUIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
        for (int i = 0; i < teamUIClasses.Length; i++)
        {
            List<PlayerCharacter> character = teamDeployment.GetAllOfType<PlayerCharacter>();
            teamUIClasses[i].Initialize(character[i], i);
        }
    }

    private void Update()
    {
        //  Summary
        //      Always update the closest UIClass in order to sort and swap the UI
        TeamLinkUI closestUIClass = GetClosestUIClass();
        
        if (Input.GetMouseButtonDown(0))
        {
            //  Summary
            //      Reset all before function
            ResetTeamLinkObject();

            GetTeamLinkUIObject();
            RecordActivePositionInUI();

            ResetTeamLinkClass();
        }
       
        if (Input.GetMouseButton(0))
        {
            DragUI();
            PopInTeamLinkOptionContentDrag();
            PromptTeamUIWhenDrag(closestUIClass);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ExchangeSorts(closestUIClass);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ProcessTeamLinkOption(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ProcessTeamLinkOption(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ProcessTeamLinkOption(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ProcessTeamLinkOption(3);
        }

        //  Summary
        //      Always update the UI tooltip content
        UpdateUILinkTooltip();
    }

    #region UI Update Content
    private void UpdateUILinkTooltip()
    {
        UpdateUILinkTooltipLinkText();
    }

    private void UpdateUILinkTooltipLinkText()
    {
        if (prevPopUpIndex == -1) { return; }

        List<PlayerCharacter> character = teamDeployment.GetAllOfType<PlayerCharacter>();

        for (int i = 0; i < character.Count; i++)
        {
            if (character[i].index == prevPopUpIndex)
            {
                bool isLink = character[i].isLink;
                miniUISetTooltip.ChangeOptionUILinkText(isLink);
            }
        }
    }
    #endregion

    #region Reset Methods
    private void ResetTeamLinkObject()
    {
        //  Summary
        //      Reset all previous UI object and control state
        currentInteractObject = null;
        objectRectTransform = null;
        currentTeamLinkUI = null;
        lastMousePosition = Input.mousePosition;
        isDragging = false;
    }

    private void ResetTeamLinkClass()
    {
        if (currentTeamLinkUI == null)
        {
            currentInteractObject = null;
            objectRectTransform = null;
            return;
        }
    }
    #endregion

    #region UI Gain Object Methods
    private void GetTeamLinkUIObject()
    {
        if (currentInteractObject != null) { return; }

        currentInteractObject = Utils.GetMouseOverUIElement(canvas);
        if (currentInteractObject == null) { return; }

        objectRectTransform = currentInteractObject.GetComponent<RectTransform>();

        foreach (TeamLinkUI teamUI in teamUIClasses)
        {
            if (teamUI.imageObject == currentInteractObject)
            {
                currentTeamLinkUI = teamUI;
                break; 
            }
        }
    }

    private void ProcessTeamLinkOption(int index = -1)
    {
        ResetTeamLinkObject();
        GetTeamLinkUI(index);
        PopOutTeamLinkOptionContent();
        ResetTeamLinkClass();
    }

    private void GetTeamLinkUI(int index)
    {
        currentTeamLinkUI = teamUIClasses[index];
    }

    private TeamLinkUI GetClosestUIClass()
    {
        //  Summary
        //      Get the closest UIClass to the current object, in order to sort and swap the UI
        if (currentTeamLinkUI == null) { return null; }

        TeamLinkUI closestUIClass = null;
        float closestDistance = float.MaxValue;

        foreach (TeamLinkUI teamUI in teamUIClasses)
        {
            if (objectRectTransform != null && teamUI != currentTeamLinkUI)
            {
                float distance = Vector2.Distance(objectRectTransform.anchoredPosition, teamUI.rectPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUIClass = teamUI;
                }
            }
        }
        return closestUIClass;
    }
    #endregion

    #region UI Control State
    private void RecordActivePositionInUI()
    {
        if (currentTeamLinkUI == null) { return; }

        lastMousePosition = Input.mousePosition;
    }
    #endregion

    #region UI Control Methods
    private void DragUI()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition && isDragging == false && objectRectTransform != null) 
        { isDragging = true; }
        if (objectRectTransform == null) { return; }
        //Debug.Log(" currentMousePosition compare to lastMousePosition so is Dragging: " + isDragging);
        Vector2 delta = currentMousePosition - lastMousePosition;
        objectRectTransform.anchoredPosition += new Vector2(delta.x, delta.y);
        lastMousePosition = currentMousePosition;
    }

    private void ExchangeSorts(TeamLinkUI closestUIClass)
    {
        if (closestUIClass == null || isDragging == false) return;

        //Debug.Log($"Closest UIClass Image {closestUIClass.image}");
        bool didSwap = currentTeamLinkUI.Swap(closestUIClass);

        if (didSwap) { ResetTeamLinkObject(); }
    }

    #region Pop Out /In Methods
    private void PopOutTeamLinkOptionContent()
    {
        if (currentTeamLinkUI == null) { return; }

        //  Summary
        //      Check if the tooltip is already active, if not, activate it
        if (!miniUISetTooltip.gameObject.activeSelf) 
        {
            miniUISetTooltip.gameObject.SetActive(true);
        }

        int currentIndex = currentTeamLinkUI.index;
        miniUISetTooltip.PopOut(miniUIPopUpPos[currentIndex]);
        Debug.Log($"currentIndex: {currentIndex} characterID {currentTeamLinkUI.character}");
        teamLinkButton.Initialize(currentTeamLinkUI);
        
        prevPopUpIndex = currentIndex;
        isTinyUIPopUp = true;
    }

    private void PopInTeamLinkOptionContentDrag()
    {
        if (isDragging == true)
        {
            PopInTeamLinkOptionContent();
        }
    }
    public void PopInTeamLinkOptionContent()
    {
        if (prevPopUpIndex != -1 && isTinyUIPopUp == true)
        {
            miniUISetTooltip.PopIn(miniUIPopUpPos[prevPopUpIndex]);
            prevPopUpIndex = -1;

            isTinyUIPopUp = false;
        }
    }
    #endregion

    #endregion

    #region Prompt UI
    //  Summary
    //      Prompt the UI to show the closest UIClass
    private void PromptTeamUIWhenDrag(TeamLinkUI closestUIClass)
    {
        if (isDragging) { MarkUI(closestUIClass); }
    }

    private void MarkUI(TeamLinkUI closestUIClass)
    {
        if (closestUIClass == null) { return; }

        if (markedTeamLinkUI != closestUIClass)
        {
            if (markedTeamLinkUI != null) { UnmarkUI(); }

            markedTeamLinkUI = closestUIClass;
            closestUIClass.AdjustOffsetToPosition(UIAdjustedOffset);
        }
    }

    private void UnmarkUI()
    {
        if (markedTeamLinkUI == null) { return; }
        
        markedTeamLinkUI.ResetPosition();
        markedTeamLinkUI = null;
    }
    #endregion
}
