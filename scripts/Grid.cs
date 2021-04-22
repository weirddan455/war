using Godot;

public class Grid : Node2D
{
    private const int NUM_CELLS_X = 16;
    private const int NUM_CELLS_Y = 9;
    private const int CELL_SIZE = 64;

    private Rect2 _highlightedCell = new Rect2(0, 0, CELL_SIZE, CELL_SIZE);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent)
        {
            Vector2 mouseCell = new Vector2((int)mouseEvent.Position.x / CELL_SIZE * CELL_SIZE, (int)mouseEvent.Position.y / CELL_SIZE * CELL_SIZE);
            if (_highlightedCell.Position != mouseCell)
            {
                _highlightedCell.Position = mouseCell;
                Update();
            }
        }
    }

    public override void _Draw()
    {
        for (int i = 0; i < NUM_CELLS_X - 1; i++)
        {
            DrawLine(new Vector2(CELL_SIZE - 1 + i * CELL_SIZE, 0), new Vector2(CELL_SIZE - 1 + i * CELL_SIZE, CELL_SIZE * NUM_CELLS_Y), Colors.Black, 2, false);
        }
        for (int i = 0; i < NUM_CELLS_Y - 1; i++)
        {
            DrawLine(new Vector2(0, CELL_SIZE - 1 + i * CELL_SIZE), new Vector2(CELL_SIZE * NUM_CELLS_X, CELL_SIZE - 1 + i * CELL_SIZE), Colors.Black, 2, false);
        }
        DrawRect(_highlightedCell, Colors.Red, false, 2, false);
    }
}
