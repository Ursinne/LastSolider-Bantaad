using UnityEngine;

public class MissionPanelController : MonoBehaviour
{
    public GameObject missionPanel;

    private void Start()
    {
        // Se till att missionpanelen �r st�ngd vid start
        missionPanel.SetActive(true);
    }

    private void Update()
    {
        // Lyssna p� M-tangenttryckning
        if (Input.GetKeyDown(KeyCode.M))
        {
            // V�xla visningen av missionpanelen
            missionPanel.SetActive(!missionPanel.activeSelf);
        }
    }
}
