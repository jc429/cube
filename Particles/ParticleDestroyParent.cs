using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyParent : MonoBehaviour
{
	public void OnParticleSystemStopped(){
		Destroy(transform.parent.gameObject);
	}
}
