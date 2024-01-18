using Godot;

[GlobalClass]
public partial class Tile : Node2D {
	Grid grid;
	public (int x, int y) pos;
	public Node2D tileObj;
	public Tile(Grid g, Node2D obj, (int, int) p) {
		grid = g;
		pos = p;
		tileObj = obj;
		if (!grid.PlaceInGrid(this, p)) {
			GD.PrintErr($"Can't Spawn a tile on ({pos.x}, {pos.y})");
			GD.Print(GetParent());
		}
	}

	public Tile((int, int) p) { // Scan Tile (ghost tiles)
		pos = p;
	}

	public void ShiftL() {
		if (pos.x <= 0) return;
		pos.x -= 1;
	}

	public void ShiftR() {
		if (pos.x >= grid.GetWidth() - 1) return;
		pos.x += 1;
	}

	public void ShiftD() {
		if (pos.y <= 0) return;
		pos.y -= 1;
	}

	public void ShiftU() {
		if (pos.y >= grid.GetHeight() - 1) return;
		pos.y += 1;
	}

	public bool canShiftL() { return grid.PosAvailable((pos.x - 1, pos.y)); }
	public bool canShiftR() { return grid.PosAvailable((pos.x + 1, pos.y)); }
	public bool canShiftD() { return grid.PosAvailable((pos.x, pos.y - 1)); }
	public bool canShiftU() { return grid.PosAvailable((pos.x, pos.y + 1)); }

	public bool canSetPos((int, int) pos) {return grid.PosAvailable(pos); }
}