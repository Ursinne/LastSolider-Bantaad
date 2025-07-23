using UnityEngine;
using TMPro;
using System.Collections;

public class LocationDiscoveryUI : MonoBehaviour
{
    public static LocationDiscoveryUI Instance { get; private set; }

    [SerializeField] private GameObject discoveryPanel;
    [SerializeField] private TextMeshProUGUI locationNameText;
    [SerializeField] private TextMeshProUGUI locationDescriptionText;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 1f;

    private Coroutine currentDisplayCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup == null)
            canvasGroup = discoveryPanel.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (discoveryPanel != null)
            discoveryPanel.SetActive(false);
    }

    public void ShowLocationDiscovery(string locationName, string locationDescription = "")
    {
        if (discoveryPanel == null) return;

        // Avbryt tidigare visning om det finns en
        if (currentDisplayCoroutine != null)
            StopCoroutine(currentDisplayCoroutine);

        // Starta ny visningsprocess
        currentDisplayCoroutine = StartCoroutine(DisplayLocationCoroutine(locationName, locationDescription));
    }

    private IEnumerator DisplayLocationCoroutine(string locationName, string locationDescription)
    {
        // Sätt text
        locationNameText.text = locationName;
        locationDescriptionText.text = locationDescription;

        // Aktivera panel och nollställ opacitet
        discoveryPanel.SetActive(true);
        canvasGroup.alpha = 0f;

        // Fade in
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Visa för angiven tid
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }

        // Dölj panel
        discoveryPanel.SetActive(false);
        currentDisplayCoroutine = null;
    }
}