using Godot;
using System;

public partial class PieceController : Node {
	[Export] float arr, das, sdf;
	[Export] Timer arrTimer, dasTimer, sdTimer;
	[Export] public int sdGrace;
	public int sd;
	bool autoRepeat;
	bool right;

	public Piece piece;

	Main main;

    public override void _Ready() {
        main = (Main)GetParent();
    }

    public override void _Process(double delta) {
		ManageInput();
	}

	public void ManageInput() {
		if (Input.IsActionJustPressed("Move Left")) {
			autoRepeat = false;
			piece.ShiftL();
			right = false;
			dasTimer.Start(das / 1000);
		}
		if (Input.IsActionJustPressed("Move Right")) {
			autoRepeat = false;
			piece.ShiftR();
			right = true;
			dasTimer.Start(das / 1000);
		}
		if (Input.IsActionJustPressed("R Rotate")) { piece.RotateR(); sd = sdGrace; }
		if (Input.IsActionJustPressed("L Rotate")) { piece.RotateL(); sd = sdGrace; }
		if (Input.IsActionJustPressed("H Rotate")) { piece.RotateH(); sd = sdGrace; }

		if (Input.IsActionJustPressed("Soft Drop")) {
			if (piece.ShiftD()) main.IncreaseScore(1);
			else {
				if (sd <= 0) {
					piece.Drop();
					main.GetNextPiece();
				}
				else sd--;
			}
			sdTimer.Start(1 / (2 * sdf));
		}
		if (Input.IsActionJustPressed("Hard Drop")) {
			// Drop that thug shaker (might not work)
			int score = piece.HardDrop() * 2;
			main.GetNextPiece();
			main.IncreaseScore(score);
		}

		if (Input.IsActionJustPressed("Hold")) {
			main.Hold();
		}
	}

	public void DasTimerDone() {
		if (!right && Input.IsActionPressed("Move Left")) { piece.ShiftL(); }
		if (right && Input.IsActionPressed("Move Right")) { piece.ShiftR(); }
		arrTimer.Start(arr / 1000);
		autoRepeat = true;
	}

	public void ArrTimerDone() {
		if (!autoRepeat) return;
		if (!right && Input.IsActionPressed("Move Left")) {
			piece.ShiftL();
			arrTimer.Start(arr / 1000);
		}
		if (right && Input.IsActionPressed("Move Right")) {
			piece.ShiftR();
			arrTimer.Start(arr / 1000);
		}
	}

	public void SdTimerDone() {
		if (Input.IsActionPressed("Soft Drop")) {
			if (piece.ShiftD()) main.IncreaseScore(1);
			else {
				if (sd <= 0) {
					piece.Drop();
					main.GetNextPiece();
				}
				else sd--;
			}
			sdTimer.Start(1 / (2 * sdf));
		}
	}
}