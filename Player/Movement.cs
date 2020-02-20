using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}

	bool isMoving;
	public bool IsMoving{
		get { return isMoving; }
	}

	Vector3 startPos; 
	Vector3 endPos;
	Quaternion startRotation;
	Quaternion endRotation;
	Timer moveTimer;
	const float moveDuration = 0.6f;

	int gMask = Layers.GetSolidsMask(true);
	bool[] floorsTouching = {false, false, false, false};
	Vector3 floorDir;


	// Start is called before the first frame update
	void Awake() {
		moveTimer = new Timer();
	}

	// Update is called once per frame
	void Update()
	{
		if(isMoving){
			moveTimer.AdvanceTimer(Time.deltaTime);
			transform.position = Vector3.Lerp(startPos, endPos, moveTimer.CompletionPercentage) 
				+  (floorDir * -0.2f * Mathf.Sin(Mathf.PI * moveTimer.CompletionPercentage));
			transform.rotation = Quaternion.Slerp(startRotation, endRotation, moveTimer.CompletionPercentage);
			if(moveTimer.IsFinished){
				transform.position = endPos;
				isMoving = false;
				moveTimer.Reset();
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
		if(VirtualController.LeftDPadPressed(true)
		&& !VirtualController.RightDPadPressed(true)){
			if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
				MoveCCW(Direction.W);
			}
			else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
				MoveCW(Direction.W);
			}
		}
		if(VirtualController.RightDPadPressed(true)
		&& !VirtualController.LeftDPadPressed(true)){
			if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
				MoveCW(Direction.E);
			}
			else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
				MoveCCW(Direction.E);
			}
		}

	}

	// moves the cube one tile left
	void MoveCCW(Direction d){
		Vector3 dir = d.ToVector3();
		startPos = transform.position;
		startRotation = transform.rotation;
		endRotation = startRotation * Quaternion.Euler(0, 0, 90);
		bool rayHit = CastRayStack(startPos, dir, 1, gMask);
		if(!rayHit){
			floorDir = Vector3.down;
			endPos = startPos + dir;
			moveTimer.SetDuration(moveDuration);
			isMoving = true;
		}
		else{
			// if we hit a wall maybe we can climb up it
			dir = Vector3.up;
			rayHit = CastRayStack(startPos, dir, 1, gMask);
			if(!rayHit){
				floorDir = Vector3.left;
				endPos = startPos + dir;
				moveTimer.SetDuration(moveDuration);
				isMoving = true;
			}
		}
	}

	void MoveCW(Direction d){
		Vector3 dir = d.ToVector3();
		startPos = transform.position;
		bool rayHit = CastRayStack(startPos, dir, 1, gMask);
		startRotation = transform.rotation;
		endRotation = startRotation * Quaternion.Euler(0, 0, -90);
		if(!rayHit){
			floorDir = Vector3.down;
			endPos = startPos + dir;
			moveTimer.SetDuration(moveDuration);
			isMoving = true;
		}
		else{
			// if we hit a wall maybe we can climb up it
			dir = Vector3.up;
			rayHit = CastRayStack(startPos, dir, 1, gMask);
			if(!rayHit){
				floorDir = Vector3.right;
				endPos = startPos + dir;
				moveTimer.SetDuration(moveDuration);
				isMoving = true;
			}
		}
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
