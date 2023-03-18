using System;
using Microsoft.Xna.Framework;

namespace Gravitar.Shape; 

public class Line {
    public Vector2 Start { get; }
    public Vector2 End { get; }
    
    public Line(Vector2 start, Vector2 end) {
        Start = start;
        End = end;
    }
    
    // Only 1 operator overload because it's all I need
    public static Line operator +(Line line, Vector2 vec) => new(line.Start + vec, line.End + vec);
    
    public Line(float startX, float startY, float endX, float endY) {
        Start = new Vector2(startX, startY);
        End = new Vector2(endX, endY);
    }

    public float Length => Vector2.Distance(Start, End);
    
    public Vector2 Direction => Vector2.Normalize(End - Start);
    
    public Vector2 Midpoint => (Start + End) / 2;
    
    public float Angle => (float) Math.Atan2(End.Y - Start.Y, End.X - Start.X);
    
    public float AngleDeg => MathHelper.ToDegrees(Angle);

    public float LongestAxis => Math.Max(Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y));
    
    public Vector2 lerp(float t) {
        return Vector2.Lerp(Start, End, t);
    }
    
    public bool Intersects(Line other) {
        // https://stackoverflow.com/a/565282/10904296
        Vector2 r = End - Start;
        Vector2 q = other.Start;
        Vector2 s = other.End - other.Start;
        
        float t = cross(q - Start, s) / cross(r, s);
        float u = cross(q - Start, r) / cross(r, s);
        
        return t is >= 0 and <= 1 && u is >= 0 and <= 1;
    }
    
    private float cross(Vector2 v1, Vector2 v2) {
        return v1.X * v2.Y - v1.Y * v2.X;
    }
}