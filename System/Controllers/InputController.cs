using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputID{
	DEBUG		= (1<<0),
	Left		= (1<<1),
	Right		= (1<<2),
	Up			= (1<<3),
	Down		= (1<<4),
	Jump		= (1<<5),
	Action	= (1<<6),
	Pause		= (1<<7),
	Reset		= (1<<8)
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

		inputs |= (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)) ? (ushort)InputID.Up : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)) ? (ushort)InputID.Down : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)) ? (ushort)InputID.Left : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) ? (ushort)InputID.Right : (ushort)0;
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
			return CheckQueuedInput(InputID.Up);
		}else{
			return CheckCurrentInput(InputID.Up);
		}
	}

	public static bool DownDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.Down);
		}else{
			return CheckCurrentInput(InputID.Down);
		}
	}

	public static bool LeftDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.Left);
		}else{
			return CheckCurrentInput(InputID.Left);
		}
	}

	public static bool RightDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.Right);
		}else{
			return CheckCurrentInput(InputID.Right);
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
