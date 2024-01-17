using Godot;

[GlobalClass]
public partial class Grid : Node2D {
	Tile[][] grid; // 50 because if you stack to the top on the left then get filled with garbage you dont go out of bounds
	int width, height;

	[Export] public int tileSize = 32;

	public Grid(int x, int y) {
		width = x;
		height = y;
		grid = new Tile[y][];
		for (int i = 0; i < y; i++) {
			grid[i] = new Tile[x]; // Indexing backwards because its easier to move an array down 1 instead indexing in each array and bringing it down for tetris
		}
	}
	public Grid() {
		width = 10;
		height = 50;
		grid = new Tile[50][];
		for (int i = 0; i < 50; i++) {
			grid[i] = new Tile[10];
		}
	}

	public int GetWidth() {
		return width;
	}

	public int GetHeight() {
		return height;
	}

	public Vector2 TranslateToRealPos((int x, int y) p) {
		return new Vector2I(p.x ,p.y) * tileSize;
	}

	public bool PosAvailable((int x, int y) pos) {
		if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) return false;
		return grid[pos.y][pos.x] == null;
	}

	public Tile TileAt((int x, int y) pos) {
		if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) return null;
		return grid[pos.y][pos.x];
	}

	public bool PlaceInGrid(Tile t, (int x, int y) pos) {
		if (grid[pos.y][pos.x] != null) return false;
		grid[pos.y][pos.x] = t;
		return true;
	}

	public void RemoveInGrid((int x, int y) pos) {
		grid[pos.y][pos.x] = null;
	}

	public (int, int) MoveInGrid(Tile t, (int x, int y) newPos) {
		if (t.pos.x < 0 || t.pos.x >= width || t.pos.y < 0 || t.pos.y >= height) return (t.pos.y, t.pos.y);
		if (TileAt(t.pos) == t) grid[t.pos.y][t.pos.x] = null;
		grid[newPos.y][newPos.x] = t;
		return newPos;
	}

	public void ClearLine(int index) {
		for (int i = 0; i < width; i++) {
			grid[index][i].tileObj.QueueFree();
		}
		for (int i = index; i > 0; i--) {
			grid[i] = grid[i - 1];
			for (int j = 0; j < width; j++) {
				Tile cur = grid[i][j];
				if (cur != null) {
					cur.pos = (cur.pos.x, cur.pos.y + 1);
					cur.tileObj.Position += Vector2.Down * tileSize;
				}
			}
		}
		grid[0] = new Tile[width];
	}
}