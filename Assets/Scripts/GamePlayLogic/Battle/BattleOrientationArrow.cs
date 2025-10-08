using NUnit.Framework;
using UnityEngine;

public class BattleOrientationArrow : MonoBehaviour
{
    [SerializeField] private GameObject frontArrow;
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    [SerializeField] private GameObject backArrow;

    private GameObject[] arrows;
    private bool activateArrow = false;

    public Material normalMat;
    public Material highlightMat;

    private void Update()
    {
        if (!activateArrow) { return; }
        if (Input.GetKeyDown(KeyCode.W))
        {
            HighlightOrientationArrow(Orientation.forward);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            HighlightOrientationArrow(Orientation.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            HighlightOrientationArrow(Orientation.right);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            HighlightOrientationArrow(Orientation.back);
        }
    }

    public void ShowArrows(Orientation orientation, GameNode targetNode, float centerOffset = 1.2f)
    {
        arrows = new GameObject[] { frontArrow, leftArrow, rightArrow, backArrow };
        Vector3 target = targetNode.GetVector();

        frontArrow.SetActive(true);
        frontArrow.transform.position = target + new Vector3(0, 1f, centerOffset);
        leftArrow.SetActive(true);
        leftArrow.transform.position = target + new Vector3(-centerOffset, 1f, 0);
        rightArrow.SetActive(true);
        rightArrow.transform.position = target + new Vector3(centerOffset, 1f, 0);
        backArrow.SetActive(true);
        backArrow.transform.position = target + new Vector3(0, 1f, -centerOffset);
        activateArrow = true;
    }

    public void HighlightOrientationArrow(Orientation orientation)
    {
        foreach (var arrow in arrows)
        {
            var renderer = arrow.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material = normalMat;
        }

        GameObject selectedArrow = null;
        switch (orientation)
        {
            case Orientation.forward:
                selectedArrow = frontArrow;
                break;
            case Orientation.left:
                selectedArrow = leftArrow;
                break;
            case Orientation.right:
                selectedArrow = rightArrow;
                break;
            case Orientation.back:
                selectedArrow = backArrow;
                break;
        }

        if (selectedArrow != null)
        {
            var renderer = selectedArrow.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material = highlightMat;
        }
    }

    public void HideAll()
    {
        foreach (var arrow in arrows)
            arrow.SetActive(false);
        activateArrow = false;
    }
}
