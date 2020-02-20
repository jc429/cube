using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}

	const float baseMoveSpeedG = 4f;
	const float baseMoveSpeedA = 3.2f;
	const float spdMulti = 1.3f;
	float GroundMoveSpeed{
		get{
			return baseMoveSpeedG;
		}
	}
	float AirMoveSpeed{
		get{
			return baseMoveSpeedA;
		}
	}
	float jumpSpeed = 9f;

	const int maxAirJumps = 1;
	int numAirJumps;
	bool isGrounded;

	public bool controlsLocked;
	public bool isDead;
	public bool hitstun;

	public bool IsMoving{
		get{
			return _rigidbody.velocity.x != 0;
		}
	}

	float dashPauseTimer;
	float dashPauseDuration = 0.15f;

	float wallJumpLockTimer;
	float wallJumpLockDuration = 0.2f;

	// Start is called before the first frame update
	void Start() {
		GameController.instance.player = this;
		Reset();
			
	}
	
	void Reset() {
		controlsLocked = false;
		isDead = false;
		hitstun = false;
	}

	// Update is called once per frame
	void Update() {
		BasicMovement();
	}

	// Handle physics in FixedUpdate
	void FixedUpdate () {
		if (!isGrounded && _rigidbody.velocity.y < 5f) {
			_rigidbody.AddForce(Physics.gravity * 2);
		}
		Vector3 v = _rigidbody.velocity;
		v.y = Mathf.Max(v.y, -20);
		_rigidbody.velocity = v;
	}



	void BasicMovement() {
		Vector3 moveInputs = Vector3.zero;
		moveInputs.x = VirtualController.GetAxisHorizontal();
		moveInputs.z = VirtualController.GetAxisVertical();
		//_anim.SetMoving(moveInputs.x != 0);
		//_anim.SetFacing(moveInputs.x);
		//_anim.SetCrouchInput(moveInputs.z < 0);
		

		if(moveInputs.x != 0 && _rigidbody.velocity.x != 0){
			_rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y);
		}

		Vector3 movement = CheckMovement(moveInputs, GroundMoveSpeed);
		
		Vector3 movepos = transform.position;
		movepos.x += movement.x; 
		//movepos.z += movement.z;

		transform.position = movepos;

	}

	public Vector3 CheckMovement(Vector3 movedir, float moveSpeed){
		if (movedir.Equals(Vector3.zero)) return Vector3.zero;
		Vector3 origin = transform.position;
		
		float spd =  0.5f + (moveSpeed * Time.deltaTime);
		Vector3 dir = Vector3.Normalize(new Vector3(movedir.x, 0, movedir.z));
		int gMask = Layers.GetSolidsMask(true);

		float dist = CastRayStack(origin, dir, spd, gMask);

		if(dist > 0){
			movedir *= (dist - 0.5f);
		}
		else{
			movedir *= (moveSpeed * Time.deltaTime);
		}

		
		return movedir;
	}

	float CastRayStack(Vector3 origin, Vector3 dir, float len, int mask){
		const int numRays = 5;
		if(len == 0){
			return 0;
		}
		RaycastHit r;
		float[] distlist = new float[9];

		for(int i = 0; i < numRays; i++){
			float height = -0.45f + (0.225f * i);
			Physics.Raycast(origin + new Vector3(0, height, 0), dir, out r, len, mask);
			
			Debug.DrawRay(origin + new Vector3(0, height, 0), len * dir, ((r.distance == 0) ? Color.white : Color.cyan));
			distlist[i] = r.distance;
		}
		
		float shortest = float.MaxValue;
		foreach (float f in distlist) {
			if (f > 0){
				shortest = Mathf.Min(shortest, f);
			}
		}
		return (shortest > 0 && shortest < float.MaxValue) ? shortest : 0;
	}

	float CastRayStack(Vector3 origin, Direction dir, float len, int mask){
		return CastRayStack(origin, dir.ToVector3(), len, mask);
	}
}
