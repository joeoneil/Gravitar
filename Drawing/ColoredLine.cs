using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.Drawing; 

public class ColoredLine : Line {
    public Color Color { get; }
    
    public ColoredLine (Vector2 start, Vector2 end, Color color) : base(start, end) {
        Color = color;
    }
    
    public ColoredLine (float startX, float startY, float endX, float endY, Color color) : base(startX, startY, endX, endY) {
        Color = color;
    }
    
    public ColoredLine (Line line, Color color) : base(line.Start, line.End) {
        Color = color;
    }
    
    public ColoredLine (ColoredLine line) : base(line.Start, line.End) {
        Color = line.Color;
    }
}