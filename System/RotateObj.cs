using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj : MonoBehaviour
{

	public Vector3 rotateSpeed;
	
	// Update is called once per frame
	void Update()
	{
		transform.Rotate(rotateSpeed * Time.deltaTime);
	}
}
