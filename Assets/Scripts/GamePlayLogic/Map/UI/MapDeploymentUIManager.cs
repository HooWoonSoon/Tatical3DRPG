using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapDeploymentUIManager : Entity
{
    public class UIImage
    {
        public GameObject tagObject;
        public UIImage(Transform parent, CharacterData data, Sprite tagSprite)
        {
            RectTransform imageRect = new GameObject($"{data.characterName} Deployment Image").AddComponent<RectTransform>();
            imageRect.SetParent(parent, false);
            imageRect.sizeDelta = new Vector2(230, 120);

            Image image = imageRect.AddComponent<Image>();
            image.sprite = data.characterTurnUISprite;

            tagObject = new GameObject("Arrow");
            RectTransform tag = tagObject.AddComponent<RectTransform>();
            tag.SetParent(imageRect, false);
            tag.anchoredPosition = new Vector2(0, -50);
            tag.sizeDelta = new Vector2(40, 40);
            Image tagImage = tag.gameObject.AddComponent<Image>();
            tagImage.sprite = tagSprite;
            tagObject.SetActive(false);
        }
    }

    public GameObject characterDeploymentPanel;
    public GameObject deploymentScrollView;
    public GameObject characterDeploymentInformation;
    public Transform deploymentContent;

    public List<UIImage> uIImages = new List<UIImage>();
    [SerializeField] private Sprite tagSprite;
    [SerializeField] private int columns;

    private CharacterBase[] characters;
    private CharacterBase currentCharacter;
    [SerializeField] private int deployableCount; // Debug

    public int selectedIndex { get; private set; } = -1;
    public static MapDeploymentUIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        characterDeploymentPanel.SetActive(false);
        MapDeploymentManager.instance.onDeploymentTrigger += ShowDeploymentUI;
        GridLayoutGroup layoutGroup = deploymentContent.GetComponent<GridLayoutGroup>();
        if (layoutGroup != null)
        {
            columns = layoutGroup.constraintCount;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (characterDeploymentPanel.activeSelf)
            {
                characterDeploymentPanel.SetActive(false);
                MapDeploymentManager.instance.ActivateMoveCursorAndHide(true, false);
            }
            else
            {
                characterDeploymentPanel.SetActive(true);
                MapDeploymentManager.instance.ActivateMoveCursorAndHide(false, true);
            }
        }
        HandleSelectionInput();
    }

    private void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
            selectedIndex++;
        else if (Input.GetKeyDown(KeyCode.A))
            selectedIndex--;
        else if (Input.GetKeyDown(KeyCode.S))
            selectedIndex += columns;
        else if (Input.GetKeyDown(KeyCode.W))
            selectedIndex -= columns;

        //  limited the selected range
        selectedIndex = Mathf.Clamp(selectedIndex, 0, uIImages.Count - 1);
        FocusOnCurrentCharacterUI();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            MapDeploymentManager.instance.RandomDeploymentCharacter(currentCharacter);
        }
    }

    public void FocusOnCurrentCharacterUI()
    {
        if (selectedIndex == -1 || uIImages.Count == 0) { return; }

        foreach (UIImage image in uIImages)
        {
            image.tagObject.SetActive(false);
        }
        uIImages[selectedIndex].tagObject.SetActive(true);
        currentCharacter = characters[selectedIndex];
    }

    public void ShowDeploymentUI()
    {
        ResetAll();
        characterDeploymentPanel.SetActive(true);
        characters = AvailableCharacterManager.instance.allCharacter;
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterData data = characters[i].data;
            UIImage characterUI = new UIImage(deploymentContent, data, tagSprite);
            uIImages.Add(characterUI);
        }
        FocusOnCurrentCharacterUI();
    }

    private void ResetAll()
    {
        uIImages = new List<UIImage>();
        foreach (Transform child in deploymentContent)
        {
            Destroy(child.gameObject);
        }
    }
}

