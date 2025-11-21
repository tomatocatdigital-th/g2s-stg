using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void StartGame()
    {
        
        SceneManager.LoadScene("GameMap01");
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {

        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
        
        
    }

    public void Setting()
    {

        SceneManager.LoadScene("Setting");
        Time.timeScale = 1f;
        
    }
}
