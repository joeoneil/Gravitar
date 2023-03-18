using System;
using System.Collections.Generic;
using System.Linq;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.GameObj; 

public class Player {
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    private readonly float Acceleration;
    private readonly float Friction;
    public float Rotation { get; set; }
    
    private bool _thruster;
    private bool _shield;

    private float _shieldTimer;

    public static List<Line> defaultLines { get; }= new() {
        new Line(new Vector2(0, 10), new Vector2(8, -2)),
        new Line(new Vector2(8, -2), new Vector2(5, -7)),
        new Line(new Vector2(5, -7), new Vector2(2, -7)),
        new Line(new Vector2(2, -7), new Vector2(0, -4)),
        new Line(new Vector2(0, -4), new Vector2(-2, -7)),
        new Line(new Vector2(-2, -7), new Vector2(-5, -7)),
        new Line(new Vector2(-5, -7), new Vector2(-8, -2)),
        new Line(new Vector2(-8, -2), new Vector2(0, 10)),
    };

    public static List<Line> thrustLines { get; } = new() {
        new Line(new Vector2(-2, -7), new Vector2(0, -10)),
        new Line(new Vector2(0, -10), new Vector2(2, -7)),
    };
    
    // Shield is a regular hexagon
    public static List<Line> shieldLines { get; }= new() {
        new Line(new Vector2(-6, 12), new Vector2(6, 12)),
        new Line(new Vector2(6, 12), new Vector2(12, 0)),
        new Line(new Vector2(12, 0), new Vector2(6, -12)),
        new Line(new Vector2(6, -12), new Vector2(-6, -12)),
        new Line(new Vector2(-6, -12), new Vector2(-12, 0)),
        new Line(new Vector2(-12, 0), new Vector2(-6, 12)),
    };

    public static float RotationSpeed => 3.75f;
    
    public enum Direction {
        Left,
        Right,
        None,
    }
    
    public Player(float acc, float friction = 0.1f) {
        Acceleration = acc;
        Friction = friction;
    }

    public void Update(GameTime gameTime, bool thrust, Direction rotation, bool shield) {
        switch (rotation) {
            case Direction.Left:
                Rotation -= RotationSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000f;
                break;
            case Direction.Right:
                Rotation += RotationSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000f;
                break;
            default:
                // Ship is not rotating
                break;
        }

        _thruster = thrust;
        
        if (thrust) {
            Velocity += new Vector2(
                (float) Math.Cos(Rotation + Math.PI / 2),
                (float) Math.Sin(Rotation + Math.PI / 2)
                ) * Acceleration * gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }
        
        _shield = shield;
        if (shield) {
            _shieldTimer += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            _shieldTimer %= 0.1f;
        }

        Velocity *= MathF.Pow((1 - Friction), gameTime.ElapsedGameTime.Milliseconds / 1000f);
        
        Position += Velocity * gameTime.ElapsedGameTime.Milliseconds / 1000f;
    }
    
    public void applyForce(Vector2 force) {
        Velocity += force;
    }

    public List<ColoredLine> getLines() {
        var lines = (
            from line in defaultLines 
            let start = new Vector2(
                (float)Math.Cos(Rotation) * line.Start.X - (float)Math.Sin(Rotation) * line.Start.Y, 
                (float)Math.Sin(Rotation) * line.Start.X + (float)Math.Cos(Rotation) * line.Start.Y) + Position 
            let end = new Vector2(
                (float)Math.Cos(Rotation) * line.End.X - (float)Math.Sin(Rotation) * line.End.Y,
                (float)Math.Sin(Rotation) * line.End.X + (float)Math.Cos(Rotation) * line.End.Y) + Position 
            select new ColoredLine(start, end, Color.Blue)).ToList();

        if (_thruster) {
            lines.AddRange(
                from line in thrustLines 
                let start = new Vector2(
                    (float)Math.Cos(Rotation) * line.Start.X - (float)Math.Sin(Rotation) * line.Start.Y,
                    (float)Math.Sin(Rotation) * line.Start.X + (float)Math.Cos(Rotation) * line.Start.Y) + Position 
                let end = new Vector2(
                    (float)Math.Cos(Rotation) * line.End.X - (float)Math.Sin(Rotation) * line.End.Y,
                    (float)Math.Sin(Rotation) * line.End.X + (float)Math.Cos(Rotation) * line.End.Y) + Position 
                select new ColoredLine(start, end, Color.Red));
        }

        if (_shield) {
            lines.AddRange(
                from line in shieldLines
                let start = new Vector2(
                    line.Start.X + Position.X,
                    line.Start.Y + Position.Y)
                let end = new Vector2(
                    line.End.X + Position.X, 
                    line.End.Y + Position.Y)
                    select new ColoredLine(start, end, _shieldTimer < 0.05f ? Color.Red : Color.Blue)
                );
        }
        
        return lines;
    }

    public List<Line> getHull() {
        var lines = (
            from line in defaultLines 
            let start = new Vector2(
                (float)Math.Cos(Rotation) * line.Start.X - (float)Math.Sin(Rotation) * line.Start.Y, 
                (float)Math.Sin(Rotation) * line.Start.X + (float)Math.Cos(Rotation) * line.Start.Y) + Position 
            let end = new Vector2(
                (float)Math.Cos(Rotation) * line.End.X - (float)Math.Sin(Rotation) * line.End.Y,
                (float)Math.Sin(Rotation) * line.End.X + (float)Math.Cos(Rotation) * line.End.Y) + Position 
            select new Line(start, end)).ToList();
        
        return lines;
    }

    public Projectile shoot() {
        return new Projectile(Projectile.Alignment.Player, Position, Velocity + new Vector2(
            (float)Math.Cos(Rotation + Math.PI / 2),
            (float)Math.Sin(Rotation + Math.PI / 2)
        ) * 275);
    }
}
