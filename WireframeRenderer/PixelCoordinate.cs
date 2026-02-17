namespace WireframeRenderer;

public class PixelCoordinate
{
    public int x;
    public int y;
    
    public PixelCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public PixelCoordinate(Vector2 clipspaceCoordinate)
    {
        x = (int)MathF.Round(clipspaceCoordinate.x * WireframeRenderer.screenHeight * 0.5f);
        x += (WireframeRenderer.screenWidth / 2);
        
        y = (int)MathF.Round(clipspaceCoordinate.y * WireframeRenderer.screenHeight * 0.5f);
        y += (WireframeRenderer.screenHeight / 2);
    }
}