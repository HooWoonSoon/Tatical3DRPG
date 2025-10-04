using System;
using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public GameObject exploreStatePanel;
    public GameObject deploymentPanel;

    public static MainUIController instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BattleUIManager.instance.OnBattleUIFinish += ()=> exploreStatePanel.SetActive(false);
        exploreStatePanel.SetActive(true);
        deploymentPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            deploymentPanel.SetActive(!deploymentPanel.activeSelf);
        }
    }
}

