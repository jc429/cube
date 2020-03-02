using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputID{
	DEBUG				= (1<<0),
	DpadLeft		= (1<<1),
	DpadRight		= (1<<2),
	DpadUp			= (1<<3),
	DpadDown		= (1<<4),
	FaceN 			= (1<<5),
	FaceE 			= (1<<6),
	FaceS 			= (1<<7),
	FaceW 			= (1<<8),
	Jump				= (1<<9),
	Action			= (1<<10),
	Pause				= (1<<11),
	Reset				= (1<<12)
}

public static class InputController {

	static int inputQueueLength = 10;

	static ushort currentInputs;
	
	static List<ushort> inputQueue = new List<ushort>();

	public static void Initialize(){
		if(inputQueue == null){
			inputQueue = new List<ushort>();
		}
		ClearInputs();
	}

	public static void ClearInputs(){
		inputQueue.Clear();
	}

	public static void CaptureInputs(){
		ushort inputs = 0;

		inputs |= (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)) ? (ushort)InputID.DpadUp : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)) ? (ushort)InputID.DpadDown : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)) ? (ushort)InputID.DpadLeft : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) ? (ushort)InputID.DpadRight : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.I)) ? (ushort)InputID.FaceN : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.K)) ? (ushort)InputID.FaceS : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.J)) ? (ushort)InputID.FaceW : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.L)) ? (ushort)InputID.FaceE : (ushort)0;
		inputs |= (Input.GetButtonDown("Jump")) ? (ushort)InputID.Jump : (ushort)0;
		inputs |= (Input.GetButtonDown("Action")) ? (ushort)InputID.Action : (ushort)0;
		inputs |= (Input.GetButtonDown("Pause")) ? (ushort)InputID.Pause : (ushort)0;
		inputs |= (Input.GetButtonDown("Reset")) ? (ushort)InputID.Reset : (ushort)0;


		currentInputs = inputs; 
		inputQueue.Add(inputs);
		if(inputQueue.Count > inputQueueLength){
			inputQueue.RemoveAt(0);
		}
	}

	public static bool CheckQueuedInput(InputID input){
		return CheckQueuedInput((ushort)input);
	}

	public static bool CheckQueuedInput(ushort input){
		if(inputQueue.Count < 1){
			return false;
		}
		for(int i = inputQueue.Count - 1; i >= 0; i--){
			if((inputQueue[i] & input) != 0){
				return true;
			}
		}
		return false;
	}

	public static bool CheckCurrentInput(InputID input){
		return CheckCurrentInput((ushort)input);
	}

	public static bool CheckCurrentInput(ushort input){
		return ((currentInputs & input) != 0);
	}

	public static float GetAxisHorizontal(){
		return Input.GetAxisRaw("Horizontal");
	}

	public static float GetAxisVertical(){
		return Input.GetAxisRaw("Vertical");
	}

	public static bool UpDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadUp);
		}else{
			return CheckCurrentInput(InputID.DpadUp);
		}
	}

	public static bool DownDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadDown);
		}else{
			return CheckCurrentInput(InputID.DpadDown);
		}
	}

	public static bool LeftDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadLeft);
		}else{
			return CheckCurrentInput(InputID.DpadLeft);
		}
	}

	public static bool RightDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadRight);
		}else{
			return CheckCurrentInput(InputID.DpadRight);
		}
	}

	public static Vector3 GetDirectionHeld(){
		Vector3 v = Vector3.zero;
		v.x += RightDPadPressed() ?  1 : 0;
		v.x += LeftDPadPressed()  ? -1 : 0;
		v.y += UpDPadPressed()    ?  1 : 0;
		v.y += DownDPadPressed()  ? -1 : 0;
		return v;
	}

	/**********************************************************************************/

	public static bool FaceButtonNorthPressed(bool queued = true){
		if(queued){
			return CheckQueuedInput(InputID.FaceN);
		}else{
			return CheckCurrentInput(InputID.FaceN);
		}
	}

	public static bool FaceButtonSouthPressed(bool queued = true){
		if(queued){
			return CheckQueuedInput(InputID.FaceS);
		}else{
			return CheckCurrentInput(InputID.FaceS);
		}
	}

	public static bool FaceButtonWestPressed(bool queued = true){
		if(queued){
			return CheckQueuedInput(InputID.FaceW);
		}else{
			return CheckCurrentInput(InputID.FaceW);
		}
	}

	public static bool FaceButtonEastPressed(bool queued = true){
		if(queued){
			return CheckQueuedInput(InputID.FaceE);
		}else{
			return CheckCurrentInput(InputID.FaceE);
		}
	}

	/**********************************************************************************/

	public static bool JumpButtonPressed(bool queued = true){
		if(queued){
			return CheckQueuedInput(InputID.Jump);
		}else{
			return CheckCurrentInput(InputID.Jump);
		}
	}

	public static bool ActionButtonPressed(bool held = false){

		return Input.GetButtonDown("Action");

	}

	public static bool PauseButtonPressed(bool held = false){

		return (Input.GetButtonDown("Pause"));

		
	}

	public static bool ResetButtonPressed(bool held = false){
		
		return (Input.GetButtonDown("Reset"));

		
	}

	public static bool AnyKeyPressed(){
		return ActionButtonPressed() || JumpButtonPressed() 
		|| (GetAxisHorizontal() != 0) || (GetAxisVertical() != 0)
		|| PauseButtonPressed();
	}
}
