using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
	public void PlayGame()
	{
        SceneManager.LoadScene("SelectionScene");
    }

	public static float volume = 1;
    public void UpdateVolume(UnityEngine.UI.Slider volumeSlider) {
        volume = volumeSlider.value;
    }

	public void Quit()
	{
		Application.Quit();
	}
}
