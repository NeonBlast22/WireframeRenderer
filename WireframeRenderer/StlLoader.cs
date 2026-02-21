namespace WireframeRenderer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static class STLLoader
{
    public struct Triangle
    {
        public Vector3 Normal;
        public Vector3[] Vertices; // 3 vertices
    }

    public static Vector3[] Vertices { get; private set; }
    public static (int, int)[] Edges { get; private set; }
    public static Triangle[] Triangles { get; private set; }

    public static void Load(string filePath)
    {
        // Detect if binary or ASCII STL
        if (IsBinaryStl(filePath))
            LoadBinaryStl(filePath);
        else
            LoadAsciiStl(filePath);

        BuildVerticesAndEdges();
    }
    
    private static void LoadBinaryStl(string filePath)
    {
        using var reader = new BinaryReader(File.Open(filePath, FileMode.Open));

        byte[] header = reader.ReadBytes(80);   // 80-byte header (ignored)
        uint triCount = reader.ReadUInt32();

        var triangles = new List<Triangle>();

        for (uint i = 0; i < triCount; i++)
        {
            var tri = new Triangle { Vertices = new Vector3[3] };

            tri.Normal = ReadVector3(reader);
            tri.Vertices[0] = ReadVector3(reader);
            tri.Vertices[1] = ReadVector3(reader);
            tri.Vertices[2] = ReadVector3(reader);

            reader.ReadUInt16(); // attribute byte count (ignored)

            triangles.Add(tri);
        }

        Triangles = triangles.ToArray();
    }

    private static Vector3 ReadVector3(BinaryReader r) =>
        new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    

    private static void LoadAsciiStl(string filePath)
    {
        var triangles = new List<Triangle>();
        var lines = File.ReadAllLines(filePath);

        Triangle current = new Triangle { Vertices = new Vector3[3] };
        int vertexIndex = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("facet normal"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                current.Normal = new Vector3(
                    float.Parse(parts[2]),
                    float.Parse(parts[3]),
                    float.Parse(parts[4]));
                vertexIndex = 0;
            }
            else if (line.StartsWith("vertex"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                current.Vertices[vertexIndex++] = new Vector3(
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3]));
            }
            else if (line.StartsWith("endfacet"))
            {
                triangles.Add(current);
                current = new Triangle { Vertices = new Vector3[3] };
            }
        }

        Triangles = triangles.ToArray();
    }

    private static void BuildVerticesAndEdges()
    {
        var vertexMap = new Dictionary<Vector3, int>();
        var edges = new HashSet<(int, int)>();
        var vertexList = new List<Vector3>();

        int GetOrAdd(Vector3 v)
        {
            if (!vertexMap.TryGetValue(v, out int idx))
            {
                idx = vertexList.Count;
                vertexList.Add(v);
                vertexMap[v] = idx;
            }
            return idx;
        }

        foreach (var tri in Triangles)
        {
            int a = GetOrAdd(tri.Vertices[0]);
            int b = GetOrAdd(tri.Vertices[1]);
            int c = GetOrAdd(tri.Vertices[2]);

            // Store edges with the smaller index first to avoid duplicates
            edges.Add((Math.Min(a, b), Math.Max(a, b)));
            edges.Add((Math.Min(b, c), Math.Max(b, c)));
            edges.Add((Math.Min(a, c), Math.Max(a, c)));
        }

        Vertices = vertexList.ToArray();
        Edges = edges.ToArray();
    }
    
    private static bool IsBinaryStl(string filePath)
    {
        // Binary STL files are always 80 (header) + 4 (triangle count) + n * 50 bytes
        // ASCII STL files start with the text "solid"
        byte[] header = File.ReadAllBytes(filePath).Take(80).ToArray();
        string headerText = System.Text.Encoding.ASCII.GetString(header).TrimStart();
        
        //Easy Test for Binary
        if (!headerText.StartsWith("solid", StringComparison.OrdinalIgnoreCase))
            return true;
        
        //More through test for Binary
        long fileSize = new FileInfo(filePath).Length;
        if (fileSize < 84) return false;
    
        uint triangleCount = BitConverter.ToUInt32(File.ReadAllBytes(filePath), 80);
        long expectedSize = 84 + (triangleCount * 50);
    
        return fileSize == expectedSize;
    }
    
    public static void LoadStl()
    {
        // Find the first .stl file in the same directory as the executable
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string[] stlFiles = Directory.GetFiles(dir, "*.stl");

        if (stlFiles.Length == 0)
        {
            Console.WriteLine("No .stl file found in the application directory.");
            return;
        }

        string filePath = stlFiles[0];
        Console.WriteLine($"Loading: {filePath}\n");
        Load(filePath);
        
        Console.WriteLine($"Vertices  : {Vertices.Length}");
        Console.WriteLine($"Edges     : {Edges.Length}");
    }
}