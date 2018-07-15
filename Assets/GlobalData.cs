using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

	public static GlobalData Instance;

	public BackgroundTile bg;
	public BackgroundTile bg2;
	public Tile tile;

	public Globals.ColorSet[] colors;

	public AnimationCurve floatCurve;

	public float dropTime;
	public float tileSize;

	void Awake(){
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (this.gameObject);
		} else {
			Destroy (this.gameObject);
		}
	}

}
