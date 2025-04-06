using UnityEngine;
using UnityEngine.UI;

public class TeamLinkButton : MonoBehaviour
{
    public Button linkButton;
    public Button unlinkButton;
    public Button detailButton;

    public void Initialize(int index)
    {
        //linkButton.onClick.AddListener(() => OnClickLinkButton(index));
        unlinkButton.onClick.AddListener(() => OnClickUnlinkButton(index));
        detailButton.onClick.AddListener(() => OnClickDetailButton(index));
    }

    public void OnClickLinkButton(int index)
    {
        Debug.Log("Link button clicked with index: " + index);
    }
   
    public void OnClickUnlinkButton(int index)
    {
        //Debug.Log("Unlink button clicked with index: " + index);
    }

    public void OnClickDetailButton(int index)
    {
        //Debug.Log("Detail button clicked with index: " + index);
    }
}
