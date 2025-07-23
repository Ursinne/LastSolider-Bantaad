using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // ladda in nästföljande scen
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(1); // Ladda Level 1))
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Ladda om nuvarande Level/Scen
    }
}
