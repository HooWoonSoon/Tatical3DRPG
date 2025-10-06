using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    [Serializable]
    public class SkillUIImage
    {
        public RectTransform skillListTransform;
        public Image backgroundImage;
        public Image leftConerImage;
        public Image skillImage;
        public Image lineImage;
        public TextMeshProUGUI skillText;

        public Image conditionImage;
        public Image conditionIcon;
        public TextMeshProUGUI spText;

        public SkillUIImage(Transform parent, TMP_FontAsset fontAsset = null, Sprite iconSprite = null, string skillName = null,
            int spValue = 0, Sprite iconSPSprite = null)
        {
            GenerateEmptySkillUI(parent);
            if (iconSprite != null && skillName != null)
            {
                GenerateContent(fontAsset, iconSprite, skillName);
                GenerateConditionContent(spValue, iconSPSprite);
            }
        }

        public void GenerateEmptySkillUI(Transform parent)
        {
            skillListTransform = new GameObject("Skill List").AddComponent<RectTransform>();
            skillListTransform.sizeDelta = new Vector2(450, 90);
            skillListTransform.SetParent(parent, false);

            RectTransform skillUIObject = new GameObject("Skill UI").AddComponent<RectTransform>();
            skillUIObject.SetParent(skillListTransform, false);

            backgroundImage = new GameObject("Background").AddComponent<Image>();
            backgroundImage.rectTransform.sizeDelta = new Vector2(450, 90);
            float alpha = 200 / 255f;
            backgroundImage.color = new Color(0, 0, 0, alpha);
            backgroundImage.rectTransform.SetParent(skillUIObject.transform, false);

            leftConerImage = new GameObject("Left Coner Image").AddComponent<Image>();
            leftConerImage.rectTransform.SetParent(skillListTransform, false);
            leftConerImage.rectTransform.anchoredPosition = new Vector2(-218, 0);
            leftConerImage.rectTransform.sizeDelta = new Vector2(10, 90);
        }

        public void GenerateContent(TMP_FontAsset fontAsset, Sprite icon, string skillName)
        {
            skillImage = new GameObject("Skill Type Icon").AddComponent<Image>();
            skillImage.rectTransform.SetParent(skillListTransform, false);
            skillImage.rectTransform.anchoredPosition = new Vector2(-165, 23);
            skillImage.rectTransform.sizeDelta = new Vector2(35, 35);
            skillImage.sprite = icon;

            lineImage = new GameObject("Line").AddComponent<Image>();
            lineImage.rectTransform.SetParent(skillListTransform, false);
            lineImage.rectTransform.anchoredPosition = new Vector2(0, 0);
            lineImage.rectTransform.sizeDelta = new Vector2(375, 2);

            skillText = new GameObject($"{skillName} Text").AddComponent<TextMeshProUGUI>();
            skillText.rectTransform.SetParent(skillListTransform, false);
            skillText.rectTransform.anchoredPosition = new Vector2(20, 22);
            skillText.rectTransform.sizeDelta = new Vector2(300, 24);
            skillText.font = fontAsset;
            skillText.fontSize = 24;
            skillText.text = skillName;
        }

        public void GenerateConditionContent(int requireSP, Sprite spIcon)
        {
            conditionImage = new GameObject("Condition Image").AddComponent<Image>();
            conditionImage.rectTransform.SetParent(skillListTransform, false);
            conditionImage.rectTransform.anchoredPosition = new Vector2(-92, -20);
            conditionImage.rectTransform.sizeDelta = new Vector2(110, 20);

            conditionIcon = new GameObject("Condition Icon").AddComponent<Image>();
            conditionIcon.rectTransform.SetParent(conditionImage.rectTransform, false);
            conditionIcon.rectTransform.anchoredPosition = new Vector2(-68, 0);
            conditionIcon.rectTransform.sizeDelta = new Vector2(20, 20);
            if (spIcon != null)
            {
                conditionIcon.sprite = spIcon;
            }

            spText = new GameObject("SP Text").AddComponent<TextMeshProUGUI>();
            spText.rectTransform.SetParent(conditionImage.rectTransform, false);
            spText.rectTransform.sizeDelta = new Vector2(110, 20);
            spText.fontSize = 15;
            spText.color = Color.black;
            spText.alignment = TextAlignmentOptions.Center;

            if (requireSP == 0)
            {
                spText.text = "SP ---";
            }
            else
            {
                spText.text = "SP " + requireSP.ToString();
            }
        }
    }

    [SerializeField] private GameObject skillUIContent;
    [SerializeField] private TMP_FontAsset fontAsset;
    private CharacterBase currentCharacter;
    private List<SkillUIImage> skillUIImages = new List<SkillUIImage>();
    private List<SkillData> skillDatas;
    private int selectedIndex = -1;
    
    public event Action onSkillChanged;
    public static SkillUIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (skillDatas == null || skillDatas.Count == 0) { return; }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (selectedIndex > 0)
            {
                selectedIndex -= 1;
                FocusCurrentSkillList(selectedIndex);
                currentCharacter.SetSkill(GetCurrentSelectedSkill());
                onSkillChanged?.Invoke();
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (selectedIndex < skillDatas.Count - 1)
            {
                selectedIndex += 1;
                FocusCurrentSkillList(selectedIndex);
                currentCharacter.SetSkill(GetCurrentSelectedSkill());
                onSkillChanged?.Invoke();
            }
        }
    }

    public void Initialize(List<SkillData> skillDatas, CharacterBase character)
    {
        ResetAll();
        currentCharacter = character;
        InitializeSkillList(skillDatas);
    }

    public void InitializeSkillList(List<SkillData> skillDatas)
    {
        this.skillDatas = skillDatas;

        for (int i = 0; i < skillDatas.Count; i++)
        {
            SkillUIImage skillUIImage = new SkillUIImage(skillUIContent.transform, fontAsset, 
                skillDatas[i].mainIcon, skillDatas[i].skillName, skillDatas[i].requiredSP, skillDatas[i].spIcon);
            skillUIImages.Add(skillUIImage);
        }

        if (skillDatas.Count < 4)
        {
            int release = 4 - skillDatas.Count;
            for (int i = 0; i < release; i++)
            {
                SkillUIImage skillUIImage = new SkillUIImage(skillUIContent.transform);
            }
        }

        if (skillDatas.Count >= 1)
        {
            selectedIndex = 0;
            FocusCurrentSkillList(0);
            currentCharacter.SetSkill(GetCurrentSelectedSkill());
        }
    }

    public void ResetAll()
    {
        currentCharacter = null;
        selectedIndex = -1;
        skillDatas = new List<SkillData>();
        skillUIImages = new List<SkillUIImage>();
        foreach (Transform child in skillUIContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void RecoveryAllSkillUI()
    {
        for (int i = 0; i < skillUIImages.Count; i++)
        {
            RectTransform imageRect = skillUIImages[i].leftConerImage.rectTransform;
            imageRect.anchoredPosition = new Vector2(-218, 0);
            imageRect.sizeDelta = new Vector2(10, 90);

            Image backgroundImage = skillUIImages[i].backgroundImage;
            float alpha = 200f / 255f;
            Color color = new Color(0, 0, 0, alpha);
            backgroundImage.color = color;

            Image leftConerImage = skillUIImages[i].leftConerImage;
            leftConerImage.color = Color.white;

            Image skillImage = skillUIImages[i].skillImage;
            skillImage.color = Color.white;

            Image lineImage = skillUIImages[i].lineImage;
            lineImage.color = Color.white;

            Image conditionImage = skillUIImages[i].conditionImage;
            conditionImage.color = Color.white;

            Image conditionIcon = skillUIImages[i].conditionIcon;
            conditionIcon.color = Color.white;

            TextMeshProUGUI skillText = skillUIImages[i].skillText;
            skillText.color = Color.white;

            TextMeshProUGUI spText = skillUIImages[i].spText;
            spText.color = Color.black;
        }
    }

    #region External Methods
    public void FocusCurrentSkillList(int index)
    {
        RecoveryAllSkillUI();

        if (index < 0 || index >= skillUIImages.Count) { return; }

        RectTransform imageRect = skillUIImages[index].leftConerImage.rectTransform;
        StartCoroutine(Utils.UIExtraMoveCoroutine(imageRect, new Vector2(5, 0), 0.2f));
        StartCoroutine(Utils.UIExtraScaleCoroutine(imageRect, new Vector2(10, 0), 0.2f));

        Image backgroundImage = skillUIImages[index].backgroundImage;
        StartCoroutine(Utils.UIColorInverseCorroutine(backgroundImage, 0.2f));
        Image leftConerImage = skillUIImages[index].leftConerImage;
        StartCoroutine(Utils.UIColorInverseCorroutine(leftConerImage, 0.2f));
        Image skillImage = skillUIImages[index].skillImage;
        StartCoroutine(Utils.UIColorInverseCorroutine(skillImage, 0.2f));
        Image lineImage = skillUIImages[index].lineImage;
        StartCoroutine(Utils.UIColorInverseCorroutine(lineImage, 0.2f));
        Image conditionImage = skillUIImages[index].conditionImage;
        StartCoroutine(Utils.UIColorInverseCorroutine(conditionImage, 0.2f));
        Image conditionIcon = skillUIImages[index].conditionIcon;
        StartCoroutine(Utils.UIColorInverseCorroutine(conditionIcon, 0.2f));
        TextMeshProUGUI skillText = skillUIImages[index].skillText;
        StartCoroutine(Utils.TextColorInverseCorroutine(skillText, 0.2f));
        TextMeshProUGUI spText = skillUIImages[index].spText;
        StartCoroutine(Utils.TextColorInverseCorroutine(spText, 0.2f));
    }

    public SkillData GetCurrentSelectedSkill()
    {
        return skillDatas[selectedIndex];
    }

    #endregion
}
