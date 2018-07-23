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

	}
}
