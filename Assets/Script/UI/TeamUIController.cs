using UnityEngine;
using UnityEngine.Events;

public class TeamUIController : MonoBehaviour
{
    public TeamDeployment teamDeployment;

    [SerializeField] private Canvas canvas;
    public TeamLinkUIClass[] teamUIClasses;
    private TeamLinkUIClass currentTeamUIClass;
    private TeamLinkUIClass markedUIClass;

    public UILinkTooltip uILinkTooltip;
    public Vector2[] popUpPositions = new Vector2[]
    {
        new Vector2(-800, 460),
        new Vector2(-720, 380),
        new Vector2(-800, 300),
        new Vector2(-720, 220)
    };
    public TeamLinkButton teamLinkButton;

    [SerializeField] private LayerMask layerMask;

    private GameObject currentInteractObject;
    private RectTransform objectRectTransform;
    private Vector2 lastMousePosition;

    private bool isDragging = false;
    private bool isTinyUIPopOut = false;

    private int prevPopUpIndex = -1;
    private GameObject prevInteractObject;

    [Header("Team UI Effect")]
    //[SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Vector2 UIAdjustedOffset = new Vector2(5, 0);

    private void Start()
    {
        for (int i = 0; i < teamUIClasses.Length; i++)
        {
            teamUIClasses[i].Initialize(teamDeployment.teamCharacter[i], i);
        }
        uILinkTooltip.gameObject.SetActive(false);
    }

    private void Update()
    {
        //  Summary
        //      Always update the closest UIClass in order to sort and swap the UI
        TeamLinkUIClass closestUIClass = GetClosestUIClass();
        
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
            PromptTeamUIWhenDrag(closestUIClass, isDragging);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ExchangeSorts(closestUIClass);
        }

        if (Input.GetMouseButtonUp(1))
        {
            //  Summary
            //      Reset all before function
            ResetTeamLinkObject();

            GetTeamLinkUIObject();
            RecordCheckInUI();
            PopOutTeamLinkOptionContent();

            ResetTeamLinkClass();
        }

        if (Input.GetMouseButtonUp(1) && currentTeamUIClass == null)
        {
            PopInTeamLinkOptionContent();
        }
    }

    #region Reset Methods
    private void ResetTeamLinkObject()
    {
        //  Summary
        //      Reset all previous UI object and control state
        currentInteractObject = null;
        objectRectTransform = null;
        currentTeamUIClass = null;
        lastMousePosition = Input.mousePosition;
        isDragging = false;
    }

    private void ResetTeamLinkClass()
    {
        if (currentTeamUIClass == null)
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
        if (currentInteractObject == null)
        {
            currentInteractObject = Utils.GetMouseOverUIElement(canvas);
            if (currentInteractObject == null) { return; }

            objectRectTransform = currentInteractObject.GetComponent<RectTransform>();

            foreach (TeamLinkUIClass teamUI in teamUIClasses)
            {
                if (teamUI.imageObject == currentInteractObject)
                {
                    currentTeamUIClass = teamUI;
                    break; 
                }
            }
        }
    }

    private TeamLinkUIClass GetClosestUIClass()
    {
        //  Summary
        //      Get the closest UIClass to the current object, in order to sort and swap the UI
        if (currentTeamUIClass == null) { return null; }

        TeamLinkUIClass closestUIClass = null;
        float closestDistance = float.MaxValue;

        foreach (TeamLinkUIClass teamUI in teamUIClasses)
        {
            if (objectRectTransform != null && teamUI != currentTeamUIClass)
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
    private void RecordCheckInUI()
    {
        if (currentTeamUIClass == null) { return; }
        //Debug.Log($"currentTeamUIClass {currentTeamUIClass.index}");
    }

    private void RecordActivePositionInUI()
    {
        if (currentTeamUIClass == null) { return; }

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

    private void ExchangeSorts(TeamLinkUIClass closestUIClass)
    {
        if (closestUIClass == null) return;

        //Debug.Log($"Closest UIClass Image {closestUIClass.image}");
        currentTeamUIClass.Swap(closestUIClass);

        ResetTeamLinkObject();
    }

    #region Pop Out /In Methods
    private void PopOutTeamLinkOptionContent()
    {
        if (currentTeamUIClass == null) { return; }

        //  Summary
        //      Check if the tooltip is already active, if not, activate it
        if (!uILinkTooltip.gameObject.activeSelf) 
        {
            uILinkTooltip.gameObject.SetActive(true);
        }

        int currentIndex = currentTeamUIClass.index;
        uILinkTooltip.PopOut(popUpPositions[currentIndex]);
        Debug.Log($"currentIndex: {currentIndex} characterID {currentTeamUIClass.ID}");
        teamLinkButton.Initialize(currentIndex);
        
        prevPopUpIndex = currentIndex;
        isTinyUIPopOut = true;
    }

    private void PopInTeamLinkOptionContentDrag()
    {
        if (isDragging == true)
        {
            PopInTeamLinkOptionContent();
        }
    }

    private void PopInTeamLinkOptionContent()
    {
        if (prevPopUpIndex != -1 && isTinyUIPopOut == true)
        {
            UnityEvent onComplete = new UnityEvent();
            onComplete.AddListener(() => uILinkTooltip.gameObject.SetActive(false));
            uILinkTooltip.PopIn(popUpPositions[prevPopUpIndex], onComplete);
            prevPopUpIndex = -1;
            isTinyUIPopOut = false;
        }
    }
    #endregion

    #endregion

    #region Prompt UI
    //  Summary
    //      Prompt the UI to show the closest UIClass
    private void PromptTeamUIWhenDrag(TeamLinkUIClass closestUIClass, bool isDragging)
    {
        if (isDragging) { MarkUI(closestUIClass); }
    }

    private void MarkUI(TeamLinkUIClass closestUIClass)
    {
        if (closestUIClass == null) { return; }

        if (markedUIClass != closestUIClass)
        {
            if (markedUIClass != null) { UnmarkUI(); }

            markedUIClass = closestUIClass;
            closestUIClass.AdjustOffsetToPosition(UIAdjustedOffset);
        }
    }

    private void UnmarkUI()
    {
        if (markedUIClass == null) { return; }
        
        markedUIClass.ResetPosition();
        markedUIClass = null;
    }
    #endregion
}
