using System;
using UnityEngine;

public class ExploreUIManager : MonoBehaviour
{
    public GameObject exploreStatePanel;

    public static ExploreUIManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameEvent.OnBattleUIFinish += DisableExploreMachanine;
        GameEvent.onStartDeployment += DisableExploreMachanine;
        GameEvent.onEndBattle += EnableExploreMachanine;
        EnableExploreMachanine();
    }

    private void EnableExploreMachanine()
    {
        exploreStatePanel.SetActive(true);
        PlayerTeamLinkUIManager.instance.ActivateTeamLinkUI(true);
    }
    private void DisableExploreMachanine()
    {
        exploreStatePanel.SetActive(false);
        PlayerTeamLinkUIManager.instance.ActivateTeamLinkUI(false);
    }
}

