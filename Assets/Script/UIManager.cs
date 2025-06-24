using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject exploreStatePanel;
    public GameObject battleStatePanel;

    public event Action onReadyExplorePanel;
    public event Action onReadyBattlePanel;

    public static UIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start() => CTTimeline.instance.confirmCTTimeline += ReadyBattlePanel;

    private void ReadyBattlePanel()
    {
        exploreStatePanel.SetActive(true);
        battleStatePanel.SetActive(true);
        onReadyBattlePanel?.Invoke();
    }

    private void ReadyExplorePanel()
    {
        exploreStatePanel.SetActive(true);
        battleStatePanel.SetActive(false);
        onReadyExplorePanel?.Invoke();
    }
}

