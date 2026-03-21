using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Required for SceneManager

public class MenuUI : MonoBehaviour 
{
	public void PlayButton ()
	{
		SceneManager.LoadScene(1); // Updated from Application.LoadLevel
	}

	public void QuitButton ()
	{
		Application.Quit();
	}
}