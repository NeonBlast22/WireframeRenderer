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
}