using Godot;

public class PathOverlay : TileMap
{
    public override void _Ready()
    {
        Image image = new Image();
        image.Create(64, 64, false, Image.Format.Rgba8);
        image.Fill(new Color(1, 1, 0, 0.5f));
        ImageTexture imageTexture = new ImageTexture();
        imageTexture.CreateFromImage(image);
        TileSet = new TileSet();
        TileSet.CreateTile(0);
        TileSet.TileSetTexture(0, imageTexture);
    }

    public void DrawOverlay(Godot.Collections.Dictionary cells)
    {
        foreach(System.Collections.DictionaryEntry entry in cells)
        {
            SetCellv((Vector2)entry.Key, 0);
        }
    }
}
