using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxArm : MonoBehaviour
{
	public Transform model;

	static Vector3 armPosRetracted = Vector3.zero;
	static Vector3 armPosExtended = Vector3.up;

	// Start is called before the first frame update
	void Start()
	{
		SetExtensionLength(0);
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	public void Extend(){
		SetExtensionLength(1);
	}

	public void Retract(){
		SetExtensionLength(0);
	}	

	void SetPosition(Vector3 pos){
		model.localPosition = pos;
	}

	void SetExtensionLength(float len){
		len = Mathf.Clamp(len, 0, 1);
		SetPosition(new Vector3(0,len,0));
	}
}
