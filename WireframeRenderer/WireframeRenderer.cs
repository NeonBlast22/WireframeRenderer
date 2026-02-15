namespace WireframeRenderer;

public static class WireframeRenderer
{
    private static int width = 1920;
    private static int height = 1080;
    static int Main()
    {
        Screen.OnStart += Start;
        Screen.OnUpdate += Update;
        
        Screen.Initialize(width, height, "WireframeRenderer");
        
        return 0;
    }

    static void Start()
    {
        Screen.ClearScreen();
    }
    
    static void Update(double deltaTime)
    {
        Screen.ClearScreen();
        // Create a gradient
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = new Color((float)x / width, (float)y / width, 0);
                Screen.SetPixel(x, y, color); 
            }
        }
        Console.WriteLine($"FPS: {Math.Round(1f / deltaTime, 1)}");
    }
}