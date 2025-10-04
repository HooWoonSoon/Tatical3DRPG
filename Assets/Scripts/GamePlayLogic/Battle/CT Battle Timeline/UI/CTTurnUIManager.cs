using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class CTTurnUIManager : MonoBehaviour
{
    [Serializable]
    public class TurnUIImage
    {
        public Image characterImage;
        public Image backgroundImage;
        public TextMeshProUGUI turnUIText;

        public CharacterBase character;
        public int turnCount;
        public int turnQueue;

        public TurnUIImage(Transform parent, CharacterBase character, TMP_FontAsset fontAsset, int turnCount, int turnQueue)
        {
            this.character = character;
            this.turnCount = turnCount;
            this.turnQueue = turnQueue;

            CreateBackgroundPanel(parent);
            CreateCharacterImage(backgroundImage.transform);
            CreateTurnText(backgroundImage.transform, fontAsset);
        }

        private void CreateBackgroundPanel(Transform parent)
        {
            backgroundImage = new GameObject("BackgroundPanel").AddComponent<Image>();
            backgroundImage.transform.SetParent(parent, false);
            backgroundImage.rectTransform.sizeDelta = new Vector2(140, 100);
            backgroundImage.color = new Color(0, 0, 0, 220 / 255f);
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
    [SerializeField] private GameObject targetUnitImage;
    [SerializeField] private GameObject turnUIContent;
    [SerializeField] private GameObject turnPhaseUI;
    private List<TurnUIImage> turnUIImages = new List<TurnUIImage>();
    [SerializeField] private TMP_FontAsset fontAsset;
    private List<CTTurn> allCTTurn;
    private CTTurn currentCTTurn;
    private int currentTurnIndex = 0;

    [Header("UI Smooth Move")]
    [SerializeField] private float smoothTime = 0.2f;
    private Vector2 targetAnchoredPos;
    private bool isFocusing = false;
    private Vector2 velocity = Vector2.zero;
    public static CTTurnUIManager instance {  get; private set; }

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        CTTimeline.instance.confirmCTTimeline += GenerateTimelineUI;
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
        //  Wait 1 frame to let LayoutGroup/Canvas finish
        StartCoroutine(DelayTargetFocus());
    }
    private IEnumerator DelayTargetFocus()
    {
        yield return null;
        TargetCurrentCTTurnUI(CTTimeline.instance.GetCurrentCTTurn(), CTTimeline.instance.GetCurrentTurnIndex());
    }

    private void GenerateTurnImages()
    {
        for (int i = 0; i < allCTTurn.Count; i++)
        {
            GameObject turnPhaseGameObject = Instantiate(turnPhaseUI);
            turnPhaseGameObject.transform.SetParent(turnUIContent.transform);

            List<CharacterBase> characterQueues = allCTTurn[i].GetCharacterQueue();
            for (int j = 0; j < characterQueues.Count; j++)
            {
                TurnUIImage turnUIImage = new TurnUIImage(turnUIContent.transform, allCTTurn[i].GetCharacterQueue()[j], fontAsset, allCTTurn[i].turnCount, j);
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
                if (turnUIImages[i].backgroundImage != null && turnUIImages[i].backgroundImage.gameObject != null)
                {
                    Destroy(turnUIImages[i].backgroundImage.gameObject);
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
    public void TargetCurrentCTTurnUI(CTTurn turn, int currentTurnIndex)
    {
        currentCTTurn = turn;
        this.currentTurnIndex = currentTurnIndex;

        for (int i = 0; i < turnUIImages.Count; i++)
        {
            if (turnUIImages[i].turnCount == turn.turnCount)
            {
                if (turnUIImages[i].turnQueue == currentTurnIndex)
                {
                    UpdateTargetCharacterUI(turnUIImages[i].character);
                    RectTransform target = turnUIImages[i].backgroundImage.rectTransform;
                    FocusOnCharacterUI(target);
                    break;
                }
            }
        }
    }

    public void TargetCursorCharacterUI(CharacterBase character)
    {
        for (int i = 0; i < turnUIImages.Count; i++)
        {
            if (turnUIImages[i].turnCount != currentCTTurn.turnCount) { continue; }
            else
            {
                if (turnUIImages[i].character == character)
                {
                    UpdateTargetCharacterUI(character);
                    RectTransform target = turnUIImages[i].backgroundImage.rectTransform;
                    FocusOnCharacterUI(target);
                    break;
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
        Image image = targetUnitImage.GetComponent<Image>();
        if (image == null) { return; }

        Sprite sprite = character.data.characterTurnUISprite;
        if (sprite != null)
        {
            image.sprite = sprite;
        }
    }
}
