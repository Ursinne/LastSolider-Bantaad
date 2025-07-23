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
        // Säkerställ referenser
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

        // Registrera händelser
        if (equipmentManager != null)
        {
            equipmentManager.onEquipmentChanged += UpdateEquippedWeapon;
        }

        // Initial uppdatering
        UpdateEquippedWeapon();

        // Säkerställ att aiming och shooting är false från start
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
        // Hantera sikteläge - bara när man trycker ner höger musknapp
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

            // Skjuta när vänster musknapp trycks ner under sikteläge
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

                // Återställ skjutläge efter en kort stund
                StartCoroutine(ResetShootingState());
            }
        }
    }

    private System.Collections.IEnumerator ResetShootingState()
    {
        // Vänta en kort stund innan skjutläget återställs
        yield return new WaitForSeconds(0.5f);

        isShooting = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isShooting", false);
        }
    }

    // Resten av koden förblir oförändrad från föregående version
    private void UpdateEquippedWeapon()
    {
        if (equipmentManager == null) return;

        var allSlots = equipmentManager.GetAllSlots();

        EquipmentSlot weaponSlot = null;

        // Hitta rätt vapneslot (MainHand)
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

        // Samma vapen är redan utrustat
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
            // Instantiera vapen på rätt position
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
                Debug.LogWarning($"WeaponSystem-komponenten saknas på vapenprefab: {weaponItem.itemName}");
            }

            // Säkerställ att aiming och shooting är av när vapnet utrustas
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
        // Sök i mappningen först
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

        Debug.LogWarning($"Hittade ingen prefab för {item.itemName}");
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

            // Säkerställ att aiming och shooting stängs av när vapnet tas bort
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
            // Skapa en Ray från kameran genom crosshair (mitten av skärmen)
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            Vector3 targetPoint;

            // Om strålen träffar något, använd den punkten som mål
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                // Annars, använd en punkt långt framåt i kamerans riktning
                targetPoint = ray.origin + ray.direction * 100f;
            }

            // Anropa UseWeapon med målpunkten
            //weaponSystem.UseWeapon(targetPoint);
        }
    }
}