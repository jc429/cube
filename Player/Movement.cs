using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveState{
	Idle,
	Stepping,
	Dashing
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ArmManager))]
public class Movement : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}
	PlayerController _controller{
		get{ return GetComponent<PlayerController>(); }
	}
	ArmManager _armManager{
		get{ return GetComponent<ArmManager>(); }
	}

	public GameObject model;


	[Header("Movement")]
	MoveState moveState;
	public bool IsMoving{
		get { return moveState == MoveState.Stepping || moveState == MoveState.Dashing; }
	}
	Direction curMoveDir;		// direction we are currently (or previously) moving
	public Direction CurrentMoveDirection{
		get{ return curMoveDir; }
	}
	Spin currentSpin;				// direction we are currently (or previously) rotating
	public Spin CurrentSpinDirection{
		get{ return currentSpin; }
	}
	// stepping
	const float stepDuration = 0.16f;
	const float stepEndDuration = 0.08f;	//time to wait after a step (to add weight to movement)
	Timer stepTimer;
	bool endStep;
	Vector3 startPos; 
	Vector3 endPos;
	Quaternion startRotation;
	Quaternion endRotation;	
	// dashing
	float dashSpeed = 20f; 

	bool hasStepped;



	void Awake() {
		stepTimer = new Timer();
	}

	public void UpdateMovement(){
		switch(moveState){
			case MoveState.Stepping:
			{
				stepTimer.AdvanceTimer(Time.deltaTime);
				if(!endStep){
					transform.position = Vector3.Lerp(startPos, endPos, stepTimer.CompletionPercentage) 
					+  (_controller.FloorDir.ToVector3() * -0.2f * Mathf.Sin(Mathf.PI * stepTimer.CompletionPercentage));
					model.transform.rotation = Quaternion.Slerp(startRotation, endRotation, stepTimer.CompletionPercentage);
				}				
				if(stepTimer.IsFinished){
					transform.position = endPos;
					if(CheckFollowThrough()){
						endStep = false;
						stepTimer.Reset();
					}
					else{
						if(!endStep){
							GameController.particleController.SpawnDustParticlesLight(transform.position, _controller.FloorDir.Opposite());
							GameController.audioController.PlayThud();
							GameController.cameraController.StepShake(_controller.FloorDir);
							endStep = true;
							stepTimer.Reset();
							stepTimer.SetDuration(stepEndDuration);
						}
						else{
							endStep = false;
							moveState = MoveState.Idle;
							stepTimer.Reset();
							if(_controller.MovementInputSustained()){
								Move(curMoveDir, currentSpin);
							}
						}
					}					
				}
			}
			break;
			case  MoveState.Dashing:
			{
				_rigidbody.AddForce(dashSpeed * curMoveDir.ToVector3() * Time.deltaTime, ForceMode.VelocityChange);
				float hitDist;
				bool rayHit = CastRayStack(transform.position, curMoveDir, 0.8f, out hitDist, Layers.GroundMask);
				if(rayHit){
					transform.position += curMoveDir.ToVector3() * (hitDist - 0.5f);
					_controller.EndDash();
				}	
			}
			break;
		}
	}


	public void Move(Direction d, Spin s){
		ClearMovementParameters();
		startPos = transform.position;
		Vector3 dir = d.ToVector3();
		bool rayHit = CastRayStack(startPos, dir, 1, Layers.GroundMask);
		if(!rayHit){	//if the space forward is free
			endPos = startPos + dir;
			stepTimer.SetDuration(stepDuration);
			currentSpin = s;
			curMoveDir = d;
			startRotation = model.transform.rotation;
			if(s == Spin.CW){
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
				_controller.Orientation = _controller.Orientation.NextCW();
			}
			else if(s == Spin.CCW){
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
				_controller.Orientation = _controller.Orientation.NextCCW();
			}
			moveState = MoveState.Stepping;
		}
		else{
			Direction d2 = (s == Spin.CW) ? d.NextCCW() : d.NextCW();
			dir = d2.ToVector3();
			rayHit = CastRayStack(startPos, dir, 1, Layers.GroundMask);
			if(!rayHit){
				_controller.SetFloorDir(d);
				endPos = startPos + dir;
				stepTimer.SetDuration(stepDuration);
				currentSpin = s;
				curMoveDir = d2;
				startRotation = model.transform.rotation;
				if(s == Spin.CW){
					endRotation = startRotation * Quaternion.Euler(0, 0, -90);
					_controller.Orientation = _controller.Orientation.NextCW();
				}
				else if(s == Spin.CCW){
					endRotation = startRotation * Quaternion.Euler(0, 0, 90);
					_controller.Orientation = _controller.Orientation.NextCCW();
				}
				moveState = MoveState.Stepping;
			}
		}
	}

	bool CheckFollowThrough(){
		bool doneMoving = false;
		bool rayHit = CastRayStack(transform.position, curMoveDir, 1, Layers.GroundMask);
		if(rayHit){
			doneMoving = true;
			//floorDir = curMoveDir;
			return !doneMoving;
		}

		Vector3 dir = _controller.FloorDir.ToVector3();
		rayHit = CastRayStack(transform.position, dir, 1, Layers.GroundMask);
		if(!rayHit){		//no ground below us
			startPos = transform.position;
			endPos = startPos + dir;
			curMoveDir = _controller.FloorDir;
			stepTimer.Reset();
			stepTimer.SetDuration(stepDuration);
			startRotation = model.transform.rotation;
			if(currentSpin == Spin.CW){
				_controller.SetFloorDir(_controller.FloorDir.NextCW());
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
				_controller.Orientation = _controller.Orientation.NextCW();
			}
			else if(currentSpin == Spin.CCW){
				_controller.SetFloorDir(_controller.FloorDir.NextCCW());
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
				_controller.Orientation = _controller.Orientation.NextCCW();
			}
			moveState = MoveState.Stepping;
		}
		else{
			doneMoving = true;
		}
		return !doneMoving;
	}

	void EndStep(){

	}


	public bool AttemptDashMovement(Direction dir){
		ClearMovementParameters();
		bool rayHit = CastRayStack(transform.position, dir, 1, Layers.GroundMask);
		if(rayHit){
			return false;
		}
		_rigidbody.AddForce(dashSpeed * dir.ToVector3(), ForceMode.Impulse);
		moveState = MoveState.Dashing;
		curMoveDir = dir;
	
		return true;
	}

	public void EndDashMovement(){
		_rigidbody.velocity = Vector3.zero;
		moveState = MoveState.Idle;
		AlignToGrid();
	}

	void ClearMovementParameters(){
		startPos = transform.position;
		endPos = transform.position;
		startRotation = model.transform.rotation;
		endRotation = model.transform.rotation;
		stepTimer.SetActive(false);
	}

	public void AlignToGrid(){
		Vector3 pos = transform.position;
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		pos.z = Mathf.Round(pos.z);
		transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime);
	}

	public bool CastRayStack(Vector3 origin, Vector3 dir, float len, int mask){
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

	public bool CastRayStack(Vector3 origin, Direction dir, float len, int mask){
		return CastRayStack(origin, dir.ToVector3(), len, mask);
	}

	public bool CastRayStack(Vector3 origin, Vector3 dir, float len, out float hitLen, int mask){
		const int numRays = 5;
		if(len == 0){
			hitLen = 0;
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
		hitLen = shortest;
		return (shortest > 0 && shortest < float.MaxValue) ? true : false;
	}

	public bool CastRayStack(Vector3 origin, Direction dir, float len, out float hitLen, int mask){
		return CastRayStack(origin, dir.ToVector3(), len, out hitLen, mask);
	}

}
