using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private bool isInventoryOpen = false;
    private bool isGamePaused = false;

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

    public void SetInventoryState(bool isOpen)
    {
        isInventoryOpen = isOpen;
        UpdateCursorVisibility();
    }

    public void SetPauseState(bool isPaused)
    {
        isGamePaused = isPaused;
        UpdateCursorVisibility();
    }

    private void UpdateCursorVisibility()
    {
        if (isGamePaused || isInventoryOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}