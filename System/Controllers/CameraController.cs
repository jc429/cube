using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
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
			
	}

	public void Shake(int strength, Direction dir){
		
	}
}
