using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
	// General Variables
	[Export] Vector2 holdPiecePos;
	[Export] Node2D gridObj;
	[Export] Timer fallTimer;
	[Export] double fallTime;
	[Export] int graceFalls;
	Vector2[] nextPiecePoints = new Vector2[5];
	public PackedScene garbageTile;
	public Piece holdPiece = null;
	Grid grid = new(10, 50);
	PackedScene[] pieceObjects = new PackedScene[7];
	Piece[] next = new Piece[5];
	Queue<int> pieceBag;
	PieceController controller;
	Random rand = new Random();

	// Score and level variables
	Label scoreLabel;
	Label levelLabel;
	int score = 0;
	int level = 1;
	int lineCount = 0;
	enum backToBack { TETRIS, TSPIN, NONE }
	backToBack b2b;
	int b2bCount = -1;

	// Game variables
	int grace;
	Dictionary<int, double> dropSpeeds = new Dictionary<int, double> {
		{ 0, 48/60.0 },
		{ 1, 43/60.0 },
		{ 2, 38/60.0 },
		{ 3, 33/60.0 },
		{ 4, 28/60.0 },
		{ 5, 23/60.0 },
		{ 6, 18/60.0 },
		{ 7, 13/60.0 },
		{ 8, 8/60.0 },
		{ 9, 6/60.0 },
		{ 10, 5/60.0 },
		{ 13, 4/60.0 },
		{ 16, 3/60.0 },
		{ 19, 2/60.0 },
		{ 29, 1/60.0 },
	};

	public override void _Ready() {
		for (int i = 0; i < next.Length; i++) {
			nextPiecePoints[i] = ((Node2D)FindChild("Next")).Position + new Vector2(0, i * 96);
		}

		controller = (PieceController)FindChild("PieceController");
		scoreLabel = (Label)FindChild("Score");
		levelLabel = (Label)FindChild("Level");

		InitPieces();
		StartGame();
	}

    public override void _Process(double delta) {
		if (Input.IsActionJustPressed("Reset")) {
			foreach (Piece p in next) p.QueueFree();
			foreach (Node n in gridObj.GetChildren()) n.QueueFree();
			if (holdPiece != null) { holdPiece.QueueFree(); holdPiece = null; }
			grid.EmptyGrid();
			score = 0;
			scoreLabel.Text = "Level: 0";
			level = 1;
			lineCount = 0;
			levelLabel.Text = "Level: 1\n0/10";
			StartGame();
		}
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
		grace = graceFalls;
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

		controller.sd = controller.sdGrace;
		grace = graceFalls;
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

			grace = graceFalls;
		}
		else { // Handles case for first held piece
			holdPiece = controller.piece;
		}

		// This is mostly a bunch of parent swapping, very suboptimal but whateva
		holdPiece.GetParent().RemoveChild(holdPiece);
		AddChild(holdPiece);
		holdPiece.Position = holdPiecePos;
		holdPiece.Hold();

		if (isFirst) GetNextPiece();
		else SpawnPiece();

		controller.piece.GetParent().RemoveChild(controller.piece);
		gridObj.AddChild(controller.piece);
		controller.piece.DisplayGhostPiece();

	}

	public void CheckLines(bool t) {
		int linesCleared = 0;
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
				linesCleared++;
			}
		}

		lineCount += linesCleared;
		IncreaseLevel();

		int score = 0;
		switch (linesCleared) {
			case 0:
				if (t) score = 400;
				break;
			case 1:
				if (t) {
					score = 800;
					b2b = backToBack.TSPIN;
					b2bCount++;
					}
				else {
					score = 100;
					b2b = backToBack.NONE;
					b2bCount = -1;
				}
				break;
			case 2:
				if (t) {
					score = 1200;
					b2b = backToBack.TSPIN;
					b2bCount++;
					}
				else {
					score = 300;
					b2b = backToBack.NONE;
					b2bCount = -1;
				}
				break;
			case 3:
				if (t) {
					score = 1600;
					b2b = backToBack.TSPIN;
					b2bCount++;
					}
				else {
					score = 500;
					b2b = backToBack.NONE;
					b2bCount = -1;
				}
				break;
			case 4:
				score = 800;
				b2b = backToBack.TETRIS;
				b2bCount++;
				break;
			default:
				break;
		}
		if (b2bCount > 0) score = (int)(score * 1.5);
		IncreaseScore(score);
	}

	public void LowerPiece() {
		if (!controller.piece.ShiftD()) grace--;
		if (grace <= 0) {
			controller.piece.Drop();
			GetNextPiece();
		}
		fallTimer.Start(fallTime);
	}

	public void IncreaseScore(int s) {
		score += s * level;
		scoreLabel.Text = $"Score: {score}";
	}
	public void IncreaseLevel() {
		if (lineCount - 10 * level >= 0) {
			level++;
			dropSpeeds.TryGetValue(level, out fallTime);
			GD.Print(fallTime);
		}
		levelLabel.Text = $"Level: {level}\n{lineCount % 10}/10";

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