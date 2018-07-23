using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	public BackgroundTile bg;
	public float bg1;
	public float bg2;
	public Tile tile;

	public GridManager grid;
	public UIManager ui;

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
