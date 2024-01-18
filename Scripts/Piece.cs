using Godot;
using System;
using System.Linq;

[GlobalClass]
public abstract partial class Piece : Node2D {
	// Variables
	public int rotation = 0;
	public (int x, int y) pos;
	public Grid grid;
	public Tile[] tiles;
	public GhostPiece ghostPiece;
	public PackedScene ghostPieceObj = ResourceLoader.Load<PackedScene>("res://Scenes/ghost_piece.tscn");
	public abstract (int x, int y)[,] rotations { get; }
	public abstract (int x, int y)[,,] kickData { get; }
	public abstract Vector2 gridOffset {get; }

	public void Construct(Grid g, (int x, int y) pos) {
		grid = g;
		tiles = new Tile[4];
		this.pos = pos;
		for (int i = 0; i < tiles.Length; i++) {
			tiles[i] = new Tile(grid, (Node2D)GetChildren()[i], (
				pos.x + rotations[0, i].x,
				pos.y + rotations[0, i].y
			));
		}
		DisplayMovement();
	}
	
	public virtual void RotateL() {
		rotation -= 1;
        rotation = (rotation + 4) % 4;
        if (!canRotate(false)) {
            rotation += 1;
            rotation = (rotation + 4) % 4;
			return;
        }
		DisplayMovement();
	}

	public virtual void RotateR() {
		rotation += 1;
        rotation = (rotation + 4) % 4;
		
        if (!canRotate(true)) {
            rotation -= 1;
            rotation = (rotation + 4) % 4;
			return;
        }
		DisplayMovement();
	}
	
	public void RotateH() { // Doesnt do any kicking, just kinda acts as a 180 flip and sees if it can be placed
		rotation += 2;
		rotation = rotation % 4;

		bool posValid = true;
		for (int i = 0; i < tiles.Length; i++) {
			posValid = tiles[i].canSetPos(rotations[rotation, i]);
			if (!posValid) break;
		}
		if (posValid) {
			for (int i = 0; i < tiles.Length; i++) {
				tiles[i].pos = grid.MoveInGrid(tiles[i], (
					pos.x + rotations[rotation, i].x,
					pos.y + rotations[rotation, i].y
				));
			}
		}
		else {
			rotation += 2;
			rotation = rotation % 4;
		}
		DisplayMovement();
	}

	public bool canRotate(bool cw) {
        int r = cw ? 0 : 1;
		bool posValid = true;
        for (int i = 0; i < kickData.GetLength(2); i++) {
            for (int j = 0; j < tiles.Length; j++) {
				posValid = true;
				(int x, int y) checkedPos = (
                    pos.x + rotations[rotation, j].x + kickData[rotation, r, i].x,
                    pos.y + rotations[rotation, j].y + kickData[rotation, r, i].y
                );
                if (!grid.PosAvailable(checkedPos)) posValid = OccupyingTile(checkedPos);
                if (!posValid) break;
            }
            if (posValid) {
				for (int j = 0; j < tiles.Length; j++) {
					tiles[j].pos = grid.MoveInGrid(tiles[j], (
						pos.x + rotations[rotation, j].x + kickData[rotation, r, i].x,
						pos.y + rotations[rotation, j].y + kickData[rotation, r, i].y
					));
				}
				pos = (pos.x + kickData[rotation, r, i].x, pos.y + kickData[rotation, r, i].y);
				return true;
			}
        }
        return false;
    }

	public void ShiftL() {
		if (Move((-1, 0))) {
			pos = (pos.x - 1, pos.y);
			DisplayMovement();
		}
	}
	public void ShiftR() {
		if (Move((1, 0))) {
			pos = (pos.x + 1, pos.y);
			DisplayMovement();
		}
	}
	public bool ShiftD() {
		if (Move((0, 1))) {
			pos = (pos.x, pos.y + 1);
			DisplayMovement();
			return true;
		}
		return false;
	}
	public int HardDrop() { // Returns how many steps it took
		int counter = 0;
		while (Move((0, 1))) {
			pos = (pos.x, pos.y + 1);
			counter++;
		}
		DisplayMovement();
		Drop();
		return counter;
	}

	public virtual void Drop() {
		for (int i = 0; i < tiles.Length; i++) {
			RemoveChild(tiles[i].tileObj);
			GetParent().AddChild(tiles[i].tileObj);
		}
		((Main)FindParent("Main")).CheckLines(false);
		ghostPiece.QueueFree();
		ghostPiece = null;
		QueueFree();
	}

	public void Hold() {
		rotation = 0;
		for (int i = 0; i < tiles.Length; i++) {
			grid.RemoveInGrid(tiles[i].pos);
			tiles[i].pos = rotations[0, i];
			tiles[i].tileObj.Position = new Vector2(rotations[0, i].x, rotations[0, i].y) * grid.tileSize;
		}
		ghostPiece.Visible = false;
	}

	public void DisplayMovement() {
		for (int i = 0; i < tiles.Length; i++) {
			tiles[i].tileObj.Position = grid.TranslateToRealPos(tiles[i].pos);
		}
		DisplayGhostPiece();
	}

	public void DisplayGhostPiece() {
		if (ghostPiece == null) {
			ghostPiece = (GhostPiece)ghostPieceObj.Instantiate();
			GetParent().AddChild(ghostPiece);
			ghostPiece.Construct(grid, this);
		}
		ghostPiece.Visible = true;
		ghostPiece.Place(tiles);
	}

	public bool OccupyingTile((int x, int y) pos) { // Checks true if the occupied tile is being occupied by this piece
		return tiles.Contains(grid.TileAt(pos));
	}

	bool Move((int x, int y) delta) {
		for (int i = 0; i < tiles.Length; i++) {
			(int x, int y) checkedPos = (
				tiles[i].pos.x + delta.x,
				tiles[i].pos.y + delta.y
			);
			if (!grid.PosAvailable(checkedPos)) {
				if (!OccupyingTile(checkedPos)) return false;
			}
		}
		for (int i = 0; i < tiles.Length; i++) {
			tiles[i].pos = grid.MoveInGrid(tiles[i], (
				tiles[i].pos.x + delta.x,
				tiles[i].pos.y + delta.y
			));
		}
		return true;
	}
}