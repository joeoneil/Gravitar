using System;
using System.Collections.Generic;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.GameObj; 

public class Projectile {
    public enum Alignment {
        Player,
        Enemy,
        Particle, // Used for explosions
    }
    
    public readonly Alignment alignment;
    
    public Vector2 Position { get; set; }
    
    public Vector2 Velocity { get; }

    private float lifetime = 2.25f;
    
    public Projectile(Alignment alignment, Vector2 position, Vector2 velocity, float lifetime = 2.25f) {
        this.alignment = alignment;
        Position = position;
        Velocity = velocity;
        this.lifetime = lifetime;
    }
    
    public void Update(GameTime gameTime) {
        Position += Velocity * gameTime.ElapsedGameTime.Milliseconds / 1000f;
        lifetime -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
    }
    
    public IEnumerable<ColoredLine> getLines() =>
        new List<ColoredLine> {
            new(
                new Line(Position, Position + Vector2.UnitX),
                alignment switch {
                    Alignment.Player => Color.Yellow,
                    Alignment.Enemy => Color.Red,
                    Alignment.Particle => Color.Pink,
                    _ => Color.White
                }
            ),
            new(
                new Line(Position + Vector2.UnitX, Position + Vector2.One),
                alignment switch {
                    Alignment.Player => Color.Yellow,
                    Alignment.Enemy => Color.Red,
                    Alignment.Particle => Color.HotPink,
                    _ => Color.White
                }
            ),
        };

    public bool isExpired() {
        return lifetime <= 0;
    }

    public static IEnumerable<Projectile> explode(Vector2 position, int count) {
        // Create a bunch of particles
        List<Projectile> particles = new();
        for (int i = 0; i < count; i++) {
            float angle = (float)Random.Shared.NextDouble() * MathF.PI * 2;
            float speed = (float)Random.Shared.NextDouble() * 100 + 50;
            particles.Add(new Projectile(Alignment.Particle, position, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed, 1.75f));
        }

        return particles;
    }
}