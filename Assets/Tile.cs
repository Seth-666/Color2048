using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public int type;
	public int level;
	public SpriteRenderer render;

	public Globals.Coord pos;

	public Animator eyeAnim;
	public float blinkTimer;

	public Animator mouthAnim;
	public bool selected = false;

	public bool initialized = false;

	void Start(){
		blinkTimer = Random.Range (5.0f, 10.0f);
		initialized = true;
	}

	void Update(){
		BlinkBehavior ();
	}

	public void ToggleState(bool state){
		if (selected && !state) {
			selected = false;
			mouthAnim.SetBool ("Active", false);
		} else if (!selected && state) {
			selected = true;
			mouthAnim.SetBool ("Active", true);
		}
	}

	void BlinkBehavior(){
		if (initialized) {
			if (blinkTimer > 0) {
				blinkTimer -= Time.deltaTime;
			} else if (blinkTimer <= 0) {
				blinkTimer = Random.Range (2.0f, 5.5f);
				eyeAnim.SetTrigger ("Blink");
			}
		}
	}
}
