using UnityEngine;

public enum Spin {
	CW, CCW
}

public static class SpinExtensions {
	public static Spin Opposite(this Spin spin){
		return (spin == Spin.CCW) ? Spin.CW : Spin.CCW;
	}
}