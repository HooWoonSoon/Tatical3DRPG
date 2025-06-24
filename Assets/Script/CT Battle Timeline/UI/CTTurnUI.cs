using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(RectTransform))]
public class CTTurnUI : MonoBehaviour
{
    [Serializable]
    public class TurnUIImage
    {
        public Image characterImage;
        public Image backgroundPanel;
        public TextMeshProUGUI turnUIText;

        public TurnUIImage(Vector2 swapPosition, Transform parent, TMP_FontAsset fontAsset, int turnCount)
        {
            CreateBackgroundPanel(parent, swapPosition);
            CreateCharacterImage(backgroundPanel.transform);
            CreateTurnText(backgroundPanel.transform, fontAsset, turnCount);
        }

        private void CreateBackgroundPanel(Transform parent, Vector2 position)
        {
            backgroundPanel = new GameObject("BackgroundPanel").AddComponent<Image>();
            backgroundPanel.gameObject.transform.parent = parent;
            backgroundPanel.rectTransform.anchoredPosition = position;
            backgroundPanel.rectTransform.sizeDelta = new Vector2(140, 100);
            backgroundPanel.color = new Color(0, 0, 0, 220 / 255f);
        }

        private void CreateCharacterImage(Transform parent)
        {
            characterImage = new GameObject("CharacterImage").AddComponent<Image>();
            characterImage.gameObject.transform.parent = parent;
            characterImage.rectTransform.anchoredPosition = Vector2.zero;
            characterImage.rectTransform.sizeDelta = new Vector2(130, 90);
        }

        private void CreateTurnText(Transform parent, TMP_FontAsset fontAsset, int turnCount)
        {
            turnUIText = new GameObject("TurnText").AddComponent<TextMeshProUGUI>();
            turnUIText.gameObject.transform.parent = parent;
            turnUIText.rectTransform.anchoredPosition = new Vector2(-20, -56);
            turnUIText.rectTransform.sizeDelta = new Vector2(100, 20);
            turnUIText.fontSize = 16;
            turnUIText.fontStyle = FontStyles.Italic | FontStyles.SmallCaps;
            turnUIText.font = fontAsset;
            turnUIText.color = new Color(165, 165, 165);
            turnUIText.text = $"Turn {turnCount}";
        }

    }

    [Header("Unit Image")]
    [SerializeField] private List<TurnUIImage> turnUIImages = new List<TurnUIImage>();
    [SerializeField] private Vector2 swapPosition = new Vector2(-80, 0);
    [SerializeField] private int offsetXValue = 150;
    [SerializeField] private int generateCount;
    [SerializeField] private int turnCount = 0;
    [SerializeField] private UILineTextureRenderer lineRenderer;
    [SerializeField] private TMP_FontAsset fontAsset;

    private void OnEnable()
    {
        UIManager.instance.onReadyBattlePanel += GenerateTimelineUI;
    }

    private void OnDisable()
    {
        UIManager.instance.onReadyBattlePanel -= GenerateTimelineUI;
    }

    private void GenerateTimelineUI()
    {
        generateCount = CTTimeline.instance.cTTurnhistory[0].cTTimelineQueue.Count;
        turnCount = CTTimeline.instance.cTTurnhistory[0].turnCount;

        if (generateCount <= 0) { return; }

        ResetTurnUI();
        SetupLineRenderer();
        GenerateTurnImages();
    }

    private void GenerateTurnImages()
    {
        Vector2 basePosition = swapPosition;
        for (int i = 0; i < generateCount; i++)
        {
            Vector2 position = new Vector2(basePosition.x + offsetXValue * i, basePosition.y);
            TurnUIImage turnUIImage = new TurnUIImage(position, this.transform, fontAsset, turnCount);
            turnUIImages.Add(turnUIImage);
        }
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            CreateLineRenderer();
        }

        UpdateLineRendererPoints();
    }

    private void CreateLineRenderer()
    {
        lineRenderer = new GameObject("LineRenderer").AddComponent<UILineTextureRenderer>();
        lineRenderer.gameObject.transform.parent = this.transform;
        lineRenderer.rectTransform.sizeDelta = new Vector2(180, 10);
        lineRenderer.rectTransform.anchoredPosition = new Vector2(-72.5f, -60);
    }

    private void UpdateLineRendererPoints()
    {
        if (lineRenderer.Points == null || lineRenderer.Points.Length != 3)
        {
            lineRenderer.Points = new Vector2[3];
        }

        lineRenderer.Points[0] = new Vector2(0, 125);
        lineRenderer.Points[1] = new Vector2(0, 0);
        lineRenderer.Points[2] = new Vector2(5 + offsetXValue * generateCount, 0);
        lineRenderer.LineThickness = 5;
        lineRenderer.color = new Color(0, 0, 0, 220 / 255f);
        lineRenderer.SetAllDirty();
    }

    private void ResetTurnUI()
    {
        for (int i = 0; i < turnUIImages.Count; i++)
        {
            if (turnUIImages[i] != null)
            {
                if (turnUIImages[i].backgroundPanel != null && turnUIImages[i].backgroundPanel.gameObject != null)
                {
                    Destroy(turnUIImages[i].backgroundPanel.gameObject);
                }
                if (turnUIImages[i].characterImage != null && turnUIImages[i].characterImage.gameObject != null)
                {
                    Destroy(turnUIImages[i].characterImage.gameObject);
                }
            }
        }
        turnUIImages.Clear();
    }
}
