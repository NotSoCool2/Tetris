using Godot;

[GlobalClass]
public partial class S_Piece : Piece {
    public override (int, int)[,] rotations {
        get {
            return new (int y, int x)[,] { 
                { (0, 0), (-1, 0), (0, -1), (1, -1) }, // Rotation 0
                { (0, 0), (0, -1), (1, 0), (1, 1) }, // Rotation 1 (R)
                { (0, 0), (1, 0), (0, 1), (-1, 1) }, // Rotation 2
                { (0, 0), (0, 1), (-1, 0), (-1, -1)} // Rotation 3 (L)
            };
        }
    }
    public override (int, int)[,,] kickData {
        get {
            return new (int x, int y)[,,] {
                { // Rotation 0
                    { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) }, // CW (3/L -> 0)
                    { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) } // CCW (1/R -> 0)
                },
                { // Rotation 1
                    { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) }, // CW (0 -> 1/R)
                    { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) } // CCW (2 -> 1/R)
                },
                { // Rotation 2
                    { (0, 0), (+1, 0), (+1, 1), (0, -2), (1, -2) }, // CW (1/R -> 2)
                    { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) } // CCW (3/L -> 2)
                },
                { // Rotation 3
                    { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) }, // CW (2 -> 3/L)
                    { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) } // CCW (0 -> 3/L)
                }
            };
        }
    }
    public override Vector2 gridOffset {
        get {
            return new Vector2();
        }
    }
}