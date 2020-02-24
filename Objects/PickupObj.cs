using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObj : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
			
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject == GameController.instance.player.gameObject){
			Pickup();
		}
	}

	public virtual void Pickup(){
		Destroy(this.gameObject);
	}
}
