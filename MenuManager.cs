using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void LoadInventory()
    {
        SceneManager.LoadScene("Inventory");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}