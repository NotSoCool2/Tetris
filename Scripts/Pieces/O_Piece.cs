using Godot;

[GlobalClass]
public partial class O_Piece : Piece {
    public override (int, int)[,] rotations {
        get {
            return new (int y, int x)[,] { // Treat the top left corner of piece to be 0, 0
                { (0, 0), (1, 0), (1, 1), (0, 1) }, // Rotation 0
                { (1, 0), (1, 1), (0, 1), (0, 0) }, // Rotation 1 (R)
                { (1, 1), (0, 1), (0, 0), (1, 0) }, // Rotation 2
                { (0, 1), (0, 0), (1, 0), (1, 1)} // Rotation 3 (L)
            };
        }
    }
    public override (int, int)[,,] kickData {
        get {
            return new (int x, int y)[,,] {
                { // Rotation 0
                    { },
                    { }
                },
                { // Rotation 1
                    {  },
                    {  }
                },
                { // Rotation 2
                    {  },
                    {  }
                },
                { // Rotation 3
                    {  },
                    {  }
                }
            };
        }
    }
    public override Vector2 gridOffset {
        get {
            return new Vector2(-.5f, .5f);
        }
    }
}