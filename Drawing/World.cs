using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Gravitar.Shape;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gravitar.Drawing;

public class World {
    private readonly Image _image;
    public Camera Camera;

    private readonly List<ColoredLine> _lines = new();
    private readonly List<ColoredLine> _tempLines = new();

    private bool moduloRender;
    private float moduloRenderWidth;

    public World(Image image, Camera camera) {
        _image = image;
        this.Camera = camera;
    }

    public void AddLine(ColoredLine line) {
        _lines.Add(line);
    }

    public void AddLines(IEnumerable<ColoredLine> lines) {
        _lines.AddRange(lines);
    }

    public void Reset() {
        _image.Fill(Color.Black);
    }

    public void Draw(SpriteBatch spriteBatch) {
        foreach (ColoredLine line in _lines) {
            _image.drawLine(Camera.WorldToScreen(line), line.Color);
        }
        foreach (ColoredLine line in _tempLines) {
            _image.drawLine(Camera.WorldToScreen(line), line.Color);
        }

        if (moduloRender) {
            // Triple (or more) the overhead of drawing the lines, but the math is too hard otherwise.
            foreach (ColoredLine line in _lines) {
                _image.drawLine(Camera.WorldToScreen(line + new Vector2(moduloRenderWidth, 0)), line.Color);
                _image.drawLine(Camera.WorldToScreen(line + new Vector2(-moduloRenderWidth, 0)), line.Color);
            }
            foreach (ColoredLine line in _tempLines) {
                _image.drawLine(Camera.WorldToScreen(line + new Vector2(moduloRenderWidth, 0)), line.Color);
                _image.drawLine(Camera.WorldToScreen(line + new Vector2(-moduloRenderWidth, 0)), line.Color);
            }
        }
        
        _tempLines.Clear();

        spriteBatch.Draw(_image.Texture, new Rectangle(0, 0, Game1.Width, Game1.Height), Color.White);
    }
    
    public void drawPoint(Vector2 point, Color color) {
        _image.drawPoint(Camera.WorldToScreen(point), color);
    }
    
    public void addTempLine(ColoredLine line) {
        _tempLines.Add(line);
    }
    
    public void addTempLines(IEnumerable<ColoredLine> lines) {
        _tempLines.AddRange(lines);
    }
    
    public void setLines(IEnumerable<ColoredLine> lines) {
        _lines.Clear();
        _lines.AddRange(lines);
    }
    
    public List<ColoredLine> getLines() {
        return _lines;
    }
    
    public void setModuloRender(float width) {
        this.moduloRender = true;
        this.moduloRenderWidth = width;
    }
    
    private float positiveModulo(float a, float b) {
        return (a % b + b) % b;
    }
}