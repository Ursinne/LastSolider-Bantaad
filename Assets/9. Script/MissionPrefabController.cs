using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPrefabController : MonoBehaviour
{
    public TextMeshProUGUI missionTitle;
    public Toggle missionToggle;
    public GameObject rewardPanel;

    public void Initialize(Mission mission)
    {
        Debug.Log("Initializing mission: " + mission.title);

        missionTitle.text = mission.title;
        missionToggle.isOn = mission.isCompleted;
        missionToggle.onValueChanged.AddListener(delegate {
            if (missionToggle.isOn)
            {
                missionTitle.color = Color.gray;
                ShowReward(mission);
            }
            else
            {
                missionTitle.color = Color.white;
            }
        });
    }

    private void ShowReward(Mission mission)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            Debug.Log($"Belöning: {mission.rewardItem} eller {mission.xpReward} XP");
        }
    }
}
