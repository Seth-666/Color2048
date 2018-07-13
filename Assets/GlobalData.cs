using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

	public static GlobalData Instance;

	public BackgroundTile bg;
	public BackgroundTile bg2;
	public Tile tile;

	public Globals.ColorSet[] colors;

	public AnimationCurve popin;
	public float popinTime;
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
