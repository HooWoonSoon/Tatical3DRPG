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
        public int roundCount;
        public int turnCount;

        public TurnUIImage(Transform parent, CharacterBase character, TMP_FontAsset fontAsset, int roundCount, int turnQueue)
        {
            this.character = character;
            this.roundCount = roundCount;
            this.turnCount = turnQueue;

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
            turnUIText.text = $"Turn {roundCount}";
        }

    }

    [Header("Unit Image")]
    [SerializeField] private GameObject targetUnitImage;
    [SerializeField] private GameObject turnUIContent;
    [SerializeField] private GameObject turnPhaseUI;
    [SerializeField] private TMP_FontAsset fontAsset;
    private List<TurnUIImage> turnUIImages = new List<TurnUIImage>();
    private List<TurnUIImage> pastTurnUIImages = new List<TurnUIImage>();

    private List<CTRound> allCTRound;
    private CTRound currentCTTurn;
    private int currentTurnIndex = 0;
    private int generatedRoundIndex = -1;
    private int generatedTurnIndex = -1;
    
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

    public void GenerateTimelineUI()
    {
        ResetTurnUI();
        GenerateTurnUI();
        //  Wait 1 frame to let LayoutGroup/Canvas finish
        StartCoroutine(DelayTargetFocus());
    }
    private IEnumerator DelayTargetFocus()
    {
        yield return null;
        TargetCurrentCTTurnUI(CTTimeline.instance.GetCurrentCTTurn(), CTTimeline.instance.GetCurrentTurn());
    }

    private void GenerateTurnUI()
    {
        for (int i = 0; i < allCTRound.Count; i++)
        {
            GameObject turnPhaseGameObject = Instantiate(turnPhaseUI);
            turnPhaseGameObject.transform.SetParent(turnUIContent.transform, false);

            List<CharacterBase> characterQueues = allCTRound[i].GetCharacterQueue();

            for (int j = 0; j < characterQueues.Count; j++)
            {
                TurnUIImage turnUIImage = new TurnUIImage(turnUIContent.transform, allCTRound[i].GetCharacterQueue()[j], fontAsset, allCTRound[i].roundCount, j);
                turnUIImages.Add(turnUIImage);
                generatedTurnIndex++;
            }
            generatedRoundIndex++;
        }
    }

    public void AppendTurnUI()
    {
        allCTRound = CTTimeline.instance.GetAllTurnHistory();

        //  Check release round
        if (generatedRoundIndex >= allCTRound.Count) { return; }

        CTRound cTRound = allCTRound[generatedRoundIndex];
        List<CharacterBase> queue = cTRound.GetCharacterQueue();

        generatedTurnIndex++;   //  Next generate
        if (generatedTurnIndex >= queue.Count)
        {
            generatedRoundIndex++;
            generatedTurnIndex = 0;

            GameObject turnPhaseGameObject = Instantiate(turnPhaseUI);
            turnPhaseGameObject.transform.SetParent(turnUIContent.transform, false);

            if (generatedRoundIndex >= allCTRound.Count) { return; }

            cTRound = allCTRound[generatedRoundIndex];
            queue = cTRound.GetCharacterQueue();
        }

        CharacterBase character = cTRound.GetCharacterAt(generatedTurnIndex);
        if (character != null)
        {
            TurnUIImage turnUIImage = new TurnUIImage(turnUIContent.transform, character, 
                fontAsset, generatedRoundIndex, generatedTurnIndex);
            turnUIImages.Add(turnUIImage);
        }
    }

    public void ResetTurnUI()
    {
        generatedRoundIndex = -1;
        generatedTurnIndex = -1;
        allCTRound = CTTimeline.instance.GetAllTurnHistory();
        if (allCTRound.Count == 0)
        {
            Debug.LogWarning("CTTurnUIManager: No CTRound in history!");
            return;
        }

        foreach (Transform child in turnUIContent.transform)
        {
            Destroy(child.gameObject);
        }
        turnUIImages.Clear();
    }

    #region External Called
    public void RecordPastTurnUI(CTRound cTRound, int currentTurnIndex)
    {
        for (int i = turnUIImages.Count - 1; i >= 0 ; i--)
        {
            if (turnUIImages[i].roundCount == cTRound.roundCount)
            {
                if (turnUIImages[i].turnCount == currentTurnIndex)
                {
                    pastTurnUIImages.Add(turnUIImages[i]);
                    break;
                }
            }
        }
    }
    public void TargetCurrentCTTurnUI(CTRound cTRound, int currentTurn)
    {
        currentCTTurn = cTRound;
        this.currentTurnIndex = currentTurn;

        for (int i = 0; i < turnUIImages.Count; i++)
        {
            if (turnUIImages[i].roundCount == cTRound.roundCount)
            {
                if (turnUIImages[i].turnCount == currentTurn)
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
            if (turnUIImages[i].roundCount != currentCTTurn.roundCount) { continue; }
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
