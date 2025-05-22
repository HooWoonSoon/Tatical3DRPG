using TMPro;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CTTurnUI : MonoBehaviour
{
    [Header("Unit Image")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image backgroundPanel;
    private Vector2 position;

    [SerializeField] private TextMeshProUGUI turnUIText;

    private void Start()
    {
        position = new Vector2(-80, 0);
        backgroundPanel.rectTransform.anchoredPosition = position;
        
    }
}
