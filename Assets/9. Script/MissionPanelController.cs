using UnityEngine;

public class MissionPanelController : MonoBehaviour
{
    public GameObject missionPanel;

    private void Start()
    {
        // Se till att missionpanelen är stängd vid start
        missionPanel.SetActive(true);
    }

    private void Update()
    {
        // Lyssna på M-tangenttryckning
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Växla visningen av missionpanelen
            missionPanel.SetActive(!missionPanel.activeSelf);
        }
    }
}
