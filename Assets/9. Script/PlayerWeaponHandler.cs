using UnityEngine;
using InventoryAndCrafting;
using System.Collections.Generic;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private Animator playerAnimator;

    [Header("Active Weapon")]
    [SerializeField] private GameObject currentWeaponObject;
    private ItemData currentWeaponItemData;

    [Header("Weapon States")]
    [SerializeField] private bool isAiming = false;
    [SerializeField] private bool isShooting = false;

    [System.Serializable]
    public class WeaponPrefabMapping
    {
        public ItemData itemData;
        public GameObject prefab;
    }

    [Header("Weapon Prefabs")]
    public List<WeaponPrefabMapping> weaponPrefabMappings = new List<WeaponPrefabMapping>();

    private void Start()
    {
        // S�kerst�ll referenser
        if (equipmentManager == null)
            equipmentManager = EquipmentManager.Instance;

        // Hitta weaponHoldPoint och animator om inte tilldelad
        if (weaponHoldPoint == null)
        {
            weaponHoldPoint = transform.Find("RightHand") ??
                            transform.Find("Hand") ??
                            CreateDefaultWeaponHoldPoint();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();
        }

        // Registrera h�ndelser
        if (equipmentManager != null)
        {
            equipmentManager.onEquipmentChanged += UpdateEquippedWeapon;
        }

        // Initial uppdatering
        UpdateEquippedWeapon();

        // S�kerst�ll att aiming och shooting �r false fr�n start
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isAiming", false);
            playerAnimator.SetBool("isShooting", false);
        }
    }

    private Transform CreateDefaultWeaponHoldPoint()
    {
        GameObject holdPoint = new GameObject("WeaponHoldPoint");
        holdPoint.transform.SetParent(transform);
        holdPoint.transform.localPosition = new Vector3(0.5f, 0, 0.3f);
        return holdPoint.transform;
    }

    private void Update()
    {
        // Hantera siktel�ge - bara n�r man trycker ner h�ger musknapp
        if (currentWeaponObject != null)
        {
            // Sikta
            if (Input.GetMouseButtonDown(1))
            {
                isAiming = true;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isAiming", true);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isAiming = false;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isAiming", false);
                }
            }

            // Skjuta n�r v�nster musknapp trycks ner under siktel�ge
            if (isAiming && Input.GetMouseButtonDown(0))
            {
                // Starta skjutanimation
                isShooting = true;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("isShooting", true);
                }

                // Skjut vapnet
                UseWeapon();

                // �terst�ll skjutl�ge efter en kort stund
                StartCoroutine(ResetShootingState());
            }
        }
    }

    private System.Collections.IEnumerator ResetShootingState()
    {
        // V�nta en kort stund innan skjutl�get �terst�lls
        yield return new WaitForSeconds(0.5f);

        isShooting = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isShooting", false);
        }
    }

    // Resten av koden f�rblir of�r�ndrad fr�n f�reg�ende version
    private void UpdateEquippedWeapon()
    {
        if (equipmentManager == null) return;

        var allSlots = equipmentManager.GetAllSlots();

        EquipmentSlot weaponSlot = null;

        // Hitta r�tt vapneslot (MainHand)
        foreach (var slot in allSlots)
        {
            if (slot == null) continue;

            if (slot.slotType == EquipmentSlotType.MainHand && slot.CurrentItem != null && IsItemAWeapon(slot.CurrentItem))
            {
                weaponSlot = slot;
                break;
            }
        }

        // Hantera vapenbytet
        if (weaponSlot == null || weaponSlot.CurrentItem == null)
        {
            RemoveCurrentWeapon();
            return;
        }

        // Samma vapen �r redan utrustat
        if (currentWeaponItemData == weaponSlot.CurrentItem) return;

        // Ta bort nuvarande vapen och utrusta nytt
        RemoveCurrentWeapon();
        EquipWeapon(weaponSlot.CurrentItem);
    }

    private bool IsItemAWeapon(ItemData item)
    {
        return item != null && (
            item.itemType == ItemType.Weapon ||
            weaponPrefabMappings.Exists(m => m.itemData == item)
        );
    }

    private void EquipWeapon(ItemData weaponItem)
    {
        if (weaponItem == null || weaponHoldPoint == null) return;

        GameObject weaponPrefab = GetWeaponPrefabForItem(weaponItem);

        if (weaponPrefab != null)
        {
            // Instantiera vapen p� r�tt position
            currentWeaponObject = Instantiate(weaponPrefab, weaponHoldPoint);
            currentWeaponItemData = weaponItem;

            // Konfigurera vapensystemet
            WeaponSystem weaponSystem = currentWeaponObject.GetComponent<WeaponSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.Initialize(transform);
            }
            else
            {
                Debug.LogWarning($"WeaponSystem-komponenten saknas p� vapenprefab: {weaponItem.itemName}");
            }

            // S�kerst�ll att aiming och shooting �r av n�r vapnet utrustas
            isAiming = false;
            isShooting = false;
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isAiming", false);
                playerAnimator.SetBool("isShooting", false);
            }
        }
    }

    private GameObject GetWeaponPrefabForItem(ItemData item)
    {
        // S�k i mappningen f�rst
        var mapping = weaponPrefabMappings.Find(m => m.itemData == item);
        if (mapping?.prefab != null)
        {
            return mapping.prefab;
        }

        // Fallback till itemPrefab
        if (item.itemPrefab != null)
        {
            return item.itemPrefab;
        }

        Debug.LogWarning($"Hittade ingen prefab f�r {item.itemName}");
        return null;
    }

    private void RemoveCurrentWeapon()
    {
        if (currentWeaponObject != null)
        {
            // Meddela vapnet att det blir oequipat
            WeaponSystem weaponSystem = currentWeaponObject.GetComponent<WeaponSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.OnUnequipped();
            }

            Destroy(currentWeaponObject);
            currentWeaponObject = null;
            currentWeaponItemData = null;

            // S�kerst�ll att aiming och shooting st�ngs av n�r vapnet tas bort
            isAiming = false;
            isShooting = false;
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isAiming", false);
                playerAnimator.SetBool("isShooting", false);
            }
        }
    }

    public void UseWeapon()
    {
        if (currentWeaponObject == null)
            return;

        WeaponSystem weaponSystem = currentWeaponObject.GetComponent<WeaponSystem>();
        if (weaponSystem != null)
        {
            // Skapa en Ray fr�n kameran genom crosshair (mitten av sk�rmen)
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            Vector3 targetPoint;

            // Om str�len tr�ffar n�got, anv�nd den punkten som m�l
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                // Annars, anv�nd en punkt l�ngt fram�t i kamerans riktning
                targetPoint = ray.origin + ray.direction * 100f;
            }

            // Anropa UseWeapon med m�lpunkten
            //weaponSystem.UseWeapon(targetPoint);
        }
    }
}