using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
	Rigidbody _rigidbody{
		get{ return GetComponent<Rigidbody>(); }
	}
	PlayerController _controller{
		get{ return GetComponent<PlayerController>(); }
	}
	Movement _movement{
		get{ return GetComponent<Movement>(); }
	}

	[SerializeField]
	[NamedArrayAttribute (new string[] {"Up", "Right", "Down", "Left"})]
	BoxArm[] arms;

	public bool ArmMoving{
		get{
			for(int i = 0; i < 4; i++){
				if(arms[i].IsMoving){
					return true;
				}
			}
			return false;
		}
	}


	// Start is called before the first frame update
	void Start()
	{
			
	}

	// Update is called once per frame
	void Update()
	{
			
	}

	public BoxArm GetArm(Direction dir){
		int armNo = (int)dir;
		int offset = 0;
		switch(_controller.Orientation){
			case Direction.N:
			default:
				offset = 0;
				break;
			case Direction.E:
				offset = 3;
				break;
			case Direction.S:
				offset = 2;
				break;
			case Direction.W:
				offset = 1;
				break;
		}
		armNo = (armNo + offset) % 4;
		return arms[armNo];
	}
}
