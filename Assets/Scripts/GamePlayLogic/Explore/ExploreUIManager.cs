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
        BattleUIManager.instance.OnBattleUIFinish += ()=> exploreStatePanel.SetActive(false);
        MapDeploymentManager.instance.onStartDeployment += ()=> exploreStatePanel.SetActive(false);
        exploreStatePanel.SetActive(true);
    }
}

