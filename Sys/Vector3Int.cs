using OpenTK.Mathematics;

namespace minecraft.Sys;

public record struct Vector3Int(int X = 0, int Y = 0, int Z = 0)
{
    public int X = X, Y = Y, Z = Z;
    public static Vector3Int Zero => new(0, 0, 0);
    public static Vector3Int Up => new(0, 1, 0);

    public static Vector3Int operator +(Vector3Int v1, Vector3Int v2)
    {
        return new Vector3Int(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }
    
    public static Vector3Int operator +(Vector3Int v, int a)
    {
        return new Vector3Int(v.X + a, v.Y + a, v.Z + a);
    }
    
    public static Vector3 operator + (Vector3Int vi, Vector3 vf)
    {
        return new Vector3(vi.X + vf.X, vi.Y + vf.Y, vi.Z + vf.Z);
    }
    
    public static Vector3Int operator -(Vector3Int v1, Vector3Int v2)
    {
        return new Vector3Int(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }

    public static Vector3Int operator -(Vector3Int v, int a)
    {
        return new Vector3Int(v.X - a, v.Y - a, v.Z - a);
    }

    public static Vector3 operator - (Vector3Int vi, Vector3 vf)
    {
        return new Vector3(vi.X - vf.X, vi.Y - vf.Y, vi.Z - vf.Z);
    }
    
    public static Vector3Int operator *(Vector3Int v, int a)
    {
        return new Vector3Int(v.X * a, v.Y * a, v.Z * a);
    }
    
    public static explicit operator Vector3Int(Vector3 r)
    {
        return new Vector3Int((int)r.X, (int)r.Y, (int)r.Z);
        
    }
    public static implicit operator Vector3(Vector3Int vi)
    {
        return new Vector3(vi.X, vi.Y, vi.Z);
    }

    public static Vector3Int RoundToInt(Vector3 v)
    {
        return new Vector3Int(
            (int)Math.Round(v.X),
            (int)Math.Round(v.Y),
            (int)Math.Round(v.Z));
    }
}