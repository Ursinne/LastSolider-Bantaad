using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace InventoryAndCrafting
{
    public class NotificationUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ShowNotification(Sprite icon, string message, float lifetime)
        {
            // Konfigurera UI-komponenter
            if (itemIcon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.enabled = icon != null;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            // Starta animationen
            StartCoroutine(AnimateNotification(lifetime));
        }

        private IEnumerator AnimateNotification(float lifetime)
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeInDuration;
                float alpha = fadeCurve.Evaluate(progress);

                canvasGroup.alpha = alpha;

                yield return null;
            }

            // Visa meddelande under lifetime
            yield return new WaitForSeconds(lifetime);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;
                float alpha = 1f - fadeCurve.Evaluate(progress);

                canvasGroup.alpha = alpha;

                yield return null;
            }

            // Ta bort notifikationen
            Destroy(gameObject);
        }
    }
}