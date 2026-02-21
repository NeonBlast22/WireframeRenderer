namespace WireframeRenderer;

public struct Vector3
{
    public float x;
    public float y;
    public float z;
    
    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3(float[] values)
    {
        this.x = values[0];
        this.y = values[1];
        this.z = values[2];
    }

    public Vector3 Normalize()
    {
        return this / Magnitude();
    }

    public float Magnitude()
    {
        return MathF.Sqrt(x * x + y * y + z * z);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3(a.x * b, a.y * b, a.z * b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
        return new Vector3(a.x / b, a.y / b, a.z / b);
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Vector3 other)
            return x == other.x && y == other.y && z == other.z;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }
    
}