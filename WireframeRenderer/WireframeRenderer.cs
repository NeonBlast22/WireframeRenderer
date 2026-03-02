using Silk.NET.Input;

namespace WireframeRenderer;

public static class WireframeRenderer
{
    public static int screenWidth = 2650;
    public static int screenHeight = 1600;
    private const float FOV = 2;
    
    private static Matrix4X4 rotationMatrix;
    private static Matrix4X4 modelTranslation;
    private static float scale = 1f;
    private static int modelIndex = 0;
    
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
    }
    
    private static float t = 0f;
    static bool wasNpressedLastFrame = false;
    static bool wasBpressedLastFrame = false;
    static bool wasVpressedLastFrame = false;
    static void Update(double deltaTime)
    {
        Screen.ClearScreen();
        
        if (Screen.GetKey(Key.S)) rotationMatrix = Matrix4X4.FromRotation((float)deltaTime, 0f, 0f) * rotationMatrix;
        if (Screen.GetKey(Key.W)) rotationMatrix = Matrix4X4.FromRotation((float)-deltaTime, 0f, 0f) * rotationMatrix;
        
        if (Screen.GetKey(Key.A)) rotationMatrix = Matrix4X4.FromRotation(0f, (float)deltaTime, 0f) * rotationMatrix;
        if (Screen.GetKey(Key.D)) rotationMatrix = Matrix4X4.FromRotation(0f, (float)-deltaTime, 0f) * rotationMatrix;
        
        if (Screen.GetKey(Key.Q)) rotationMatrix = Matrix4X4.FromRotation(0f, 0f, (float)-deltaTime) * rotationMatrix;
        if (Screen.GetKey(Key.E)) rotationMatrix = Matrix4X4.FromRotation(0f, 0f, (float)deltaTime) * rotationMatrix;

        if (Screen.GetKey(Key.R))
        {
            rotationMatrix = Matrix4X4.FromRotation(MathF.PI / 2, 0f, 0f);
            scale = 1f;
            modelTranslation = Matrix4X4.FromTranslation(0f, 0f, 5f);
        }
        
        if (Screen.GetKey(Key.Z)) scale *= 1f + (float)deltaTime * 1.5f;
        if (Screen.GetKey(Key.X)) scale /= 1f + (float)deltaTime * 1.5f;
        
        if (Screen.GetKey(Key.I)) modelTranslation *= Matrix4X4.FromTranslation(0f, (float)-deltaTime, 0f);
        if (Screen.GetKey(Key.K)) modelTranslation *= Matrix4X4.FromTranslation(0f, (float)deltaTime, 0f);
        
        if (Screen.GetKey(Key.J)) modelTranslation *= Matrix4X4.FromTranslation((float)-deltaTime, 0f, 0f);
        if (Screen.GetKey(Key.L)) modelTranslation *= Matrix4X4.FromTranslation((float)deltaTime, 0f, 0f);

        if (Screen.GetKey(Key.U)) modelTranslation *= Matrix4X4.FromTranslation(0f, 0f, (float)deltaTime);
        if (Screen.GetKey(Key.O)) modelTranslation *= Matrix4X4.FromTranslation(0f, 0f, (float)-deltaTime);

        if (Screen.GetKey(Key.N) && !wasNpressedLastFrame)
        {
            modelIndex++;
            if (modelIndex >= STLLoader.models.Count) modelIndex = 0;
        }

        if (Screen.GetKey(Key.B) && !wasBpressedLastFrame)
        {
            modelIndex--;
            if (modelIndex < 0) modelIndex = STLLoader.models.Count - 1;
        }
        
        if (Screen.GetKey(Key.V) && !wasVpressedLastFrame)
        {
            STLLoader.OpenModelFolder();
        }
        
        Color color = Color.FromHSV(t * 180, 1f, 1f); 
        RenderWireframe(STLLoader.models[modelIndex], modelTranslation * (Matrix4X4.FromScale(scale) * rotationMatrix), color);

        t += (float)deltaTime;
        wasBpressedLastFrame = Screen.GetKey(Key.B);
        wasNpressedLastFrame = Screen.GetKey(Key.N);
        wasVpressedLastFrame = Screen.GetKey(Key.V);
    }

    static Vector2 ProjectCameraToClipspace(Vector3 vertexPosition)
    {
        if (vertexPosition.z < 0) vertexPosition.z = 0;
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
            if (x > 0 && x < screenWidth && y > 0 && y < screenHeight)Screen.SetPixel(x, y, color);

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
            
            if ((startCoord.x > screenWidth || startCoord.y > screenHeight || startCoord.x < 0 || startCoord.y < 0)
                &&  (endCoord.x > screenWidth || endCoord.y > screenHeight || endCoord.x < 0 || endCoord.y < 0)) continue;
            
            DrawLine(startCoord, endCoord, color);
        }
    }
    
}