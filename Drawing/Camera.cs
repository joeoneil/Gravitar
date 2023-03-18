using System.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.Drawing; 

public class Camera {
    /// <summary>
    /// The center of the camera in world coordinates
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;
    
    /// <summary>
    /// The center of the camera in screen coordinates
    /// </summary>
    private Vector2 screenCenter;

    /// <summary>
    /// How much the camera is zoomed in
    /// </summary>
    public float Zoom { get; set; } = 1f;

    public Camera(Vector2 position, float zoom) {
        Position = position;
        Zoom = zoom;
        
        screenCenter = new Vector2((Game1.Width / Game1.Scale) / 2f, (Game1.Height / Game1.Scale) / 2f);
    }
    
    public Matrix ViewMatrix =>
        Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));

    public Matrix InverseViewMatrix =>
        Matrix.CreateScale(new Vector3(1 / Zoom, 1 / Zoom, 1)) *
        Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));

    public void Move(Vector2 offset) {
        Position += offset;
    }
    
    public void MoveScreen(Vector2 screenOffset) {
        Position += ScreenToWorld(screenOffset);
    }
    
    public void Move(float x, float y) {
        Position += new Vector2(x, y);
    }
    
    public void ZoomIn(float amount) {
        Zoom += amount;
    }
    
    public void ZoomOut(float amount) {
        Zoom -= amount;
        if (Zoom < 0.1f) {
            Zoom = 0.1f;
        }
    }
    
    public void ZoomTo(float zoom) {
        Zoom = zoom;
    }
    
    public RectangleF WorldBounds => new (Position.X - screenCenter.X / Zoom, Position.Y - screenCenter.Y / Zoom, Game1.Width / Game1.Scale / Zoom, Game1.Height / Game1.Scale / Zoom);

    public Vector2 ScreenToWorld(Vector2 screenPosition) {
        return Vector2.Transform(screenPosition - screenCenter, InverseViewMatrix);
    }
    
    public Line ScreenToWorld(Line line) {
        return new Line(ScreenToWorld(line.Start), ScreenToWorld(line.End));
    }
    
    public Vector2 WorldToScreen(Vector2 worldPosition) {
        return Vector2.Transform(worldPosition, ViewMatrix) + screenCenter;
    }
    
    public Line WorldToScreen(Line line) {
        return new Line(WorldToScreen(line.Start), WorldToScreen(line.End));
    }
}