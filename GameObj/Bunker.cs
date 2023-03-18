using System;
using System.Collections.Generic;
using System.Linq;
using Gravitar.Data;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.GameObj; 

/// <summary>
/// Bunkers are enemies that can be destroyed by the player, they are not affected by gravity and fire randomly
/// </summary>
public class Bunker : IEnemy {
    public Vector2 Position { get; }
    public float Rotation { get; }
    public float fireRate { get; }
    public float fireCooldown { get; private set; }

    public bool Destroyed { get; set; }
    
    public float BoundsRadius => 18;

    private static readonly List<Line> _bunkerLines = new() {
        new Line(new Vector2(-18, 0), new Vector2(18, 0)),
        new Line(new Vector2(18, 0), new Vector2(8, -12)),
        new Line(new Vector2(8, -12), new Vector2(-8, -12)),
        new Line(new Vector2(-8, -12), new Vector2(-18, 0)),
        new Line(new Vector2(-4, -12), new Vector2(-4, -18)),
        new Line(new Vector2(-4, -18), new Vector2(4, -18)),
        new Line(new Vector2(4, -18), new Vector2(4, -12)),
    };
    
    public Bunker(Vector2 position, float rotation, float fireRate) {
        Position = position;
        Rotation = rotation;
        this.fireRate = fireRate;
        fireCooldown = 0;
    }
    
    public Bunker(EnemyData data) : this(data.position, data.rotation, data.fireRate ?? 1) { }
    
    public void Update(GameTime gameTime) {
        fireCooldown -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
        if (fireCooldown <= 0) {
            fireCooldown = 0;
        }
    }

    public Projectile? shoot() {
        if (fireCooldown > 0) {
            return null;
        }
        fireCooldown += (1 / fireRate) * 2 * (float)Random.Shared.NextDouble();
        // Fire a projectile in a random direction
        float angle = (float)Random.Shared.NextDouble() * MathF.PI;
        angle += Rotation + MathF.PI;
        return new Projectile(
            Projectile.Alignment.Enemy, 
            Position + new Vector2(16 * MathF.Cos(Rotation - MathF.PI / 2), 16 * MathF.Sin(Rotation - MathF.PI / 2)), 
            new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 175, 1.25f);
    }
    
    public List<ColoredLine> getLines() {
        List<ColoredLine> lines = new();
        lines.AddRange(
            from line in _bunkerLines
            let start = new Vector2(
                line.Start.X * MathF.Cos(Rotation) - line.Start.Y * MathF.Sin(Rotation),
                line.Start.X * MathF.Sin(Rotation) + line.Start.Y * MathF.Cos(Rotation)
            )
            let end = new Vector2(
                line.End.X * MathF.Cos(Rotation) - line.End.Y * MathF.Sin(Rotation),
                line.End.X * MathF.Sin(Rotation) + line.End.Y * MathF.Cos(Rotation)
            )
            select new ColoredLine(start + Position, end + Position, Color.Red)
        );
        return lines;
    }
}