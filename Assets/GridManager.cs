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

	//Which tile was last selected.
	public Tile selectedTile;

	public bool matchFound = false;

	List<Tile> allTiles;

	void Start(){
		Initialize ();
		CreateGrid ();
	}

	public IEnumerator PopInTile(Tile tile){
		float timer = 0;
		while (timer < GlobalData.Instance.popinTime) {
			timer += Time.deltaTime;
			float lerpAmount = Mathf.InverseLerp (0, GlobalData.Instance.popinTime, timer);
			float scaleAmount = GlobalData.Instance.popin.Evaluate (lerpAmount) * GlobalData.Instance.tileSize;
			Vector3 targetScale = new Vector3 (scaleAmount, scaleAmount, scaleAmount);
			tile.transform.localScale = targetScale;
			yield return null;
		}
		tile.transform.localScale = new Vector3 (GlobalData.Instance.tileSize, GlobalData.Instance.tileSize, GlobalData.Instance.tileSize);
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
		InputDetection ();
	}

	void InputDetection(){
		if (Input.GetMouseButtonDown (0)) {
			Vector3 mousePos = Input.mousePosition;
			mousePos.z = 10.0f;
			mousePos = Camera.main.ScreenToWorldPoint (mousePos);
			if (currState == Globals.State.Waiting) {
				if (Physics2D.OverlapPoint (mousePos, tileLayer)) {
					selectedTile = Physics2D.OverlapPoint (mousePos, tileLayer).GetComponent<Tile> ();
					ToggleMode (Globals.State.Selected);
				}
			} else if (currState == Globals.State.Selected) {
				if (Physics2D.OverlapPoint (mousePos, tileLayer)) {
					//If another tile is clicked, deselect.
					ToggleMode (Globals.State.Waiting);
				} else {
					if (Physics2D.OverlapPoint (mousePos, backgroundLayer)) {
						BackgroundTile bgObj = Physics2D.OverlapPoint(mousePos, backgroundLayer).GetComponent<BackgroundTile>();
						selectedTile.transform.position = bgObj.transform.position;
						tiles [selectedTile.pos.x, selectedTile.pos.y] = null;
						selectedTile.pos.x = bgObj.pos.x;
						selectedTile.pos.y = bgObj.pos.y;
						tiles [bgObj.pos.x, bgObj.pos.y] = selectedTile;
						//Wait for movement finish.
						GridSweep ();
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
			Debug.Log ("Horiz set found.");
			matchFound = true;
			ClearTiles (ReturnEqual (Globals.GetHorizSquares (selectedTile.pos), selectedTile.type, selectedTile.level));
		}
		if (!matchFound) {
			if (AllEqual (Globals.GetVertSquares(selectedTile.pos), selectedTile.type, selectedTile.level)) {
				Debug.Log ("Vert set found.");
				matchFound = true;
				ClearTiles (ReturnEqual (Globals.GetVertSquares (selectedTile.pos), selectedTile.type, selectedTile.level));
			}
		}
		if (!matchFound) {
			if (AllEqual (Globals.GetSquareSets(selectedTile.pos), selectedTile.type, selectedTile.level)) {
				Debug.Log ("Square set found.");
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
		/*if (a.level < maxLevel) {

		}*/ 
		for(int xx = 0; xx < positions.Count; xx++){
			allTiles.Remove (tiles[positions[xx].x, positions[xx].y]);
			Destroy(tiles[positions[xx].x, positions[xx].y].gameObject);
			tiles [positions[xx].x, positions[xx].y] = null;
		}
	}

	void SpawnExtraTiles(){
		if (HasFreeTiles ()) {
			List<Globals.Coord> free = GetEmptySpaces ();
			int spawnCount = Random.Range (minSpawn, maxSpawn);
			for (int xx = 0; xx < spawnCount;) {
				int picked = Random.Range (0, free.Count);
				int type = Random.Range (0, GlobalData.Instance.colors.Length);
				int surroundCount = SurroundingType (type, free [picked]);
				Debug.Log ("Surrounds: " + surroundCount);
				if (surroundCount < 2) {
					CreateTile (free [picked].x, free [picked].y, type);
					xx++;
					free.RemoveAt (picked);
					if (free.Count <= 0) {
						break;
					}
				}
			}
		} else {
			Debug.Log ("No free spaces found. Game over.");
		}
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

	void ToggleMode(Globals.State theState){
		if (theState == Globals.State.Selected) {
			currState = theState;
			selectedTile.ToggleState (true);
		} else if (theState == Globals.State.Waiting) {
			currState = theState;
			selectedTile.ToggleState (false);
			selectedTile = null;
		}
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
					BackgroundTile bg = null;
					if (xx % 2 == 0) {
						if (yy % 2 == 0) {
							bg = Instantiate (GlobalData.Instance.bg);
						} else {
							bg = Instantiate (GlobalData.Instance.bg2);
						}
					} else {
						if (yy % 2 == 0) {
							bg = Instantiate (GlobalData.Instance.bg2);
						} else {
							bg = Instantiate (GlobalData.Instance.bg);
						}
					}
					bg.transform.position = myPos;
					bg.pos.x = xx;
					bg.pos.y = yy;
				}
			}
		}
		for (int xx = 0; xx < startingTiles;) {
			int randX = Random.Range (0, xSize);
			int randY = Random.Range (0, ySize);
			int type = Random.Range (0, GlobalData.Instance.colors.Length);
			if (tiles [randX, randY] == null) {
				CreateTile (randX, randY, type);
				xx++;
			}
		}
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

	void CreateTile(int posX, int posY, int type){
		Tile newTile = Instantiate(GlobalData.Instance.tile);
		newTile.transform.position = PosToVector2(posX, posY);
		tiles [posX, posY] = newTile;
		newTile.pos.x = posX;
		newTile.pos.y = posY;
		newTile.type = type;
		newTile.render.color = GlobalData.Instance.colors [newTile.type].colors [0];
		if (!allTiles.Contains (newTile)) {
			allTiles.Add (newTile);
		}
		StartCoroutine (PopInTile (newTile));
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
