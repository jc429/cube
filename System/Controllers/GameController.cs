using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public static bool DEBUG_MODE = false;
	public static GameController instance; 

	public static AudioController audioController;
	public static CameraController cameraController;
	public static ParticleController particleController;


	public PlayerController player;

	public OptionsMenu optionsMenu;
	public PauseMenu pauseMenu;

	// Start is called before the first frame update
	void Awake(){
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(instance != this) {
			Destroy(this.gameObject);
		}
		Initialize();
	}

	void Initialize(){
		InputController.Initialize();
	}

	void Update(){
		InputController.CaptureInputs();
		if (InputController.PauseButtonPressed()) {
			
			if (pauseMenu != null) {
				if (optionsMenu != null && optionsMenu.open) {
					optionsMenu.CloseOptionsMenu();
				}
				else {
					pauseMenu.Toggle();
				}
			}
		}
		if (InputController.ResetButtonPressed()) {
			//ResetGame();
		}
		if(DEBUG_MODE){
			if(Input.GetKeyDown(KeyCode.RightAlt)){
				string time = System.DateTime.Now.ToString("yyyy'-'MM'-'dd'--'HH'-'mm'-'ss");
				string path = System.IO.Path.Combine(Application.persistentDataPath, "Pictures/screenshot " + time + ".png");
				ScreenCapture.CaptureScreenshot(path);
				Debug.Log("Screen capture saved! " + path);
			}

			if(Input.GetKeyDown(KeyCode.Keypad0)){
				RenderSettings.ToggleFullscreen();
			}
			if(Input.GetKeyDown(KeyCode.Keypad1)){
				RenderSettings.SetRenderScale(1);
			}
			if(Input.GetKeyDown(KeyCode.Keypad2)){
				RenderSettings.SetRenderScale(2);
			}
			if(Input.GetKeyDown(KeyCode.Keypad3)){
				RenderSettings.SetRenderScale(3);
			}
			if(Input.GetKeyDown(KeyCode.Keypad4)){
				RenderSettings.SetRenderScale(4);
			}
		}
	}


}
