using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelfCanvasController : MonoBehaviour
{
    public Canvas selfCanvas;
    public Image healtUI;
    public TextMeshProUGUI queueTextUI;

    public void SetQueue(int number)
    {
        queueTextUI.text = number.ToString();
    }
}
