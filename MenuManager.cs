using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadMainMenu()
{
    // Make sure "MainMenu" matches the exact name of your Menu scene file
    SceneManager.LoadScene("Menu"); 
}
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