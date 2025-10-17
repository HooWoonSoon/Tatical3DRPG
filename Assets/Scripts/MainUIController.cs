using System;
using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public GameObject exploreStatePanel;

    public static MainUIController instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BattleUIManager.instance.OnBattleUIFinish += ()=> exploreStatePanel.SetActive(false);
        MapDeploymentManager.instance.onDeploymentTrigger += ()=> exploreStatePanel.SetActive(false);
        exploreStatePanel.SetActive(true);
    }
}

