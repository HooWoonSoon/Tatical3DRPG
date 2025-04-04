using UnityEngine;
using UnityEngine.Events;

enum HandleType
{
    Drag, Check
}

public class TeamUIController : MonoBehaviour
{
    public Canvas canvas;
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
    private int prevPopUpIndex = -1;

    [SerializeField] private LayerMask layerMask;

    private GameObject currentInteractObject;
    private RectTransform objectRectTransform;
    private Vector2 lastMousePosition;
    private bool isDragging = false;

    [Header("Team UI Effect")]
    //[SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Vector2 UIAdjustedOffset = new Vector2(5, 0);

    private void Start()
    {
        for (int i = 0; i < teamUIClasses.Length; i++)
        {
            teamUIClasses[i].Initialize(i);
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
            GetTeamLinkUIObject(HandleType.Drag);
        }
       
        if (Input.GetMouseButton(0))
        {
            DragUI();
            PromptTeamUI(closestUIClass);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ExchangeSorts(closestUIClass);
        }

        if (Input.GetMouseButtonUp(1))
        {
            GetTeamLinkUIObject(HandleType.Check);
            PopOutTeamLinkOptionContent();
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
        if (currentInteractObject != null)
        {
            currentInteractObject = null;
            objectRectTransform = null;
            currentTeamUIClass = null;
            isDragging = false;
        }
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
    private void GetTeamLinkUIObject(HandleType handleType)
    {
        ResetTeamLinkObject();

        if (currentInteractObject == null)
        {
            currentInteractObject = Utils.GetMouseOverUIElement(canvas);
            if (currentInteractObject == null) { return; }

            objectRectTransform = currentInteractObject.GetComponent<RectTransform>();

            currentTeamUIClass = null;
            foreach (TeamLinkUIClass teamUI in teamUIClasses)
            {
                if (teamUI.gameObject == currentInteractObject)
                {
                    currentTeamUIClass = teamUI;
                    break; 
                }
            }

            if (handleType == HandleType.Check)
            {
                Debug.Log($"currentTeamUIClass {currentTeamUIClass.index}");
            }
            if (handleType == HandleType.Drag)
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }

            ResetTeamLinkClass();
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

    #region UI Control Methods
    private void DragUI()
    {
        if (objectRectTransform != null)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastMousePosition;
            objectRectTransform.anchoredPosition += new Vector2(delta.x, delta.y);
            lastMousePosition = currentMousePosition;
        }
    }

    private void ExchangeSorts(TeamLinkUIClass closestUIClass)
    {
        if (closestUIClass == null) return;

        Debug.Log($"Closest UIClass Image {closestUIClass.image}");
        currentTeamUIClass.Swap(closestUIClass);

        ResetTeamLinkObject();
    }

    private void PopOutTeamLinkOptionContent()
    {
        if (currentTeamUIClass == null) { return; }

        uILinkTooltip.gameObject.SetActive(true);

        int currentIndex = currentTeamUIClass.index;
        uILinkTooltip.PopOut(popUpPositions[currentIndex]);
        Debug.Log($"{currentTeamUIClass.ID}, {currentTeamUIClass.character.isLink}");
        
        prevPopUpIndex = currentIndex;
    }

    private void PopInTeamLinkOptionContent()
    {
        if (prevPopUpIndex != -1)
        {
            Debug.Log("Process PopInTeamLinkOptionContent");
            UnityEvent onComplete = new UnityEvent();
            onComplete.AddListener(() => uILinkTooltip.gameObject.SetActive(false));
            uILinkTooltip.PopIn(popUpPositions[prevPopUpIndex], onComplete);
        }
        prevPopUpIndex = -1;
    }
    #endregion

    #region Prompt UI
    //  Summary
    //      Prompt the UI to show the closest UIClass
    private void PromptTeamUI(TeamLinkUIClass closestUIClass)
    {
        MarkUI(closestUIClass);
    }

    private void MarkUI(TeamLinkUIClass closestUIClass)
    {
        if (closestUIClass == null) { return; }

        if (markedUIClass != closestUIClass)
        {
            if (markedUIClass != null) { UnmarkUI(); }

            markedUIClass = closestUIClass;
            closestUIClass.AdjustOffsetToPosition(UIAdjustedOffset);
            Debug.Log("IsProcess");
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
