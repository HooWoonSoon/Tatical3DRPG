using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public GameObject mainUIPanel;

    private bool isDeploymentPhase = false;

    private void Start()
    {
        GameEvent.onStartDeployment += () => isDeploymentPhase = true;
        GameEvent.onEndDeployment += () => isDeploymentPhase = false;
    }

    private void Update()
    {
        if (isDeploymentPhase) { return; }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            mainUIPanel.SetActive(!mainUIPanel.activeSelf);
        }
    }
}
