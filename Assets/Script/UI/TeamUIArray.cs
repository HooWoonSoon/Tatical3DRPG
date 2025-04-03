using UnityEngine;

enum HandleType
{
    Drag, Check
}

public class TeamUIArray : MonoBehaviour
{
    public Canvas canvas;
    public TeamUIClass[] teamUIClasses;
    private TeamUIClass currentInteractUIClass;
    private TeamUIClass markedUIClass;

    public UILinkTooltip uILinkTooltip;

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
    }

    private void Update()
    {
        TeamUIClass closestUIClass = GetClosestUIClass();
        
        if (Input.GetMouseButtonDown(0))
        {
            GetUIObject(HandleType.Drag);
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
            GetUIObject(HandleType.Check);
            ShowingDetail();
        }
    }

    private void GetUIObject(HandleType handleType)
    {
        if (currentInteractObject == null)
        {
            currentInteractObject = Utils.GetMouseOverUIElement(canvas);
            if (currentInteractObject != null)
            {
                objectRectTransform = currentInteractObject.GetComponent<RectTransform>();
                foreach (TeamUIClass teamUI in teamUIClasses)
                {
                    if (teamUI.gameObject == currentInteractObject) { currentInteractUIClass = teamUI; }
                }

                if (handleType == HandleType.Drag)
                {
                    lastMousePosition = Input.mousePosition;
                    isDragging = true;
                }
            }
        }
    }

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

    private void ExchangeSorts(TeamUIClass closestUIClass)
    {
        if (closestUIClass == null) return;

        Debug.Log($"Closest UIClass Image {closestUIClass.image}");
        currentInteractUIClass.Swap(closestUIClass);
        
        currentInteractObject = null;
        objectRectTransform = null;
        currentInteractUIClass = null;
        isDragging = false;
    }

    private TeamUIClass GetClosestUIClass()
    {
        if (currentInteractUIClass == null) { return null; }

        TeamUIClass closestUIClass = null;
        float closestDistance = float.MaxValue;

        foreach (TeamUIClass teamUI in teamUIClasses)
        {
            if (objectRectTransform != null && teamUI != currentInteractUIClass)
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

    #region Prompt UI
    private void PromptTeamUI(TeamUIClass closestUIClass)
    {
        MarkUI(closestUIClass);
    }

    private void MarkUI(TeamUIClass closestUIClass)
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

    private void ShowingDetail()
    {
        //  Testing
        UIPopupEffectScaleAndOffset.ApplyAnimation(this, uILinkTooltip, 0.3f, true);
        Debug.Log($"{currentInteractUIClass.ID}, {currentInteractUIClass.character.isLink}");
    }
}
