namespace WireframeRenderer;

public static class WireframeRenderer
{
    public static int screenWidth = 1280;
    public static int screenHeight = 720;
    private const float FOV = 2;

    private static WireframeModel cube;
    
    static int Main()
    {
        Screen.OnStart += Start;
        Screen.OnUpdate += Update;
        
        Screen.Initialize(screenWidth, screenHeight, "WireframeRenderer");
        
        return 0;
    }

    static void Start()
    {
        Screen.ClearScreen();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1)
        };

        int[] connections = new int[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            
            0, 4,
            1, 5,
            2, 6,
            3, 7,
            
            4, 5,
            5, 6,
            6, 7,
            7, 4,
        };
        
        cube = new WireframeModel(vertices, connections);
    }

    private static float yaw = 0f;
    static void Update(double deltaTime)
    {
        Screen.ClearScreen();
        yaw += (float)deltaTime;
        Matrix4X4 rotation = Matrix4X4.FromRotation(0f, yaw, 0f);
        Matrix4X4 translation = Matrix4X4.FromTranslation(0f, 0f, 10f);
        
        RenderWireframe(cube, translation * rotation);
        
        Console.WriteLine($"FPS: {Math.Round(1f / deltaTime, 1)}");
    }

    static Vector2 ProjectCameraToClipspace(Vector3 vertexPosition)
    {
        float x = (vertexPosition.x * FOV) / (FOV + vertexPosition.z);
        float y = (vertexPosition.y * FOV) / (FOV + vertexPosition.z);
        Vector2 clip = new Vector2(x, y);
        return clip;
    }

    static void DrawLine(PixelCoordinate start, PixelCoordinate end, Color color)
    {
        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);
        int sx = (start.x < end.x) ? 1 : -1;
        int sy = (start.y < end.y) ? 1 : -1;
        int err = dx - dy;

        int x = start.x;
        int y = start.y;

        while (true)
        {
            Screen.SetPixel(x, y, color);

            if (x == end.x && y == end.y)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    static void RenderWireframe(WireframeModel model, Matrix4X4 transformation)
    {
        Vector2[] clipSpaceVertexCoordinates = new Vector2[model.vertices.Length];
        for (int vertexIndex = 0; vertexIndex < model.vertices.Length; vertexIndex++)
        {
            Vector3 transformedVertex = model.vertices[vertexIndex] * transformation;
            clipSpaceVertexCoordinates[vertexIndex] = ProjectCameraToClipspace(transformedVertex);
        }

        for (int connectionIndex = 0; connectionIndex < model.connections.Length; connectionIndex+= 2)
        {
            int startVertexIndex = model.connections[connectionIndex];
            int endVertexIndex = model.connections[connectionIndex + 1];
            
            PixelCoordinate startCoord = new PixelCoordinate(clipSpaceVertexCoordinates[startVertexIndex]);
            PixelCoordinate endCoord = new PixelCoordinate(clipSpaceVertexCoordinates[endVertexIndex]);
            DrawLine(startCoord, endCoord, new Color(1, 1, 1));
        }
    }
    
}