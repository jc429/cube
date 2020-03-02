using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(ArmManager))]
[SelectionBase]
public class PlayerController : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}
	Movement _movement{
		get{ return GetComponent<Movement>(); }
	}
	ArmManager _armManager{
		get{ return GetComponent<ArmManager>(); }
	}

	/* blast particle prefab */
	public BlastParticle blastParticles;

	Direction orientation = Direction.N;
	public Direction Orientation{
		get{ return orientation; }
		set{ orientation = value; }
	}



	bool inputHeld;
	public bool InputHeld{
		get{ return inputHeld; }
	}
	public void ForceReleaseInput(){
		inputHeld = false;
	}
	public Direction holdDir;

	[SerializeField]
	[NamedArrayAttribute (new string[] {"North", "East", "South", "West"})]
	bool[] floorsTouching = {false, false, false, false};
	Direction floorDir;
	public Direction FloorDir{
		get{ return floorDir; }
	}



	// Start is called before the first frame update
	void Start()
	{
		GameController.instance.player = this;
	}

	// Update is called once per frame
	void Update()
	{
		CheckFloor();
		if(_movement.IsMoving){
			_movement.UpdateMovement();
		}
		else{
			_movement.AlignToGrid();
			AttemptMovement();
			CheckArmInputs();
		}

		if(inputHeld){
			if(InputController.GetDirectionHeld() != holdDir.ToVector3()){
				inputHeld = false;
			}
		}
	}

	void AttemptMovement(){
		if(InputController.JumpButtonPressed()){
			AttemptDash(floorDir.Opposite());
		}
		else{
			if(InputController.GetDirectionHeld().x < 0){
				if(!floorsTouching[(int)Direction.W]){
					inputHeld = true;
					holdDir = Direction.W;
					if(floorsTouching[(int)Direction.S] && !floorsTouching[(int)Direction.N]){
						floorDir = Direction.S;
						_movement.Move(Direction.W, Spin.CCW);
					}
					else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
						floorDir = Direction.N;
						_movement.Move(Direction.W, Spin.CW);
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
						_movement.Move(Direction.E, Spin.CW);
					}
					else if(floorsTouching[(int)Direction.N] && !floorsTouching[(int)Direction.S]){
						inputHeld = true;
						floorDir = Direction.N;
						_movement.Move(Direction.E, Spin.CCW);
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
						_movement.Move(Direction.N, Spin.CCW);
					}
					else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
						inputHeld = true;
						floorDir = Direction.E;
						_movement.Move(Direction.N, Spin.CW);
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
						_movement.Move(Direction.S, Spin.CW);
					}
					else if(floorsTouching[(int)Direction.E] && !floorsTouching[(int)Direction.W]){
						inputHeld = true;
						floorDir = Direction.E;
						_movement.Move(Direction.S, Spin.CCW);
					}
				}
			}
		}
	}
	
	public void AttemptDash(Direction dir){
		if(_movement.AttemptDashMovement(dir)){
			blastParticles.SetDirection(_movement.CurrentMoveDirection);
			blastParticles.Play();
			InputController.ClearInputs();
			ForceReleaseInput();
		}
	}

	public void EndDash(){
		SetFloorDir(_movement.CurrentMoveDirection);
		GameController.particleController.SpawnDustParticles(transform.position, floorDir.Opposite());
		GameController.audioController.PlayThud();
		GameController.cameraController.CrashShake(_movement.CurrentMoveDirection);
		blastParticles.Stop();
		_movement.EndDashMovement();
	}


	void CheckFloor(){
		Direction d;
		int i;
		for (d = Direction.N, i = 0; d <= Direction.W; d++, i++) {
			floorsTouching[i] = _movement.CastRayStack(transform.position, d, 0.7f, Layers.GroundMask);
		}
	}

	public void SetFloorDir(Direction d){
		floorDir = d;
	}

	public bool MovementInputSustained(){
		return (
			InputHeld && (
				((holdDir == Direction.E) && (InputController.GetDirectionHeld().x > 0))
				|| ((holdDir == Direction.W) && (InputController.GetDirectionHeld().x < 0))
				|| ((holdDir == Direction.N) && (InputController.GetDirectionHeld().y > 0))
				|| ((holdDir == Direction.S) && (InputController.GetDirectionHeld().y < 0))
			)
		);
	}

	void CheckArmInputs(){
		if(InputController.FaceButtonNorthPressed(false)){
			_armManager.GetArm(Direction.N).Extend();
		}else{
			_armManager.GetArm(Direction.N).Retract();
		}
		if(InputController.FaceButtonSouthPressed(false)){
			_armManager.GetArm(Direction.S).Extend();
		}else{
			_armManager.GetArm(Direction.S).Retract();
		}
		if(InputController.FaceButtonWestPressed(false)){
			_armManager.GetArm(Direction.W).Extend();
		}else{
			_armManager.GetArm(Direction.W).Retract();
		}
		if(InputController.FaceButtonEastPressed(false)){
			_armManager.GetArm(Direction.E).Extend();
		}else{
			_armManager.GetArm(Direction.E).Retract();
		}

	}
}
