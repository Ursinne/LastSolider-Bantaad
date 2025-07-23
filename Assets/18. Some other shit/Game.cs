using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public enum difficultyLevel
    {
        Easy, Medium, Hard
    }

    public enum gender
    {
        Male, Female, Doll
    }

    public enum race
    {
        Gnom, Retard, Rabbit
    }

    public enum style
    {
        Warrior, Garderner, Witch
    }

    public enum rate
    {
        five, ten , Ugly
    }

    private int scenindex = 1; // hamna på nästa vald scen, använd switch SceneManager.LoadScene(scenindex);

    public difficultyLevel LevelSelector; // Här sparas resultatet av dom val du gör
    public gender Gender;
    public race Race;
    public style Style;
    public rate Rate;

    void Start()
    {
        
    }

    void Update()
    {
        switch (LevelSelector)
        {
            case difficultyLevel.Easy:
                //Debug.Log("Easy Level ");
                SceneManager.LoadScene(scenindex);
                break;
            case difficultyLevel.Medium:
                //Debug.Log("Medium Level ");
                break;
            case difficultyLevel.Hard:
                //Debug.Log("Hard Level ");
                break;
        }
        switch (Gender)
        {
            case gender.Male:
                //Debug.Log("Male ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case gender.Female:
                //Debug.Log("Female ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case gender.Doll:
                //Debug.Log("Doll ");
                //SceneManager.LoadScene(TitelScen);
                break;
        }
        switch (Race)
        {
            case race.Gnom:
                //Debug.Log("Choosen Gnome ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case race.Rabbit:
                //Debug.Log("Choosen Reabbit ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case race.Retard:
                //Debug.Log("Choosen Retard ");
                //SceneManager.LoadScene(TitelScen);
                break;
        }
        switch (Style)
        {
            case style.Warrior:
                //Debug.Log("Choosen Warrior ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case style.Garderner:
                //Debug.Log("Choosen garderner ");
                //SceneManager.LoadScene(TitelScen);
                break;

            case style.Witch:
                //Debug.Log("Choosen Witch ");
                //SceneManager.LoadScene(TitelScen);
                break;
        }
    }
}
