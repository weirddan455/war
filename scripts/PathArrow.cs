using Godot;

public class PathArrow : TileMap
{
    public void DrawArrow(Vector2 cell)
    {
        SetCellv(cell, 0);
        UpdateBitmaskArea(cell);
    }
}
