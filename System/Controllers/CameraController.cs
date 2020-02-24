using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform cameraRig; 
	bool ShakeDisabled;			//TODO: turn into game setting

	bool isShaking;
	Timer shakeTimer = new Timer();
	Direction shakeDir;

	float shakeAmplitude;
	float shakeSpeed;
    
	void Awake(){
		if(GameController.cameraController == null) {
			GameController.cameraController = this;
		}
		else if(GameController.cameraController != this) {
			Destroy(this.gameObject);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if(isShaking && !ShakeDisabled){
			shakeTimer.AdvanceTimer(Time.deltaTime);
			float f = shakeAmplitude * Mathf.Sin(shakeSpeed * shakeTimer.CompletionPercentage);
			f *= (1f - shakeTimer.CompletionPercentage);
			Vector3 v = shakeDir.ToVector3() * f;
			cameraRig.transform.localPosition = v;
			if(shakeTimer.IsFinished){
				cameraRig.transform.localPosition = Vector3.zero;
				isShaking = false;
				shakeTimer.Reset();
			}
		}
	}

	public void Shake(Direction dir, int strength, float speed, float duration){
		if(ShakeDisabled){
			return;
		}
		shakeAmplitude = 0.05f * (float)strength;
		shakeSpeed = speed;
		shakeTimer.SetDuration(duration);
		shakeDir = dir;
		isShaking = true;
	}	

	public void StepShake(Direction dir){
		Shake(dir, 3, 15, 0.1f);
	}
	
	public void CrashShake(Direction dir){
		Shake(dir, 6, 15, 0.15f);
	}

}
