using System;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public float duration = 2.0f;
    public Action OnBattleUIFinish;

    public void PlayBattleUI()
    {
        Debug.Log("PlayBattleUI...");
        Invoke("UIFinish", duration);
    }

    public void UIFinish()
    {
        Debug.Log("UIFinish");
        OnBattleUIFinish?.Invoke();
    }
}
