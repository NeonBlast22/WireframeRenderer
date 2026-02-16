namespace WireframeRenderer;

public struct Matrix4X4
{
    //Collum, Row (it looks opposite here)
    public float[,] M =
    {
        { 1f, 0f, 0f, 0f },
        { 0f, 1f, 0f, 0f },
        { 0f, 0f, 1f, 0f },
        { 0f, 0f, 0f, 1f }
    };

    public Matrix4X4()
    {
        
    }
    
    public static Matrix4X4 FromRotation(float xRotation, float yRotation, float zRotation)
    {
        Matrix4X4 result = new Matrix4X4();

        float cosX = (float)Math.Cos(xRotation);
        float sinX = (float)Math.Sin(xRotation);
        float cosY = (float)Math.Cos(yRotation);
        float sinY = (float)Math.Sin(yRotation);
        float cosZ = (float)Math.Cos(zRotation);
        float sinZ = (float)Math.Sin(zRotation);
        
        result.M[0, 0] = cosY * cosZ;
        result.M[0, 1] = sinX * sinY * cosZ + cosX * sinZ;
        result.M[0, 2] = -cosX * sinY * cosZ + sinX * sinZ;
        result.M[0, 3] = 0f;
        
        result.M[1, 0] = -cosY * sinZ;
        result.M[1, 1] = -sinX * sinY * sinZ + cosX * cosZ;
        result.M[1, 2] = cosX * sinY * sinZ + sinX * cosZ;
        result.M[1, 3] = 0f;
        
        result.M[2, 0] = sinY;
        result.M[2, 1] = -sinX * cosY;
        result.M[2, 2] = cosX * cosY;
        result.M[2, 3] = 0f;
        
        result.M[3, 0] = 0f;
        result.M[3, 1] = 0f;
        result.M[3, 2] = 0f;
        result.M[3, 3] = 1f;
    
        return result;
    }
    
    public static Matrix4X4 FromTranslation(float x, float y, float z)
    {
        Matrix4X4 result = new Matrix4X4();
        
        result.M[0, 0] = 1f;
        result.M[0, 1] = 0f;
        result.M[0, 2] = 0f;
        result.M[0, 3] = 0f;
        
        result.M[1, 0] = 0f;
        result.M[1, 1] = 1f;
        result.M[1, 2] = 0f;
        result.M[1, 3] = 0f;
        
        result.M[2, 0] = 0f;
        result.M[2, 1] = 0f;
        result.M[2, 2] = 1f;
        result.M[2, 3] = 0f;
        
        result.M[3, 0] = x;
        result.M[3, 1] = y;
        result.M[3, 2] = z;
        result.M[3, 3] = 1f;
    
        return result;
    }
    
    public static Matrix4X4 FromTranslation(Vector3 translation)
    {
        return FromTranslation(translation.x, translation.y, translation.z);
    }

    public static Vector3 operator *(Vector3 v, Matrix4X4 m)
    {
        Vector3 transformed = new Vector3(MultiplyWithCollum(v.x, m.GetColumn(0)));
        transformed += new Vector3(MultiplyWithCollum(v.y, m.GetColumn(1)));
        transformed += new Vector3(MultiplyWithCollum(v.z, m.GetColumn(2)));
        transformed += new Vector3(MultiplyWithCollum(1, m.GetColumn(3))); //4th axis is one always

        return transformed;
    }

    static float[] MultiplyWithCollum(float scalar, float[] collum)
    {
        float[] output = new float[collum.Length];

        for (int row = 0; row < collum.Length; row++)
        {
            output[row] = collum[row] * scalar;
        }
        
        return output;
    }
    
    float[] GetColumn(int columnIndex)
    {
        int rows = M.GetLength(0);
        float[] column = new float[rows];
    
        for (int i = 0; i < rows; i++)
        {
            column[i] = M[columnIndex, i];
        }
    
        return column;
    }
}