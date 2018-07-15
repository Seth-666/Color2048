using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {

	public enum State{
		Moving,
		Waiting,
		Selected,
		Busy,
		Paused
	}

	public enum Direction{
		Up,
		Down,
		Left,
		Right,
		UpLeft,
		DownLeft,
		UpRight,
		DownRight
	}
		
	public struct CoordSet{
		public List<Coord> coords;
	}

	public static List<CoordSet> GetSquareSets(Coord start){
		List<CoordSet> ret = new List<CoordSet> ();
		CoordSet a = new CoordSet ();
		a.coords = new List<Coord> ();
		a.coords.Add (start);
		a.coords.Add (new Coord (start.x - 1, start.y + 1));
		a.coords.Add(new Coord(start.x, start.y + 1));
		a.coords.Add (new Coord (start.x - 1, start.y));

		CoordSet b = new CoordSet ();
		b.coords = new List<Coord> ();
		b.coords.Add (start);
		b.coords.Add (new Coord (start.x, start.y + 1));
		b.coords.Add (new Coord (start.x + 1, start.y + 1));
		b.coords.Add (new Coord (start.x + 1, start.y));

		CoordSet c = new CoordSet ();
		c.coords = new List<Coord> ();
		c.coords.Add (start);
		c.coords.Add (new Coord (start.x - 1, start.y));
		c.coords.Add (new Coord (start.x - 1, start.y - 1));
		c.coords.Add (new Coord (start.x, start.y - 1));

		CoordSet d = new CoordSet ();
		d.coords = new List<Coord> ();
		d.coords.Add (start);
		d.coords.Add (new Coord (start.x + 1, start.y));
		d.coords.Add (new Coord (start.x, start.y - 1));
		d.coords.Add (new Coord (start.x + 1, start.y - 1));

		ret.Add (a);
		ret.Add (b);
		ret.Add (c);
		ret.Add (d);
		return ret;
	}

	public static List<CoordSet> GetHorizSquares(Coord start){
		List<CoordSet> ret = new List<CoordSet> ();
		CoordSet a = new CoordSet ();
		a.coords = new List<Coord> ();
		a.coords.Add (start);
		a.coords.Add (new Coord (start.x + 1, start.y));
		a.coords.Add (new Coord (start.x + 2, start.y));
		a.coords.Add (new Coord (start.x, start.y - 1));
		a.coords.Add (new Coord (start.x + 1, start.y - 1));
		a.coords.Add (new Coord (start.x + 2, start.y - 1));

		CoordSet b = new CoordSet ();
		b.coords = new List<Coord> ();
		b.coords.Add (start);
		b.coords.Add (new Coord (start.x - 1, start.y));
		b.coords.Add (new Coord (start.x + 1, start.y));
		b.coords.Add (new Coord (start.x - 1, start.y - 1));
		b.coords.Add (new Coord (start.x, start.y - 1));
		b.coords.Add (new Coord (start.x + 1, start.y - 1));

		CoordSet c = new CoordSet ();
		c.coords = new List<Coord> ();
		c.coords.Add (start);
		c.coords.Add (new Coord (start.x - 2, start.y));
		c.coords.Add (new Coord (start.x - 1, start.y));
		c.coords.Add (new Coord (start.x - 2, start.y - 1));
		c.coords.Add (new Coord (start.x - 1, start.y - 1));
		c.coords.Add (new Coord (start.x, start.y - 1));

		CoordSet d = new CoordSet ();
		d.coords = new List<Coord> ();
		d.coords.Add (start);
		d.coords.Add (new Coord (start.x, start.y + 1));
		d.coords.Add (new Coord (start.x + 1, start.y + 1));
		d.coords.Add (new Coord (start.x + 2, start.y + 1));
		d.coords.Add (new Coord (start.x + 1, start.y));
		d.coords.Add (new Coord (start.x + 2, start.y));

		CoordSet e = new CoordSet ();
		e.coords = new List<Coord> ();
		e.coords.Add (start);
		e.coords.Add (new Coord (start.x - 1, start.y + 1));
		e.coords.Add (new Coord (start.x, start.y + 1));
		e.coords.Add (new Coord (start.x + 1, start.y + 1));
		e.coords.Add (new Coord (start.x - 1, start.y));
		e.coords.Add (new Coord (start.x + 1, start.y));

		CoordSet f = new CoordSet ();
		f.coords = new List<Coord> ();
		f.coords.Add (start);
		f.coords.Add (new Coord (start.x - 2, start.y + 1));
		f.coords.Add (new Coord (start.x - 1, start.y + 1));
		f.coords.Add (new Coord (start.x, start.y + 1));
		f.coords.Add (new Coord (start.x - 2, start.y));
		f.coords.Add (new Coord (start.x - 1, start.y));

		ret.Add (a);
		ret.Add (b);
		ret.Add (c);
		ret.Add (d);
		ret.Add (e);
		ret.Add (f);
		return ret;
	}

	public static List<CoordSet> GetVertSquares(Coord start){
		List<CoordSet> ret = new List<CoordSet> ();
		CoordSet a = new CoordSet ();
		a.coords = new List<Coord> ();
		a.coords.Add (start);
		a.coords.Add (new Coord (start.x + 1, start.y));
		a.coords.Add (new Coord (start.x, start.y - 1));
		a.coords.Add (new Coord (start.x + 1, start.y - 1));
		a.coords.Add (new Coord (start.x, start.y - 2));
		a.coords.Add (new Coord (start.x + 1, start.y - 2));

		CoordSet b = new CoordSet ();
		b.coords = new List<Coord> ();
		b.coords.Add (start);
		b.coords.Add (new Coord (start.x, start.y + 1));
		b.coords.Add (new Coord (start.x + 1, start.y + 1));
		b.coords.Add (new Coord (start.x + 1, start.y));
		b.coords.Add (new Coord (start.x, start.y - 1));
		b.coords.Add (new Coord (start.x + 1, start.y - 1));

		CoordSet c = new CoordSet ();
		c.coords = new List<Coord> ();
		c.coords.Add (start);
		c.coords.Add (new Coord (start.x, start.y + 2));
		c.coords.Add (new Coord (start.x + 1, start.y + 2));
		c.coords.Add (new Coord (start.x, start.y + 1));
		c.coords.Add (new Coord (start.x + 1, start.y + 1));
		c.coords.Add (new Coord (start.x + 1, start.y));

		CoordSet d = new CoordSet ();
		d.coords = new List<Coord> ();
		d.coords.Add (start);
		d.coords.Add (new Coord (start.x - 1, start.y));
		d.coords.Add (new Coord (start.x - 1, start.y - 1));
		d.coords.Add (new Coord (start.x, start.y - 1));
		d.coords.Add (new Coord (start.x - 1, start.y - 2));
		d.coords.Add (new Coord (start.x, start.y - 2));

		CoordSet e = new CoordSet ();
		e.coords = new List<Coord> ();
		e.coords.Add (start);
		e.coords.Add (new Coord (start.x - 1, start.y + 1));
		e.coords.Add (new Coord (start.x, start.y + 1));
		e.coords.Add (new Coord (start.x - 1, start.y));
		e.coords.Add (new Coord (start.x - 1, start.y - 1));
		e.coords.Add (new Coord (start.x, start.y - 1));

		CoordSet f = new CoordSet ();
		f.coords = new List<Coord> ();
		f.coords.Add (start);
		f.coords.Add (new Coord (start.x - 1, start.y + 2));
		f.coords.Add (new Coord (start.x, start.y + 2));
		f.coords.Add (new Coord (start.x - 1, start.y + 1));
		f.coords.Add (new Coord (start.x, start.y + 1));
		f.coords.Add (new Coord (start.x - 1, start.y));

		ret.Add (a);
		ret.Add (b);
		ret.Add (c);
		ret.Add (d);
		ret.Add (e);
		ret.Add (f);
		return ret;
	}

	public static Globals.Coord AddDirection(Coord orig, Direction dir){
		Globals.Coord ret = orig;
		if (dir == Direction.Left) {
			ret.x -= 1;
		}
		else if (dir == Direction.Right) {
			ret.x += 1;
		}
		else if (dir == Direction.Up) {
			ret.y += 1;
		}
		else if (dir == Direction.Down) {
			ret.y -= 1;
		}
		else if (dir == Direction.UpLeft) {
			ret.x -= 1;
			ret.y += 1;
		}
		else if (dir == Direction.UpRight) {
			ret.x += 1;
			ret.y += 1;
		}
		else if (dir == Direction.DownLeft) {
			ret.x -= 1;
			ret.y -= 1;
		}
		else if (dir == Direction.DownRight) {
			ret.x += 1;
			ret.y -= 1;
		}
		return ret;
	}

	[System.Serializable]
	public struct Coord{
		public int x;
		public int y;

		public Coord(int xx, int yy){
			x = xx;
			y = yy;
		}

	}

	[System.Serializable]
	public struct ColorSet{
		public Color[] colors;
	}

	//Generic method for turning a 2D array into a 1D array.
	public static T[] SaveArray<T>(T[,] data){
		int xSize = data.GetLength (0);
		int ySize = data.GetLength (1);
		T[] arr = new T[xSize * ySize];
		int currIndex = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				arr [currIndex] = data [xx, yy];
				currIndex++;
			}
		}
		return arr;
	}

	//Generic method for returning a 1D array to a 2D array, based on dimensions provided.
	public static T[,] LoadArray<T>(T[] arr, int xSize, int ySize){
		T[,] ret = new T[xSize, ySize];
		int currIndex = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				ret [xx, yy] = arr [currIndex];
				currIndex++;
			}
		}
		return ret;
	}

}
