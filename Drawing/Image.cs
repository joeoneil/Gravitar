using System;
using Gravitar.Shape;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gravitar.Drawing; 

public class Image {
    private Texture2D _texture;
    private Color[] _data;
    
    public int Width { get; }

    public int Height { get; }
    
    private bool _updated = false;
    
    public Texture2D Texture {
        get {
            if (_updated) {
                updateTexture();
            }
            return _texture;
        }
    }

    public Image(GraphicsDevice graphicsDevice) {
        Width = (int) (Game1.Width / Game1.Scale);
        Height = (int) (Game1.Height / Game1.Scale);
        _texture = new Texture2D(graphicsDevice, Width, Height);
        _data = new Color[Width * Height];
    }
    
    public void Fill(Color color) {
        _updated = true;
        for (int i = 0; i < _data.Length; i++) {
            _data[i] = color;
        }
    }

    public void drawLine(Line line) {
        drawLine(line.Start, line.End, Color.White);
    }

    public void drawLine(Line line, Color color) {
        drawLine(line.Start, line.End, color);
    }

    public void drawLine(ColoredLine line) {
        drawLine(line.Start, line.End, line.Color);
    }

    public void drawPoint(Vector2 point, Color color) {
        setPixel((int)point.X, (int)point.Y, color);
    }

    private void drawLine(Vector2 start, Vector2 end, Color color) {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float length = Math.Max(Math.Abs(dx), Math.Abs(dy));
        float dt = 1 / length;
        
        for (float t = 0; t <= 1; t += dt) {
            Vector2 pos = Vector2.Lerp(start, end, t);
            if ((int) pos.X < 0 || (int) pos.X >= Width || (int) pos.Y < 0 || (int) pos.Y >= Height) {
                continue;
            }
            setPixel((int) pos.X, (int) pos.Y, color);
        }
    }

    private void setPixel(int x, int y, Color color) {
        _data[x + y * Width] = color;
    }

    private void updateTexture() {
        _texture.SetData(_data);
        _updated = false;
    }
}