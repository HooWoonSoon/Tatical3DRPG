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

    public void SetHeathPercetange(float percentage)
    {
        healtUI.type = Image.Type.Filled;
        healtUI.fillMethod = Image.FillMethod.Horizontal;
        healtUI.fillAmount = percentage;
    }
}
