using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
	[Export] Vector2 holdPiecePos;
	[Export] Node2D gridObj;
	[Export] Timer fallTimer;
	[Export] double fallTime;
	Vector2[] nextPiecePoints = new Vector2[5];
	public PackedScene garbageTile;
	public Piece holdPiece = null;
	Grid grid = new(10, 50);
	PackedScene[] pieceObjects = new PackedScene[7];
	Piece[] next = new Piece[5];
	Queue<int> pieceBag;
	PieceController controller;
	Random rand = new Random();

	public override void _Ready() {
		for (int i = 0; i < next.Length; i++) {
			nextPiecePoints[i] = ((Node2D)FindChild("Next")).Position + new Vector2(0, i * 96);
		}

		controller = (PieceController)FindChild("PieceController");

		InitPieces();
		StartGame();
	}

	public void InitPieces() {
		pieceObjects[0] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/i_piece.tscn");
		pieceObjects[1] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/t_piece.tscn");
		pieceObjects[2] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/o_piece.tscn");
		pieceObjects[3] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/l_piece.tscn");
		pieceObjects[4] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/j_piece.tscn");
		pieceObjects[5] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/s_piece.tscn");
		pieceObjects[6] = ResourceLoader.Load<PackedScene>("res://Scenes/Pieces/z_piece.tscn");

		garbageTile = ResourceLoader.Load<PackedScene>("res://Scenes/garbage_tile.tscn");
	}

	public void StartGame() {
		pieceBag = GeneratePieceBag();

		for (int i = 0; i < nextPiecePoints.Length; i++) {
			int index = pieceBag.Dequeue();
			Piece newPiece = (Piece)pieceObjects[index].Instantiate();
			AddChild(newPiece);
			newPiece.Position = nextPiecePoints[i] - newPiece.gridOffset;
			next[i] = newPiece;
		}

		GetNextPiece();
		fallTimer.Start(fallTime);
	}
	
	public void GetNextPiece() {
		controller.piece = next[0]; // Set the piece at the top of the next line to be the controlled one
		for (int i = 0; i < next.Length - 1; i++) {
			next[i] = next[i + 1]; // Shift all the pieces up the next line
			next[i].Position = nextPiecePoints[i]; 
		}

		Piece newPiece = (Piece)pieceObjects[pieceBag.Dequeue()].Instantiate(); // Load the next piece from the bag
		AddChild(newPiece);
		controller.piece.GetParent().RemoveChild(controller.piece); // Place the controlled piece in the grid
		gridObj.AddChild(controller.piece);
		newPiece.Position = nextPiecePoints[next.Length - 1]; // Place the new next piece on the next line
		next[next.Length - 1] = newPiece;

		if (pieceBag.Count == 0) pieceBag = GeneratePieceBag(); // If the bag is finished gen a new one
		SpawnPiece();
	}

	public Queue<int> GeneratePieceBag() {
		Queue<int> pb = new Queue<int>();
		int[] bag = {0, 1, 2, 3, 4, 5, 6}; // Since this is a bag of 7, add all different types to the bag

		for (int i = bag.Length - 1; i > 1; i--) { // This is a array shuffler found on StackOverflow
			int k = rand.Next(i);
			int temp = bag[i];
			bag[i] = bag[k];
			bag[k] = temp;
		}
		foreach (int num in bag) pb.Enqueue(num); // Add the pieces to a queue
		return pb;
	}

	public void SpawnPiece() { // Puts the piece at the top of the grid
		controller.piece.Position = new Vector2(0, 0);
		controller.piece.Construct(grid, (4, 28));
	}

	public void Hold() {
		bool isFirst = holdPiece == null;
		if (!isFirst) { // Default case for holding
			Piece temp = holdPiece;
			holdPiece = controller.piece;
			controller.piece = temp;
		}
		else { // Handles case for first held piece
			holdPiece = controller.piece;
		}

		// This is mostly a bunch of parent swapping, very suboptimal but whateva
		holdPiece.GetParent().RemoveChild(holdPiece);
		AddChild(holdPiece);
		holdPiece.Position = holdPiecePos;
		holdPiece.Hold();
		controller.piece.GetParent().RemoveChild(controller.piece);
		gridObj.AddChild(controller.piece);
		controller.piece.DisplayGhostPiece();

		if (isFirst) GetNextPiece();
		else SpawnPiece();
	}

	public void CheckLines() {
		for (int i = 0; i < grid.GetHeight(); i++) {
			bool lineFull = true;
			for (int j = 0; j < grid.GetWidth(); j++) {
				if (grid.PosAvailable((j, i))) {
					lineFull = false;
					break;
				}
			}
			if (lineFull) {
				grid.ClearLine(i);
			}
		}
	}

	public void LowerPiece() {
		controller.piece.ShiftD();
		fallTimer.Start(fallTime);
	}
	
	public void DebugGridInformation() {
		Node shitHolder = FindChild("ShitHolder");
		for (int i = 0; i < shitHolder.GetChildCount(); i++) {
			shitHolder.GetChild(i).QueueFree();
		}
		for (int i = 0; i < grid.GetWidth(); i++) {
			for (int j = 0; j < grid.GetHeight(); j++) {
				if (!grid.PosAvailable((i, j))) {
					Node2D tile = (Node2D)garbageTile.Instantiate();
					shitHolder.AddChild(tile);
					tile.Position = grid.TranslateToRealPos((i, j));
				}
			}
		}
	}
}