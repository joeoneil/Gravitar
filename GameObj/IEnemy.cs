using System;
using System.Collections.Generic;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;

namespace Gravitar.GameObj; 

public interface IEnemy {
    public bool Destroyed { get; set; }
    
    public float BoundsRadius { get; }
    public Vector2 Position { get; }
    
    public List<ColoredLine> getLines();
    
    public Projectile? shoot();
    
    public void Update(GameTime gameTime);
}