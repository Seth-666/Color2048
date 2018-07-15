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
	public Globals.State state;

	public bool initialized = false;
	Vector2 startPos;
	public float floatTimer;

	public SpriteRenderer[] sprites;

	void Start(){
		blinkTimer = Random.Range (5.0f, 10.0f);
		initialized = true;
	}

	void Update(){
		BlinkBehavior ();
		FloatBehavior ();
	}

	public void ChangeLayers(bool higher){
		if (higher) {
			for (int xx = 0; xx < sprites.Length; xx++) {
				sprites [xx].sortingOrder += 100;
			}
		}
		else{
			for (int xx = 0; xx < sprites.Length; xx++) {
				sprites [xx].sortingOrder -= 100;
			}
		}
	}

	public void ToggleState(Globals.State newState){
		if (state == Globals.State.Waiting) {
			if (newState == Globals.State.Selected) {
				ChangeLayers (true);
				startPos = this.transform.position;
				floatTimer = 0;
				state = newState;
				mouthAnim.SetBool ("Active", true);
			}
		} else if (state == Globals.State.Moving) {
			if (newState == Globals.State.Waiting) {
				ChangeLayers (false);
				state = newState;
				mouthAnim.SetBool ("Active", false);
			}
		} else if (state == Globals.State.Selected) {
			if (newState == Globals.State.Waiting) {
				this.transform.position = startPos;
				ChangeLayers (false);
				state = newState;
				mouthAnim.SetBool ("Active", false);
			}
			if (newState == Globals.State.Moving) {
				state = newState;
			}
		}
	}

	void FloatBehavior(){
		if (state == Globals.State.Selected) {
			if (floatTimer >= 1) {
				floatTimer = 0;
			}
			floatTimer += Time.deltaTime;
			Vector2 newPos = new Vector2 (startPos.x, startPos.y + (GlobalData.Instance.floatCurve.Evaluate (floatTimer)));
			this.transform.position = newPos;
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
