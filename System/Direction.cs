using UnityEngine;

public enum Direction {
	N = 0, 
	E = 1, 
	S = 2, 
	W = 3
}

public static class DirectionExtensions {
	public static Direction Opposite (this Direction direction) {
		return (int)direction < 2 ? (direction + 2) : (direction - 2);
	}

	public static Direction NextCCW (this Direction direction) {
		return direction == Direction.N ? Direction.W : (direction - 1);
	}

	public static Direction NextCW (this Direction direction) {
		return direction == Direction.W ? Direction.N : (direction + 1);
	}

	public static int DegreesOfRotation(this Direction direction){
		return (-90 * (int)direction) % 360;
	}

	public static Direction DirectionFromDegrees(int degrees){
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		return (Direction)(degrees / 90); 
	}

	public static Direction RandomDirection(){
		return (Direction)Mathf.FloorToInt(Random.Range(0,4));
	}

	public static Vector3 ToVector3(this Direction direction){
		switch(direction){
			case Direction.N:
				return Vector3.up;
			case Direction.E:
				return Vector3.right;
			case Direction.S:
				return Vector3.down;
			case Direction.W:
				return Vector3.left;
			default:
				return Vector3.zero;
		}
	}
}
