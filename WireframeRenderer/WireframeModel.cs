namespace WireframeRenderer;

public class WireframeModel
{
    public Vector3[] vertices;
    public int[] connections; //should be an even length

    public WireframeModel(Vector3[] vertices, int[] connections)
    {
        this.vertices = vertices;
        this.connections = connections;
    }
    
    public WireframeModel(Vector3[] vertices, (int, int)[] connections)
    {
        this.vertices = vertices;
        this.connections = new  int[connections.Length * 2];
        for (int i = 0; i < connections.Length * 2; i += 2)
        {
            this.connections[i] = connections[i / 2].Item1;
            this.connections[i + 1] = connections[i / 2].Item2;
        }
    }
}