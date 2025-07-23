using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPanelController : MonoBehaviour
{
    public GameObject characterPanel; // G�r panelen publik och tilldela via inspektorn

    public Image characterImage;
    public Image headSlot;
    public Image bodySlot;
    public Image legSlot;
    public Image feetSlot;
    public Image handLeftSlot;
    public Image handRightSlot;
    public Image noseSlot;
    public Image bodyModSlot;

    public Slider strengthSlider;
    public Slider agilitySlider;
    public Slider intelligenceSlider;

    private Dictionary<string, Sprite> gearSprites; // En dictionary f�r att lagra utrustningsbilder

    void Start()
    {
        // Initiera dictionary f�r utrustningsbilder
        gearSprites = new Dictionary<string, Sprite>();

        // Exempel p� hur man laddar in en sprite
        // gearSprites["head"] = Resources.Load<Sprite>("Path/To/HeadSprite");

        // Kontrollera referensen till karakt�rspanelen
        if (characterPanel == null)
        {
            Debug.LogError("Character panel not assigned!");
            return;
        }
        characterPanel.SetActive(true); // Se till att panelen �r dold fr�n b�rjan
        Debug.Log("Character panel is set to inactive at start. Active state: " + characterPanel.activeSelf);
    }

    void Update()
    {
        //Debug.Log("Update method is being called."); // Kontrollera att Update metoden k�rs

        // Lyssna efter P-tangenten f�r att v�xla karakt�rspanelen
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P knappen trycks ner."); // Kontrollera att P-tangenten registreras
            characterPanel.SetActive(!characterPanel.activeSelf);
            Debug.Log("Character panel active state after toggle: " + characterPanel.activeSelf);
        }
    }

    public void EquipItem(string slot, Sprite itemSprite)
    {
        switch (slot)
        {
            case "head":
                headSlot.sprite = itemSprite;
                break;
            case "body":
                bodySlot.sprite = itemSprite;
                break;
            case "leg":
                legSlot.sprite = itemSprite;
                break;
            case "feet":
                feetSlot.sprite = itemSprite;
                break;
            case "handleft":
                handLeftSlot.sprite = itemSprite;
                break;
            case "handright":
                handRightSlot.sprite = itemSprite;
                break;
            case "nose":
                noseSlot.sprite = itemSprite;
                break;
            case "bodymod":
                bodyModSlot.sprite = itemSprite;
                break;
        }
    }

    public void UpdateSkill(string skill, float value)
    {
        switch (skill)
        {
            case "strength":
                strengthSlider.value = value;
                break;
            case "agility":
                agilitySlider.value = value;
                break;
            case "intelligence":
                intelligenceSlider.value = value;
                break;
        }
    }
}
