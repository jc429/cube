using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveState{
	Idle,
	Stepping,
	Dashing
}

public class Movement : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}

	int gMask = Layers.GetSolidsMask(true);

	MoveState moveState;
	public bool IsMoving{
		get { return moveState == MoveState.Stepping || moveState == MoveState.Dashing; }
	}
	Direction curMoveDir;		// direction we are currently (or previously) moving
	Spin currentSpin;				// direction we are currently (or previously) rotating
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

	bool inputHeld;
	bool hasStepped;
	Direction holdDir;

	[SerializeField]
	[NamedArrayAttribute (new string[] {"North", "East", "South", "West"})]
	bool[] floorsTouching = {false, false, false, false};
	Direction floorDir;


	// Start is called before the first frame update
	void Awake() {
		stepTimer = new Timer();
	}

	// Update is called once per frame
	void Update()
	{
		CheckFloor();
		if(IsMoving){
			if(moveState == MoveState.Stepping){
				stepTimer.AdvanceTimer(Time.deltaTime);
				if(!endStep){
					transform.position = Vector3.Lerp(startPos, endPos, stepTimer.CompletionPercentage) 
					+  (floorDir.ToVector3() * -0.2f * Mathf.Sin(Mathf.PI * stepTimer.CompletionPercentage));
					transform.rotation = Quaternion.Slerp(startRotation, endRotation, stepTimer.CompletionPercentage);
				}				
				if(stepTimer.IsFinished){
					transform.position = endPos;
					if(CheckFollowThrough()){
						endStep = false;
						stepTimer.Reset();
					}
					else{
						if(!endStep){
							GameController.particleController.SpawnDustParticlesLight(transform.position, floorDir.Opposite());
							GameController.audioController.PlayThud();
							endStep = true;
							stepTimer.Reset();
							stepTimer.SetDuration(stepEndDuration);
						}
						else{
							endStep = false;
							moveState = MoveState.Idle;
							stepTimer.Reset();
							if(inputHeld){
								if(((holdDir == Direction.E) && (InputController.GetDirectionHeld().x > 0))
								|| ((holdDir == Direction.W) && (InputController.GetDirectionHeld().x < 0))
								|| ((holdDir == Direction.N) && (InputController.GetDirectionHeld().y > 0))
								|| ((holdDir == Direction.S) && (InputController.GetDirectionHeld().y < 0))){
									Move(curMoveDir, currentSpin);
								}
							}
						}
					}					
				}
			}
			else if(moveState == MoveState.Dashing){
				_rigidbody.AddForce(dashSpeed * curMoveDir.ToVector3() * Time.deltaTime, ForceMode.VelocityChange);
				bool rayHit = CastRayStack(transform.position, curMoveDir, 0.8f, gMask);
				if(rayHit){
					GameController.particleController.SpawnDustParticles(transform.position, curMoveDir.Opposite());
					GameController.audioController.PlayThud();
					_rigidbody.velocity = Vector3.zero;
					AlignToGrid();
					moveState = MoveState.Idle;
					floorDir = curMoveDir;
				}	
			}
		}
		else {
			AlignToGrid();
			AttemptMovement();
		}
		if(inputHeld){
			if(InputController.GetDirectionHeld() != holdDir.ToVector3()){
				inputHeld = false;
			}
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
		if(InputController.JumpButtonPressed()){
			Launch(floorDir.Opposite());
		}
		else{
			if(InputController.GetDirectionHeld().x < 0){
				if(!floorsTouching[(int)Direction.W]){
					inputHeld = true;
					holdDir = Direction.W;
					if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
						floorDir = Direction.S;
						Move(Direction.W, Spin.CCW);
					}
					else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
						floorDir = Direction.N;
						Move(Direction.W, Spin.CW);
					}
				}
			}
			else if(InputController.GetDirectionHeld().x > 0){
				if(!floorsTouching[(int)Direction.E]){
					inputHeld = true;
					holdDir = Direction.E;
					if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
						inputHeld = true;
						floorDir = Direction.S;
						Move(Direction.E, Spin.CW);
					}
					else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
						inputHeld = true;
						floorDir = Direction.N;
						Move(Direction.E, Spin.CCW);
					}
				}
			}
		 	if(InputController.GetDirectionHeld().y > 0){
				if(!floorsTouching[(int)Direction.N]){
					inputHeld = true;
					holdDir = Direction.N;
					if(floorsTouching[(int)Direction.W] && !floorsTouching[(int)Direction.E]){
						inputHeld = true;
						floorDir = Direction.W;
						Move(Direction.N, Spin.CCW);
					}
					else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
						inputHeld = true;
						floorDir = Direction.E;
						Move(Direction.N, Spin.CW);
					}
				}
			}
			else if(InputController.GetDirectionHeld().y < 0){
				if(!floorsTouching[(int)Direction.S]){
					inputHeld = true;
					holdDir = Direction.S;
					if(floorsTouching[(int)Direction.W] && !floorsTouching[(int)Direction.E]){
						inputHeld = true;
						floorDir = Direction.W;
						Move(Direction.S, Spin.CW);
					}
					else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
						inputHeld = true;
						floorDir = Direction.E;
						Move(Direction.S, Spin.CCW);
					}
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
			stepTimer.SetDuration(stepDuration);
			currentSpin = s;
			curMoveDir = d;
			startRotation = transform.rotation;
			if(s == Spin.CW){
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
			}
			else if(s == Spin.CCW){
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
			}
			moveState = MoveState.Stepping;
		}
		else{
			Direction d2 = (s == Spin.CW) ? d.Previous() : d.Next();
			dir = d2.ToVector3();
			rayHit = CastRayStack(startPos, dir, 1, gMask);
			if(!rayHit){
				floorDir = d;
				endPos = startPos + dir;
				stepTimer.SetDuration(stepDuration);
				currentSpin = s;
				curMoveDir = d2;
				startRotation = transform.rotation;
				if(s == Spin.CW){
					endRotation = startRotation * Quaternion.Euler(0, 0, -90);
				}
				else if(s == Spin.CCW){
					endRotation = startRotation * Quaternion.Euler(0, 0, 90);
				}
				moveState = MoveState.Stepping;
			}
		}
	}

	bool CheckFollowThrough(){
		bool doneMoving = false;
		bool rayHit = CastRayStack(transform.position, curMoveDir, 1, gMask);
		if(rayHit){
			doneMoving = true;
			//floorDir = curMoveDir;
			return !doneMoving;
		}

		Vector3 dir = floorDir.ToVector3();
		rayHit = CastRayStack(transform.position, dir, 1, gMask);
		if(!rayHit){		//no ground below us
			startPos = transform.position;
			endPos = startPos + dir;
			curMoveDir = floorDir;
			stepTimer.Reset();
			stepTimer.SetDuration(stepDuration);
			startRotation = transform.rotation;
			if(currentSpin == Spin.CW){
				floorDir = floorDir.Next();
				endRotation = startRotation * Quaternion.Euler(0, 0, -90);
			}
			else if(currentSpin == Spin.CCW){
				floorDir = floorDir.Previous();
				endRotation = startRotation * Quaternion.Euler(0, 0, 90);
			}
			moveState = MoveState.Stepping;
		}
		else{
			doneMoving = true;
		}
		return !doneMoving;
	}


	void Launch(Direction dir){
		ClearMovementParameters();
		bool rayHit = CastRayStack(transform.position, dir, 1, gMask);
		if(rayHit){
			return;
		}
		_rigidbody.AddForce(dashSpeed * dir.ToVector3(), ForceMode.Impulse);
		moveState = MoveState.Dashing;
		curMoveDir = dir;
		InputController.ClearInputs();
		inputHeld = false;
	}

	void ClearMovementParameters(){
		startPos = transform.position;
		endPos = transform.position;
		startRotation = transform.rotation;
		endRotation = transform.rotation;
		stepTimer.SetActive(false);
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

	bool CastRayStack(Vector3 origin, Vector3 dir, float len, out float hitLen, int mask){
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

	bool CastRayStack(Vector3 origin, Direction dir, float len, out float hitLen, int mask){
		return CastRayStack(origin, dir.ToVector3(), len, out hitLen, mask);
	}

}
