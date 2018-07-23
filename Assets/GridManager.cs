using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	public LayerMask tileLayer;
	public LayerMask backgroundLayer;

	//Grid dimensions.
	public int xSize, ySize;

	//Spacing for instantiation.
	public float tileSize;

	//What tiles in the grid are active?
	public bool[,] grid;

	//What tiles are in what spaces?
	public Tile[,] tiles;

	//How many levels of color does the level go up to?
	public int maxLevel;

	//How many tiles start on the board?
	public int startingTiles;

	//Min/max for how many tiles are placed on the board if a false move is made.
	public int minSpawn, maxSpawn;

	//How many matches have been made since no matches were made?
	public int comboCount;

	public Globals.State currState;
	public Globals.State prevState;
	public int waitCount;

	//Which tile was last selected.
	public Tile selectedTile;
	public float selectionTimer;

	public bool matchFound = false;

	public List<Tile> allTiles;

	void Start(){
		GameManager.Instance.grid = this;
		currState = Globals.State.Waiting;
		prevState = Globals.State.Waiting;
		Initialize ();
		CreateGrid ();
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			GameObject[] toDestroy = GameObject.FindGameObjectsWithTag ("Tile");
			for (int xx = 0; xx < toDestroy.Length; xx++) {
				Destroy (toDestroy [xx]);
			}
			allTiles.Clear ();
			Start ();
		}
		if (currState == Globals.State.Waiting) {
			if (waitCount > 0) {
				prevState = currState;
				currState = Globals.State.Busy;
			}
			else {
				InputDetection ();
			}
		} else if (currState == Globals.State.Selected) {
			if (waitCount > 0) {
				prevState = currState;
				currState = Globals.State.Busy;
			}
			else {
				InputDetection ();
			}
		} else if (currState == Globals.State.Busy) {
			if (waitCount <= 0) {
				currState = prevState;
			}
		} else if (currState == Globals.State.Moving) {
			if (waitCount <= 0) {
				GridSweep();
			}
		}
	}

	//Resets whole grid to ensure that tiles aren't accidentally spawned over each other.
	void ResetGrid(){
		tiles = new Tile[xSize, ySize];
		for (int xx = 0; xx < allTiles.Count; xx++) {
			tiles [allTiles [xx].pos.x, allTiles [xx].pos.y] = allTiles [xx];
		}
	}

	public void Shuffle(){
		if (currState == Globals.State.Waiting) {
			//Get list of positions available.
			List<Globals.Coord> positions = new List<Globals.Coord> ();
			for (int xx = 0; xx < xSize; xx++) {
				for (int yy = 0; yy < ySize; yy++) {
					if (grid [xx, yy] == true) {
						Globals.Coord pos = new Globals.Coord (xx, yy);
						positions.Add (pos);
					}
				}
			}
			//Clear all tiles from the grid.
			tiles = new Tile[xSize, ySize];
			for (int xx = 0; xx < allTiles.Count;) {
				int pickedPos = Random.Range (0, positions.Count);
				Globals.Coord pos = positions [pickedPos];
				int surroundCount = SurroundingType (allTiles[xx].type, positions [pickedPos]);
				if (surroundCount < 2) {
					if (tiles [positions [pickedPos].x, positions [pickedPos].y] == null) {
						positions.RemoveAt (pickedPos);
						Vector2 targetPos = PosToVector2 (pos.x, pos.y);
						StartCoroutine (MoveTile (allTiles [xx], targetPos, pos.x, pos.y, Random.Range(0.1f, 0.5f)));
						xx++;
					}
				}
			}
		}
		ResetGrid ();
	}

	IEnumerator DropInTile(Tile tile, Vector2 targetPos){
		float timer = 0;
		Vector2 startPos = targetPos;
		startPos.y += 10.0f;
		while (timer < GameManager.Instance.dropTime) {
			timer += Time.deltaTime;
			Vector2 pos = Vector2.Lerp (startPos, targetPos, Mathf.InverseLerp (0, GameManager.Instance.dropTime, timer));
			tile.transform.position = pos;
			yield return null;
		}
		tile.transform.position = targetPos;
		waitCount--;
	}

	IEnumerator MoveTile(Tile tile, Vector2 targetPos, int posX, int posY, float waitTime){
		waitCount++;
		tile.ToggleState (Globals.State.Moving);
		tiles [tile.pos.x, tile.pos.y] = null;
		tile.pos.x = posX;
		tile.pos.y = posY;
		tiles [posX, posY] = tile;

		if (waitTime > 0) {
			yield return new WaitForSeconds (waitTime);
		}
		float timer = 0;
		Vector2 startPos = tile.transform.position;
		while (timer < 1.0f) {
			timer += Time.deltaTime * 7;
			Vector2 pos = Vector2.Lerp(startPos, targetPos, Mathf.InverseLerp(0, 1.0f, timer));
			tile.transform.position = pos;
			yield return null;
		}
		tile.transform.position = targetPos;
		waitCount--;
	}

	void InputDetection(){
		if (Input.GetMouseButtonDown (0)) {
			Vector3 mousePos = Input.mousePosition;
			mousePos.z = 10.0f;
			mousePos = Camera.main.ScreenToWorldPoint (mousePos);
			if (currState == Globals.State.Waiting) {
				if (Physics2D.OverlapPoint (mousePos, tileLayer)) {
					selectedTile = Physics2D.OverlapPoint (mousePos, tileLayer).GetComponent<Tile> ();
					if (currState == Globals.State.Waiting) {
						ToggleMode (Globals.State.Selected);
					}
				}
			} else if (currState == Globals.State.Selected) {
				if (Physics2D.OverlapPoint (mousePos, tileLayer)) {
					//If another tile is clicked, deselect.
					ToggleMode (Globals.State.Waiting);
				} else {
					if (Physics2D.OverlapPoint (mousePos, backgroundLayer)) {
						currState = Globals.State.Moving;
						BackgroundTile bgObj = Physics2D.OverlapPoint(mousePos, backgroundLayer).GetComponent<BackgroundTile>();
						if (tiles [bgObj.pos.x, bgObj.pos.y] == null) {
							StartCoroutine (MoveTile (selectedTile, bgObj.transform.position, bgObj.pos.x, bgObj.pos.y, 0));
						} else {
							ToggleMode (Globals.State.Waiting);
						}
					}
					else{
						ToggleMode(Globals.State.Waiting);
					}
				}
			}
		}
	}

	void GridSweep(){
		//Check each of the cells in a square-pattern around the origin.
		//If any hits are made, cancel remaining checks and merge together/remove.
		//If no hits, don't forget to spawn more tiles.
		if (AllEqual (Globals.GetHorizSquares(selectedTile.pos), selectedTile.type, selectedTile.level)) {
			matchFound = true;
			ClearTiles (ReturnEqual (Globals.GetHorizSquares (selectedTile.pos), selectedTile.type, selectedTile.level));
		}
		if (!matchFound) {
			if (AllEqual (Globals.GetVertSquares(selectedTile.pos), selectedTile.type, selectedTile.level)) {
				matchFound = true;
				ClearTiles (ReturnEqual (Globals.GetVertSquares (selectedTile.pos), selectedTile.type, selectedTile.level));
			}
		}
		if (!matchFound) {
			if (AllEqual (Globals.GetSquareSets(selectedTile.pos), selectedTile.type, selectedTile.level)) {
				matchFound = true;
				ClearTiles (ReturnEqual (Globals.GetSquareSets (selectedTile.pos), selectedTile.type, selectedTile.level));
			}
		}
			
		if (!matchFound) {
			comboCount = 0;
			SpawnExtraTiles ();
			ToggleMode (Globals.State.Waiting);
		} else {
			comboCount++;
			matchFound = false;
			ToggleMode (Globals.State.Waiting);
		}
	}

	void ToggleMode(Globals.State theState){
		if (theState == Globals.State.Selected) {
			currState = theState;
			selectedTile.ToggleState (Globals.State.Selected);
		} else if (theState == Globals.State.Waiting) {
			currState = theState;
			selectedTile.ToggleState (Globals.State.Waiting);
			selectedTile = null;
		}
		ResetGrid ();
	}

	//Checks if all tiles in grid are equal.
	bool AllEqual(List<Globals.CoordSet> squares, int type, int level){
		bool ret = false;
		//We need to check if it's a valid coord, whether the space exists on the grid,
		//If there's a tile in the space and if the tile matches the original color.
		for(int xx = 0; xx < squares.Count; xx++){
			bool squareMatched = true;
			for (int yy = 0; yy < squares [xx].coords.Count; yy++) {
				if (!IsInGrid (squares[xx].coords [yy])) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y] == null) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y].type != type) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y].level != level) {
					squareMatched = false;
					break;
				}
				if (!squareMatched) {
					break;
				}
			}
			if (squareMatched) {
				ret = true;
				break;
			}
		}
		return ret;
	}

	List<Globals.Coord> ReturnEqual(List<Globals.CoordSet> squares, int type, int level){
		List<Globals.Coord> ret = null;
		//We need to check if it's a valid coord, whether the space exists on the grid,
		//If there's a tile in the space and if the tile matches the original color.
		for(int xx = 0; xx < squares.Count; xx++){
			bool squareMatched = true;
			for (int yy = 0; yy < squares [xx].coords.Count; yy++) {
				if (!IsInGrid (squares[xx].coords [yy])) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y] == null) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y].type != type) {
					squareMatched = false;
					break;
				}
				if (tiles [squares[xx].coords [yy].x, squares[xx].coords [yy].y].level != level) {
					squareMatched = false;
					break;
				}
				if (!squareMatched) {
					break;
				}
			}
			if (squareMatched) {
				ret = squares[xx].coords;
				break;
			}
		}
		return ret;
	}

	void ClearTiles(List<Globals.Coord> positions){
		//First we tally the score.
		int score = 0;
		//If 4 tiles are matched, the score is 4 * the level of the tiles * the current combo.
		if (positions.Count == 4) {
			int val = 4 * (tiles [positions [0].x, positions [0].y].level + 1);
			val *= comboCount + 1;
			score += val;
		} 
		//If 6 tiles are matched, the score is 6 * 2 * the level of the tiles * the current combo.
		else if (positions.Count == 6) {
			int val = 12 * (tiles [positions [0].x, positions [0].y].level + 1);
			val *= comboCount + 1;
			score += val;
		}
		GameManager.Instance.ui.AddToScore (score);
		//If there's levels in the game, check to see if the tiles can upgrade.
		if (tiles [positions [0].x, positions [0].y].level < maxLevel) {
			StartCoroutine (UpgradeTiles (positions));
		}
		//Otherwise, destroy them.
		else {
			for (int xx = 0; xx < positions.Count; xx++) {
				allTiles.Remove (tiles [positions [xx].x, positions [xx].y]);
				Destroy (tiles [positions [xx].x, positions [xx].y].gameObject);
				tiles [positions [xx].x, positions [xx].y] = null;
			}
		}
		ResetGrid ();
	}

	IEnumerator UpgradeTiles(List<Globals.Coord> pos){
		waitCount++;
		int picked = Random.Range (0, pos.Count);
		Tile pickedTile = tiles [pos [picked].x, pos [picked].y];
		Vector2[] startPositions = new Vector2[pos.Count];
		Vector2 endPos = pickedTile.transform.position;
		for (int xx = 0; xx < startPositions.Length; xx++) {
			startPositions [xx] = PosToVector2 (pos [xx].x, pos [xx].y);
		}
		float timer = 0;
		while (timer < 1.0f) {
			timer += Time.deltaTime * 10;
			for (int xx = 0; xx < pos.Count; xx++) {
				if (xx != picked) {
					tiles [pos [xx].x, pos [xx].y].transform.position = Vector2.Lerp (startPositions [xx], endPos, timer);
				}
			}
			yield return null;
		}
		for(int xx = 0; xx < pos.Count; xx++){
			if(xx != picked){
				allTiles.Remove (tiles [pos [xx].x, pos [xx].y]);
				Destroy (tiles [pos [xx].x, pos [xx].y].gameObject);
				tiles [pos [xx].x, pos [xx].y] = null;
			}
			else{
				pickedTile.level++;
				pickedTile.render.color = GameManager.Instance.colors[pickedTile.type].colors[pickedTile.level];
			}
		}
		waitCount--;
		ResetGrid ();
	}

	void SpawnExtraTiles(){
		if (HasFreeTiles ()) {
			List<Globals.Coord> free = GetEmptySpaces ();
			int spawnCount = Random.Range (minSpawn, maxSpawn);
			for (int xx = 0; xx < spawnCount;) {
				int picked = Random.Range (0, free.Count);
				int type = Random.Range (0, GameManager.Instance.colors.Length);
				int surroundCount = SurroundingType (type, free [picked]);
				Globals.Coord pos = free [picked];
				if (surroundCount < 2) {
					free.RemoveAt (picked);
					StartCoroutine(CreateTile (pos.x, pos.y, type));
					if (free.Count <= 0) {
						Debug.Log ("No free spaces found. Game over.");
						break;
					}
					xx++;
				}
			}
		} else {
			Debug.Log ("No free spaces found. Game over.");
		}
		ResetGrid ();
	}

	int SurroundingType(int type, Globals.Coord pos){
		int ret = 0;
		for (int xx = pos.x - 1; xx < pos.x + 2; xx++) {
			for (int yy = pos.y - 1; yy < pos.y + 2; yy++) {
				if (xx == pos.x || yy == pos.y) {
					if (IsInGrid (xx, yy)) {
						if (tiles [xx, yy] != null) {
							if (tiles [xx, yy].type == type) {
								ret++;
							}
						}
					}
				}
			}
		}
		return ret;
	}

	List<Globals.Coord> GetEmptySpaces(){
		List<Globals.Coord> empties = new List<Globals.Coord> ();
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (grid [xx, yy]) {
					if (tiles [xx, yy] == null) {
						Globals.Coord newCoord = new Globals.Coord (xx, yy);
						empties.Add (newCoord);
					}
				}
			}
		}
		return empties;
	}

	bool HasFreeTiles(){
		bool ret = false;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (grid [xx, yy]) {
					if (tiles [xx, yy] == null) {
						ret = true;
					}
				}
				if (ret) {
					break;
				}
			}
			if (ret) {
				break;
			}
		}
		return ret;
	}

	void Initialize(){
		grid = new bool[xSize, ySize];
		tiles = new Tile[xSize, ySize];
		allTiles = new List<Tile> ();
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				grid [xx, yy] = true;
			}
		}
	}

	//Generates grid.
	void CreateGrid(){
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (grid [xx, yy]) {
					Vector2 myPos = PosToVector2 (xx, yy);
					BackgroundTile bg = Instantiate(GameManager.Instance.bg);
					Color col = bg.render.color;
					if (xx % 2 == 0) {
						if (yy % 2 == 0) {
							col.a = Mathf.InverseLerp(0, 255, GameManager.Instance.bg1);
						} else {
							col.a = Mathf.InverseLerp(0, 255, GameManager.Instance.bg2);
						}
					} else {
						if (yy % 2 == 0) {
							col.a = Mathf.InverseLerp(0, 255, GameManager.Instance.bg2);
						} else {
							col.a = Mathf.InverseLerp(0, 255, GameManager.Instance.bg1);
						}
					}
					bg.render.color = col;
					bg.transform.position = myPos;
					bg.pos.x = xx;
					bg.pos.y = yy;
				}
			}
		}
		for (int xx = 0; xx < startingTiles;) {
			int randX = Random.Range (0, xSize);
			int randY = Random.Range (0, ySize);
			int type = Random.Range (0, GameManager.Instance.colors.Length);
			if (tiles [randX, randY] == null) {
				StartCoroutine(CreateTile (randX, randY, type));
				xx++;
			}
		}
		ResetGrid ();
	}

	public bool IsInGrid(Globals.Coord pos){
		bool ret = false;
		if (pos.x >= 0 && pos.x < xSize) {
			if (pos.y >= 0 && pos.y < ySize) {
				ret = true;
			}
		}
		return ret;
	}

	public bool IsInGrid(int xx, int yy){
		bool ret = false;
		if (xx >= 0 && xx < xSize) {
			if (yy >= 0 && yy < ySize) {
				ret = true;
			}
		}
		return ret;
	}

	IEnumerator CreateTile(int posX, int posY, int type){
		waitCount++;
		Tile newTile = Instantiate(GameManager.Instance.tile);
		Vector2 pos = PosToVector2(posX, posY);
		newTile.transform.position = new Vector2 (pos.x, pos.y + 10);
		newTile.state = Globals.State.Waiting;
		tiles [posX, posY] = newTile;
		newTile.pos.x = posX;
		newTile.pos.y = posY;
		newTile.type = type;
		newTile.render.color = GameManager.Instance.colors [newTile.type].colors [0];
		if (!allTiles.Contains (newTile)) {
			allTiles.Add (newTile);
		}
		yield return new WaitForSeconds (Random.Range(0.05f, 0.3f));
		StartCoroutine (DropInTile (newTile, pos));
	}

	public Vector2 PosToVector2(int xx, int yy){
		Vector2 ret = new Vector2 ((xx - (xSize / 2)) * tileSize, (yy - (ySize / 2)) * tileSize);
		if (xSize % 2 == 0) {
			ret.x += tileSize / 2;
		}
		if (ySize % 2 == 0) {
			ret.y += tileSize / 2;
		}
		return ret;
	}

}
