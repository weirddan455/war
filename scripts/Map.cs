using Godot;

public class Map : TileMap
{
    public bool IsInsideMap(Vector2 cellPosition)
    {
        Rect2 rect = GetUsedRect();
        if (cellPosition.x < 0 || cellPosition.y >= rect.Size.x || cellPosition.y < 0 || cellPosition.y >= rect.Size.y)
        {
            return false;
        }
        return true;
    }

    public Vector2 MapToWorldCenter(Vector2 position)
    {
        return MapToWorld(position) + CellSize / 2;
    }
}
