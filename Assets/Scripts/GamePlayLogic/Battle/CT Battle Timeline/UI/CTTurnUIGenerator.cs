using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(RectTransform))]
public class CTTurnUIGenerator : MonoBehaviour
{
    public class TurnUIImage
    {
        public Image characterImage;
        public Image backgroundPanel;
        public TextMeshProUGUI turnUIText;

        public CharacterBase character;
        public int turnCount;

        public TurnUIImage(Transform parent, CharacterBase character, TMP_FontAsset fontAsset, int turnCount)
        {
            this.character = character;
            this.turnCount = turnCount;

            CreateBackgroundPanel(parent);
            CreateCharacterImage(backgroundPanel.transform);
            CreateTurnText(backgroundPanel.transform, fontAsset);
        }

        private void CreateBackgroundPanel(Transform parent)
        {
            backgroundPanel = new GameObject("BackgroundPanel").AddComponent<Image>();
            backgroundPanel.transform.SetParent(parent, false);
            backgroundPanel.rectTransform.sizeDelta = new Vector2(140, 100);
            backgroundPanel.color = new Color(0, 0, 0, 220 / 255f);
        }

        private void CreateCharacterImage(Transform parent)
        {
            characterImage = new GameObject("CharacterImage").AddComponent<Image>();
            characterImage.gameObject.transform.SetParent(parent, false);
            characterImage.rectTransform.anchoredPosition = Vector2.zero;
            characterImage.rectTransform.sizeDelta = new Vector2(130, 90);
            characterImage.sprite = character.data.characterTurnUISprite;
        }

        private void CreateTurnText(Transform parent, TMP_FontAsset fontAsset)
        {
            turnUIText = new GameObject("TurnText").AddComponent<TextMeshProUGUI>();
            turnUIText.gameObject.transform.SetParent(parent, false);
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
    [SerializeField] private GameObject targetImageObject;
    [SerializeField] private GameObject turnUIContent;
    [SerializeField] private GameObject turnPhaseUI;
    [SerializeField] private List<TurnUIImage> turnUIImages = new List<TurnUIImage>();
    [SerializeField] private TMP_FontAsset fontAsset;
    private List<CTTurn> allCTTurn;
    private int currentTurnCount;

    [Header("UI Smooth Move")]
    [SerializeField] private float smoothTime = 0.2f;
    private Vector2 targetAnchoredPos;
    private bool isFocusing = false;
    private Vector2 velocity = Vector2.zero;

    private void OnEnable()
    {
        CTTimeline.instance.confirmCTTimeline += GenerateTimelineUI;
    }

    private void OnDisable()
    {
        CTTimeline.instance.confirmCTTimeline -= GenerateTimelineUI;
    }

    private void Update()
    {
        if (isFocusing)
        {
            RectTransform rectContent = turnUIContent.GetComponent<RectTransform>();
            rectContent.anchoredPosition = Vector2.SmoothDamp(rectContent.anchoredPosition, targetAnchoredPos, ref velocity, smoothTime);

            if (Vector2.Distance(rectContent.anchoredPosition, targetAnchoredPos) < 0.1f)
            {
                rectContent.anchoredPosition = targetAnchoredPos;
                isFocusing = false;
            }
        }
    }

    private void GenerateTimelineUI()
    {
        allCTTurn = CTTimeline.instance.GetAllTurnHistory();
        if (allCTTurn.Count == 0) { return; }
        ResetTurnUI();
        GenerateTurnImages();
    }

    private void GenerateTurnImages()
    {
        for (int i = 0; i < allCTTurn.Count; i++)
        {
            GameObject turnPhaseGameObject = Instantiate(turnPhaseUI);
            turnPhaseGameObject.transform.SetParent(turnUIContent.transform);

            for (int j = 0; j < allCTTurn[i].GetCharacterQueue().Count; j++)
            {
                TurnUIImage turnUIImage = new TurnUIImage(turnUIContent.transform, allCTTurn[i].GetCharacterQueue()[j], fontAsset, allCTTurn[i].turnCount);
                turnUIImages.Add(turnUIImage);
            }
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

    #region External Called
    public void TargetCursorCharacterUI(CharacterBase character)
    {
        for (int i = 0; i < turnUIImages.Count; i++)
        {
            if (turnUIImages[i].turnCount != currentTurnCount) { continue; }
            else
            {
                if (turnUIImages[i].character == character)
                {
                    UpdateTargetCharacterUI(character);
                    RectTransform target = turnUIImages[i].backgroundPanel.rectTransform;
                    FocusOnCharacterUI(target);

                }
            }
        }
    }
    #endregion

    private void FocusOnCharacterUI(RectTransform target)
    {
        RectTransform rectContent = turnUIContent.GetComponent<RectTransform>();
        RectTransform rectviewport = rectContent.parent.GetComponent<RectTransform>();
        HorizontalLayoutGroup horizontalLayoutGroup = rectContent.GetComponent<HorizontalLayoutGroup>();

        float targetX = target.anchoredPosition.x + target.rect.width * 0.5f;
        float distance = rectContent.anchoredPosition.x + targetX + horizontalLayoutGroup.spacing * 0.5f;

        targetAnchoredPos = new Vector2(rectContent.anchoredPosition.x - distance, rectContent.anchoredPosition.y);
        isFocusing = true;
    }

    private void UpdateTargetCharacterUI(CharacterBase character)
    {
        Image image = targetImageObject.GetComponent<Image>();
        if (image == null) { return; }

        Sprite sprite = character.data.characterTurnUISprite;
        if (sprite != null)
        {
            image.sprite = sprite;
        }
    }
}
