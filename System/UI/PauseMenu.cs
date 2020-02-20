using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {
	public GameObject pMenu;
	AudioSource _audio;
	bool open;
	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
		GameController.instance.pauseMenu = this;
		pMenu.SetActive(false);
		open = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Toggle() {
		_audio.Play();
		open = !open;
		pMenu.SetActive(open);
		//GameController.FreezeGame(open);
	}

	public void OpenPauseMenu() {
		//GameController.FreezeGame(true);
		_audio.Play();
		pMenu.SetActive(true);
		open = true;
	}

	public void ClosePauseMenu() {
		//GameController.FreezeGame(false);
		_audio.Play();
		pMenu.SetActive(false);
		open = false;
	}

	public void QuitLevel() {
		ClosePauseMenu();
		//GameController.instance.GoToMainMenu();
	}

	public void RestartLevel() {
		ClosePauseMenu();
		//GameController.instance.ResetLevel();
	}
}
