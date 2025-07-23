using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;



public class PausMeny : MonoBehaviour
{
    private bool pausing;
    public GameObject pauseMenu;

    void Start()
    {
        // pauseMenu.active = false; // gammal variant men kan funka
        pauseMenu.SetActive(false);  // sl� av menu vid spelets start
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape)) // get keydowb ifall man bara vill ha ett tryck
        {
            pausing = !pausing; // utrycket v�xlar boolen true/false varje f�ng man treycker p� escape
        }

        Pause();

    }

    private void Pause()
    {
        if (pausing)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            CursorManager.Instance.SetPauseState(true);
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            CursorManager.Instance.SetPauseState(false);
        }
    

    }
    public void Continue()
    {
        pausing = !pausing;     //starta spelet igen
    }

    public void Qiut()
    {
        Application.Quit(); // St�ng Spelet
        Debug.Log("Qiut");
    }
    public void RestarLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Ladda om nuvarande Level/Scen
    }
    
}
