using Godot;
using System;

// Make a piece that sits at the bottom, maintains the same rotation
// Slide the piece down (+y) until its in an invalid position
public partial class GhostPiece : Node2D {
	Grid grid;
	Piece piece;
	Tile[] tiles = new Tile[4];
	PackedScene ghostTile = ResourceLoader.Load<PackedScene>("res://Scenes/ghost_tile.tscn");

	public void Construct(Grid g, Piece p) {
		grid = g;
		piece = p;
	}
	public void Place(Tile[] ts) {
		for (int i = 0; i < tiles.Length; i++) { // Get the positions of the piece tiles
			if (tiles[i] == null) tiles[i] = new(ts[i].pos);
			else tiles[i].pos = ts[i].pos;
		}
		bool posValid = true;
		while (true) { // Lower them until they collide with something on the grid
			for (int i = 0; i < tiles.Length; i++) {
				if (!grid.PosAvailable((tiles[i].pos.x, tiles[i].pos.y + 1))) {
					if (!piece.OccupyingTile((tiles[i].pos.x, tiles[i].pos.y + 1))) {
						posValid = false;
						break;				
					}
				}
			}
			if (!posValid) break;
			for (int i = 0; i < tiles.Length; i++) { // Set their positions
				tiles[i].pos = (tiles[i].pos.x, tiles[i].pos.y + 1);
			}
		}
		for (int i = 0; i < tiles.Length; i++) { // place the tile objects 
			if (tiles[i].tileObj != null) {
				tiles[i].tileObj.QueueFree();
			}
			tiles[i].tileObj = (Node2D)ghostTile.Instantiate();
			AddChild(tiles[i].tileObj);
			tiles[i].tileObj.Position = grid.TranslateToRealPos(tiles[i].pos);
		}
	}
}
