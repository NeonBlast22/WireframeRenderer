using Silk.NET.Input;

namespace WireframeRenderer;

public static class WireframeRenderer
{
    public static int screenWidth = 2650;
    public static int screenHeight = 1600;
    private const float FOV = 2;

    private static WireframeModel model;
    
    private static Matrix4X4 rotationMatrix;
    private static Matrix4X4 modelTranslation;
    private static float scale = 1f;
    
    static int Main()
    {
        Screen.OnStart += Start;
        Screen.OnUpdate += Update;
        
        STLLoader.LoadStl();
        
        Screen.Initialize(screenWidth, screenHeight, "WireframeRenderer");
        return 0;
    }

    static void Start()
    {
        Screen.ClearScreen();

        rotationMatrix = Matrix4X4.FromRotation(MathF.PI / 2, 0f, 0f);
        modelTranslation = Matrix4X4.FromTranslation(0f, 0f, 5f);
        
        /*
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
        model = new WireframeModel(vertices, connections);
        */
        
        model = new WireframeModel(STLLoader.Vertices, STLLoader.Edges);
    }

    private static float yaw = 0f;
    static void Update(double deltaTime)
    {
        Screen.ClearScreen();
        
        if (Screen.GetKey(Key.S)) rotationMatrix = Matrix4X4.FromRotation((float)deltaTime, 0f, 0f) * rotationMatrix;
        if (Screen.GetKey(Key.W)) rotationMatrix = Matrix4X4.FromRotation((float)-deltaTime, 0f, 0f) * rotationMatrix;
        
        if (Screen.GetKey(Key.A)) rotationMatrix = Matrix4X4.FromRotation(0f, (float)deltaTime, 0f) * rotationMatrix;
        if (Screen.GetKey(Key.D)) rotationMatrix = Matrix4X4.FromRotation(0f, (float)-deltaTime, 0f) * rotationMatrix;
        
        if (Screen.GetKey(Key.Q)) rotationMatrix = Matrix4X4.FromRotation(0f, 0f, (float)-deltaTime) * rotationMatrix;
        if (Screen.GetKey(Key.E)) rotationMatrix = Matrix4X4.FromRotation(0f, 0f, (float)deltaTime) * rotationMatrix;
        
        if (Screen.GetKey(Key.R)) rotationMatrix = Matrix4X4.FromRotation(MathF.PI / 2, 0f, 0f);
        
        if (Screen.GetKey(Key.Z)) scale *= 1f + (float)deltaTime * 1.5f;
        if (Screen.GetKey(Key.X)) scale /= 1f + (float)deltaTime * 1.5f;
        
        Color color = new Color(1f, 1f, 1);
        RenderWireframe(model, modelTranslation * (scale * rotationMatrix), color);
        
        //Console.WriteLine($"FPS: {Math.Round(1f / deltaTime, 1)}");
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

    static void RenderWireframe(WireframeModel model, Matrix4X4 transformation, Color color)
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
            DrawLine(startCoord, endCoord, color);
        }
    }
    
}