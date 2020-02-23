using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
	public GameObject dustParticles;
	public GameObject dustParticlesL;
	
	void Awake(){
		if(GameController.particleController == null) {
			GameController.particleController = this;
		}
		else if(GameController.particleController != this) {
			Destroy(this.gameObject);
		}
	}

	void SpawnParticles(GameObject particles, Vector3 position, Direction direction){
		Quaternion rotation = new Quaternion();
		switch(direction){
			case Direction.N:
				rotation = Quaternion.Euler(0, 0, 0);
				break;
			case Direction.E:
				rotation = Quaternion.Euler(0, 0, -90);
				break;
			case Direction.S:
				rotation = Quaternion.Euler(0, 0, 180);
				break;
			case Direction.W:
				rotation = Quaternion.Euler(0, 0, 90);
				break;
		} 
		GameObject particle = Instantiate(particles, position, rotation);
	}

	public void SpawnDustParticles(Vector3 position, Direction direction){
		SpawnParticles(dustParticles, position, direction);
	}

	public void SpawnDustParticlesLight(Vector3 position, Direction direction){
		SpawnParticles(dustParticlesL, position, direction);
	}

	
}
