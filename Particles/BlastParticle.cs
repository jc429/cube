using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastParticle : MonoBehaviour
{
	public ParticleSystem particles;
	
	public void SetDirection(Direction d){
		Vector3 rot = new Vector3(0, 0, d.DegreesOfRotation());
		transform.rotation = Quaternion.Euler(rot);
	}
	
	public void Play(){
		particles.Play();
	}

	public void Stop(){
		particles.Stop();
	}


}
