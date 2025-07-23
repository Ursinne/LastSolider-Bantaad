using UnityEngine;
using System.Collections.Generic;

namespace InventoryAndCrafting
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private NotificationUI notificationPrefab;
        [SerializeField] private Transform notificationContainer;

        [Header("Notification Settings")]
        [SerializeField] private int maxSimultaneousNotifications = 5;
        [SerializeField] private float verticalSpacing = 60f; // Avstånd mellan notifikationer
        [SerializeField] private float notificationLifetime = 3f; // Hur länge varje notifikation visas

        private List<NotificationUI> activeNotifications = new List<NotificationUI>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Hitta Canvas RectTransform
            if (notificationContainer == null)
            {
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    notificationContainer = mainCanvas.transform;
                }
            }
        }

        public void ShowItemNotification(ItemData item, int amount)
        {
            if (item == null) return;

            string message = amount > 1
                ? $"Plockade upp {amount}x {item.itemName}"
                : $"Plockade upp {item.itemName}";

            ShowNotification(item.itemIcon, message);
        }

        public void ShowNotification(Sprite icon, string message)
        {
            // Ta bort gamla notifikationer om de överskrider maxgränsen
            while (activeNotifications.Count >= maxSimultaneousNotifications)
            {
                var oldNotification = activeNotifications[0];
                activeNotifications.RemoveAt(0);
                if (oldNotification != null)
                {
                    Destroy(oldNotification.gameObject);
                }
            }

            // Skapa ny notifikation
            NotificationUI notification = Instantiate(notificationPrefab, notificationContainer);

            // Beräkna position
            Vector2 notificationPosition = CalculateNotificationPosition(activeNotifications.Count);

            RectTransform rectTransform = notification.GetComponent<RectTransform>();

            // Konfigurera ankarpunkter för korrekt positionering
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = notificationPosition;

            activeNotifications.Add(notification);

            // Visa notifikationen
            notification.ShowNotification(icon, message, notificationLifetime);

            // Starta en koroutine för att ta bort notifikationen efter en viss tid
            StartCoroutine(RemoveNotificationAfterDelay(notification));
        }

        private Vector2 CalculateNotificationPosition(int notificationIndex)
        {
            // Placera notifikationer under varandra från nedre högra hörnet
            return new Vector2(-20, 20 + (notificationIndex * verticalSpacing));
        }

        private System.Collections.IEnumerator RemoveNotificationAfterDelay(NotificationUI notification)
        {
            yield return new WaitForSeconds(notificationLifetime);

            if (notification != null)
            {
                activeNotifications.Remove(notification);
                Destroy(notification.gameObject);
            }
        }

        // Metod för manuell testning av positionering
        [ContextMenu("Test Notification Position")]
        public void TestNotificationPosition()
        {
            ShowNotification(null, "Positions Test Notification 1");
            ShowNotification(null, "Positions Test Notification 2");
            ShowNotification(null, "Positions Test Notification 3");
        }
    }
}