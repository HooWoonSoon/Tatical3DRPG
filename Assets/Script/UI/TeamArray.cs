using UnityEngine;

public class TeamArray : MonoBehaviour
{
    public Canvas canvas;
    public TeamUIClass[] teamUIClasses;
    private TeamUIClass currentDragUIClass;
    private TeamUIClass markedUIClass;
    //private Team team;

    [SerializeField] private LayerMask layerMask;

    private GameObject draggingObject;
    private RectTransform draggingRectTransform;
    private Vector2 lastMousePosition;
    private bool isDragging = false;

    [Header("Team UI Effect")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Vector2 UIAdjustedOffset = new Vector2(5, 0);

    private void Start()
    {
        foreach (TeamUIClass teamUI in teamUIClasses)
        {
            teamUI.Initialize();
        }
    }

    private void Update()
    {
        TeamUIClass closestUIClass = GetClosestUIClass();
        
        if (Input.GetMouseButtonDown(0))
        {
            GetUIObject();
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
    }

    private void GetUIObject()
    {
        if (draggingObject == null)
        {
            draggingObject = Utils.GetMouseOverUIElement(canvas);
            if (draggingObject != null)
            {
                draggingRectTransform = draggingObject.GetComponent<RectTransform>();
                foreach (TeamUIClass teamUI in teamUIClasses)
                {
                    if (teamUI.gameObject == draggingObject) { currentDragUIClass = teamUI; }
                }
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }
        }
    }

    private void DragUI()
    {
        if (draggingRectTransform != null)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastMousePosition;
            draggingRectTransform.anchoredPosition += new Vector2(delta.x, delta.y);
            lastMousePosition = currentMousePosition;
        }
    }

    private void ExchangeSorts(TeamUIClass closestUIClass)
    {
        if (closestUIClass == null) return;

        Debug.Log($"Closest UIClass Image {closestUIClass.image}");
        currentDragUIClass.SwapPosition(closestUIClass);
        
        draggingObject = null;
        draggingRectTransform = null;
        currentDragUIClass = null;
        isDragging = false;
    }

    private TeamUIClass GetClosestUIClass()
    {
        if (currentDragUIClass == null) { return null; }

        TeamUIClass closestUIClass = null;
        float closestDistance = float.MaxValue;

        foreach (TeamUIClass teamUI in teamUIClasses)
        {
            if (draggingRectTransform != null && teamUI != currentDragUIClass)
            {
                float distance = Vector2.Distance(draggingRectTransform.anchoredPosition, teamUI.rectPosition);
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
}
