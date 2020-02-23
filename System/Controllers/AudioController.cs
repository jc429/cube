using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
	AudioSource _audioSource {
		get{ return GetComponent<AudioSource>(); }
	}

	public AudioClip thudSFX; 

	// Start is called before the first frame update
	void Awake()
	{
		if(GameController.audioController == null) {
			GameController.audioController = this;
		}
		else if(GameController.audioController != this) {
			Destroy(this.gameObject);
		}
	}

	public void PlayThud(){
		_audioSource.PlayOneShot(thudSFX);
	}
}
