using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxArm : MonoBehaviour
{
	public Transform model;
	public PlayerController playerCube;
	public Direction armFacing;

	static Vector3 armPosRetracted = Vector3.zero;
	static Vector3 armPosExtended = Vector3.up;
	static float lenRetracted = 0;
	static float lenExtended = 1;

	public static float extendDuration = 0.25f;
	Timer extendTimer = new Timer();

	bool isExtending = false;
	bool isRetracting = false;
	float curExtLen;

	bool isExtended;
	public bool IsExtended{
		get{ return isExtended; }
	}

	public bool IsMoving{
		get{
			return extendTimer.IsActive;
		}
	}
	public bool moveBody;

	// Start is called before the first frame update
	void Start()
	{
		extendTimer.SetDuration(extendDuration);
		extendTimer.SetActive(false);
		SetExtensionLength(0);
	}

	// Update is called once per frame
	void Update()
	{
		if(extendTimer.IsActive){
			extendTimer.AdvanceTimer(Time.deltaTime);
			if(isExtending){
				curExtLen = Mathf.Lerp(lenRetracted, lenExtended, extendTimer.CompletionPercentage);
				SetExtensionLength(curExtLen);
				if(extendTimer.IsFinished){
					isExtended = true;
					isExtending = false;
					moveBody = false;
					extendTimer.Reset();
					extendTimer.SetActive(false);
				}
			}
			else if(isRetracting){
				curExtLen = Mathf.Lerp(lenExtended, lenRetracted, extendTimer.CompletionPercentage);
				SetExtensionLength(curExtLen);
				if(extendTimer.IsFinished){
					isExtended = false;
					isRetracting = false;
					moveBody = false;
					extendTimer.Reset();
					extendTimer.SetActive(false);
				}
			}
		}
	}


	public void Extend(){
		if(curExtLen == lenExtended){
			return;
		}
		//SetExtensionLength(1);
		isExtending = true;
		extendTimer.Activate();
	}

	public void Retract(){
		if(curExtLen == lenRetracted){
			return;
		}
		//SetExtensionLength(0);
		isRetracting = true;
		extendTimer.Activate();
	}

	void SetPosition(Vector3 pos){
		if(moveBody){
			Vector3 cubePos = playerCube.transform.position;
			float moveDiff = model.localPosition.y - pos.y;
			Vector3 v = new Vector3();
			switch(playerCube.FloorDir){
				case Direction.N:
					v.y = 1*moveDiff;
					break;
				case Direction.E:
					v.x = 1*moveDiff;
					break;
				case Direction.S:
					v.y = -1*moveDiff;
					break;
				case Direction.W:
					v.x = -1*moveDiff;
					break;
			} 
			playerCube.transform.position = cubePos + v;
		}
		model.localPosition = pos;
	}

	void SetExtensionLength(float len){
		len = Mathf.Clamp(len, 0, 1);
		SetPosition(new Vector3(0,len,0));
	}
}
