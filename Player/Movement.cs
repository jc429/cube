using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}

	int gMask = Layers.GetSolidsMask(true);

	bool isStepping;
	bool isDashing;
	public bool IsMoving{
		get { return isStepping || isDashing; }
	}
	const float moveDuration = 0.16f;
	Timer moveTimer;
	Vector3 startPos; 
	Vector3 endPos;
	Quaternion startRotation;
	Quaternion endRotation;	
	Spin currentSpin;				// direction we are currently (or previously) rotating
	Direction curMoveDir;		// direction we are currently (or previously) moving


	bool[] floorsTouching = {false, false, false, false};
	Direction floorDir;


	// Start is called before the first frame update
	void Awake() {
		moveTimer = new Timer();
	}

	// Update is called once per frame
	void Update()
	{
		if(IsMoving){
			if(isStepping){
				moveTimer.AdvanceTimer(Time.deltaTime);
				transform.position = Vector3.Lerp(startPos, endPos, moveTimer.CompletionPercentage) 
					+  (floorDir.ToVector3() * -0.2f * Mathf.Sin(Mathf.PI * moveTimer.CompletionPercentage));
				transform.rotation = Quaternion.Slerp(startRotation, endRotation, moveTimer.CompletionPercentage);
				if(moveTimer.IsFinished){
					transform.position = endPos;
					if(CheckFollowThrough()){
						isStepping = false;
						moveTimer.Reset();
					}
				}
			}
		}
		else {
			AlignToGrid();
			CheckFloor();
			AttemptMovement();
		}
	}

	void AlignToGrid(){
		Vector3 pos = transform.position;
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		pos.z = Mathf.Round(pos.z);
		transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime);
	}

	void CheckFloor(){
		Direction d;
		int i;
		for (d = Direction.N, i = 0; d <= Direction.W; d++, i++) {
			floorsTouching[i] = CastRayStack(transform.position, d, 0.7f, gMask);
		}

	}

	void AttemptMovement(){
		if(VirtualController.JumpButtonPressed()){
			Launch(floorDir.Opposite());
		}
		else{
			if(VirtualController.LeftDPadPressed(true)
			&& !VirtualController.RightDPadPressed(true)){
				if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
					floorDir = Direction.S;
					Move(Direction.W, Spin.CCW);
				}
				else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
					floorDir = Direction.N;
					Move(Direction.W, Spin.CW);
				}
			}
			if(VirtualController.RightDPadPressed(true)
			&& !VirtualController.LeftDPadPressed(true)){
				if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
					floorDir = Direction.S;
					Move(Direction.E, Spin.CW);
				}
				else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
					floorDir = Direction.N;
					Move(Direction.E, Spin.CCW);
				}
			}
			if(VirtualController.UpDPadPressed(true)
			&& !VirtualController.DownDPadPressed(true)){
				if(floorsTouching[(int)Direction.W] && !floorsTouching[(int)Direction.E]){
					floorDir = Direction.W;
					Move(Direction.N, Spin.CCW);
				}
				else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
					floorDir = Direction.E;
					Move(Direction.N, Spin.CW);
				}
			}
			if(VirtualController.DownDPadPressed(true)
			&& !VirtualController.UpDPadPressed(true)){
				if(floorsTouching[(int)Direction.W] && !floorsTouching[(int)Direction.E]){
					floorDir = Direction.W;
					Move(Direction.S, Spin.CW);
				}
				else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
					floorDir = Direction.E;
					Move(Direction.S, Spin.CCW);
				}
			}
		}
	}

	void Move(Direction d, Spin s){
		ClearMovementParameters();
		startPos = transform.position;
		Vector3 dir = d.ToVector3();
		bool rayHit = CastRayStack(startPos, dir, 1, gMask);
		if(!rayHit){	//if the space forward is free
			endPos = startPos + dir;
			moveTimer.SetDuration(moveDuration);
			currentSpin = s;
			curMoveDir = d;
			startRotation = transform.rotation;
			if(s == Spin.CW){
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
			}
			else if(s == Spin.CCW){
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
			}
			isStepping = true;
		}
		else{
			Direction d2 = (s == Spin.CW) ? d.Previous() : d.Next();
			dir = d2.ToVector3();
			rayHit = CastRayStack(startPos, dir, 1, gMask);
			if(!rayHit){
				floorDir = d;
				endPos = startPos + dir;
				moveTimer.SetDuration(moveDuration);
				currentSpin = s;
				curMoveDir = d2;
				startRotation = transform.rotation;
				if(s == Spin.CW){
					endRotation = startRotation * Quaternion.Euler(0, 0, -90);
				}
				else if(s == Spin.CCW){
					endRotation = startRotation * Quaternion.Euler(0, 0, 90);
				}
				isStepping = true;
			}
		}
	}

	bool CheckFollowThrough(){
		bool doneMoving = false;
		bool rayHit = CastRayStack(transform.position, curMoveDir, 1, gMask);
		if(rayHit){
			doneMoving = true;
			floorDir = curMoveDir;
			return doneMoving;
		}

		Vector3 dir = floorDir.ToVector3();
		rayHit = CastRayStack(transform.position, dir, 1, gMask);
		if(!rayHit){		//no ground below us
			startPos = transform.position;
			endPos = startPos + dir;
			curMoveDir = floorDir;
			moveTimer.Reset();
			moveTimer.SetDuration(moveDuration);
			startRotation = transform.rotation;
			if(currentSpin == Spin.CW){
				floorDir = floorDir.Next();
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
			}
			else if(currentSpin == Spin.CCW){
				floorDir = floorDir.Previous();
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
			}
			isStepping = true;
		}
		else{
			doneMoving = true;
		}
		return doneMoving;
	}

	void Launch(Direction dir){
		ClearMovementParameters();
		float launchForce = 20f;
		_rigidbody.AddForce(launchForce * dir.ToVector3(), ForceMode.Impulse);
		isDashing = true;
	}

	void ClearMovementParameters(){
		startPos = transform.position;
		endPos = transform.position;
		startRotation = transform.rotation;
		endRotation = transform.rotation;
		moveTimer.SetActive(false);
	}


	void ActivateMagNodes(){

	}
	
	void MagPush(){

	}

	void MagPull(){


	}

	void MagRotateCCW(){

	}

	void MagRotateCW(){

	}


	bool CastRayStack(Vector3 origin, Vector3 dir, float len, int mask){
		const int numRays = 5;
		if(len == 0){
			return false;
		}
		RaycastHit r;
		float[] distlist = new float[numRays];

		for(int i = 0; i < numRays; i++){
			float height = -0.45f + (0.225f * i);
			Physics.Raycast(origin + new Vector3(height * dir.y, height * dir.x, 0), dir, out r, len, mask);
			
			Debug.DrawRay(origin + new Vector3(height * dir.y, height * dir.x, 0), len * dir, ((r.distance == 0) ? Color.white : Color.cyan));
			distlist[i] = r.distance;
		}
		
		float shortest = float.MaxValue;
		foreach (float f in distlist) {
			if (f > 0){
				shortest = Mathf.Min(shortest, f);
			}
		}
		return (shortest > 0 && shortest < float.MaxValue) ? true : false;
	}

	bool CastRayStack(Vector3 origin, Direction dir, float len, int mask){
		return CastRayStack(origin, dir.ToVector3(), len, mask);
	}

}
