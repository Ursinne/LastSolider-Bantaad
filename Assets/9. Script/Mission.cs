using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mission
{
    public string title;
    public string description;
    public bool isCompleted;
    public Dictionary<string, int> requiredItems = new Dictionary<string, int>();
    public Dictionary<string, int> collectedItems = new Dictionary<string, int>();
    public int xpReward;
    public string rewardItem;
    public int goldReward;

    public Mission(string title, string description, Dictionary<string, int> requiredItems, int xpReward, string rewardItem, int goldReward)
    {
        this.title = title;
        this.description = description;
        this.isCompleted = false;
        this.requiredItems = requiredItems;
        this.collectedItems = new Dictionary<string, int>();
        this.xpReward = xpReward;
        this.rewardItem = rewardItem;
        this.goldReward = goldReward;
    }
}
