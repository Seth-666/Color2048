using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public int score;

	public Text scoreText;

	void Start(){
		GameManager.Instance.ui = this;
	}

	public void AddToScore(int count){
		score += count;
		scoreText.text = score.ToString ();
	}

	public void Shuffle(){
		GameManager.Instance.grid.Shuffle ();
	}

	public void Swap(){
		if (GameManager.Instance.grid.currState == Globals.State.Waiting) {
			GameManager.Instance.grid.ToggleMode (Globals.State.Swap);
		} else if (GameManager.Instance.grid.currState == Globals.State.Swap || GameManager.Instance.grid.currState == Globals.State.SwapSelected) {
			GameManager.Instance.grid.ToggleMode (Globals.State.Waiting);
		}
	}
}
