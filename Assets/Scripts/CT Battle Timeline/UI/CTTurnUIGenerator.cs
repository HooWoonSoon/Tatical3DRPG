using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(RectTransform))]
public class CTTurnUIGenerator : MonoBehaviour
{
    [Serializable]
    public class TurnUIImage
    {
        public Image characterImage;
        public Image backgroundPanel;
        public TextMeshProUGUI turnUIText;

        public TurnUIImage(Transform parent, CharacterBase character, TMP_FontAsset fontAsset, int turnCount)
        {
            CreateBackgroundPanel(parent);
            CreateCharacterImage(backgroundPanel.transform, character);
            CreateTurnText(backgroundPanel.transform, fontAsset, turnCount);
        }

        private void CreateBackgroundPanel(Transform parent)
        {
            backgroundPanel = new GameObject("BackgroundPanel").AddComponent<Image>();
            backgroundPanel.transform.SetParent(parent, false);
            backgroundPanel.rectTransform.sizeDelta = new Vector2(140, 100);
            backgroundPanel.color = new Color(0, 0, 0, 220 / 255f);
        }

        private void CreateCharacterImage(Transform parent, CharacterBase character)
        {
            characterImage = new GameObject("CharacterImage").AddComponent<Image>();
            characterImage.gameObject.transform.parent = parent;
            characterImage.rectTransform.anchoredPosition = Vector2.zero;
            characterImage.rectTransform.sizeDelta = new Vector2(130, 90);
            characterImage.sprite = character.data.characterTurnUISprite;
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
    [SerializeField] private GameObject turnUIContent;
    [SerializeField] private GameObject turnPhaseUI;
    [SerializeField] private List<TurnUIImage> turnUIImages = new List<TurnUIImage>();
    [SerializeField] private int turnCount = 0;
    [SerializeField] private TMP_FontAsset fontAsset;
    private List<CharacterBase> currentTurncharacters;

    private void Start()
    {
        UIManager.instance.onReadyBattlePanel += GenerateTimelineUI;
    }

    private void GenerateTimelineUI()
    {
        List<CTTurn> allCTTurn = CTTimeline.instance.GetAllTurnHistory();
        ResetTurnUI();
        for (int i = 0; i < allCTTurn.Count; i++)
        {
            currentTurncharacters = allCTTurn[i].cTTimelineQueue;
            turnCount = allCTTurn[i].turnCount;
            if (currentTurncharacters.Count <= 0) { return; }
            GenerateTurnImages();
        }
    }
    private void GenerateTurnImages()
    {
        GameObject turnPhaseGameObject = Instantiate(turnPhaseUI);
        turnPhaseGameObject.transform.SetParent(turnUIContent.transform);
        for (int i = 0; i < currentTurncharacters.Count; i++)
        {
            TurnUIImage turnUIImage = new TurnUIImage(turnUIContent.transform, currentTurncharacters[i], fontAsset, turnCount);
            turnUIImages.Add(turnUIImage);
        }
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
