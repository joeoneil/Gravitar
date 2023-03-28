using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Devcade.events;
using Gravitar.Data;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gravitar.GameObj;

public class Encounter : IMenu {
    private float _time { get; set; }
    
    protected readonly InputManager _inputManager;
    
    protected readonly World _world;

    protected Player _player;
    protected readonly float _shipAcceleration;
    protected readonly Vector2 _playerStartPos;

    protected bool _thrusting;
    protected bool _shielding;
    protected Player.Direction _rotation = Player.Direction.None;
    
    protected List<IEnemy> _enemies = new();
    protected List<Projectile> _projectiles = new();

    protected readonly List<ColoredLine> _collidable;
    protected readonly List<ColoredLine> _nonCollidable;
    protected readonly EncounterData.GravityType _gravityType;
    protected readonly Vector2 _gravityCenter; // only used for spherical gravity
    protected const float GRAVITY_CONSTANT = 75f;
    
    protected int _bonus { get; set; }

    protected readonly (float, float) _zoom;
    protected float zoomTimer;
    protected float deadTimer;
    
    protected readonly RectangleF _bounds;
    
    protected readonly EncounterData.EncounterType _type;

    protected int _playerBullets {
        get {
            return _projectiles.Count(p => p.alignment == Projectile.Alignment.Player);
        }
    }

    protected bool modulo => _gravityType is EncounterData.GravityType.NormalPlanar or EncounterData.GravityType.InversePlanar;

    /// <summary>
    /// Notifies the system that the encounter has ended, and to return control to the system.
    /// For planet and space encounters, this is set to true when the player leaves the encounter.
    ///
    /// For solar systems, this is set to true when the reactor is destroyed or all planets are destroyed,
    /// this then advances to the next solar system.
    /// </summary>
    public bool exitFlag { get; set; }

    public int bonus => (int) Math.Max(_bonus - Math.Floor(_time / 5) * 100, 0);
    public bool completed { get; private set; }


    public Encounter(EncounterData data, GraphicsResource spriteBatch) {
        _world = new World(new Image(spriteBatch.GraphicsDevice), new Camera(Vector2.Zero, 1));
        _inputManager = InputManager.getInputManager("planets/" + data.name);
        _shipAcceleration = data.shipAcceleration;
        _bonus = data.bonus;
        _collidable = data.getCollidable();
        _nonCollidable = data.getNonCollidable();
        _playerStartPos = new Vector2(data.playerPosition.Item1, data.playerPosition.Item2);
        
        _gravityType = data.gravityType;
        _gravityCenter = new Vector2(data.gravityCenter.Item1, data.gravityCenter.Item2);
        
        _zoom = data.zoom;

        foreach (Bunker newEnemy in data.enemies.Select(enemy => enemy.type switch {
                     EnemyData.Type.Bunker => new Bunker(enemy),
                     _ => null
                 }).Where(newEnemy => newEnemy != null)) {
            _enemies.Add(newEnemy);
        }

        _bounds = bounds(_collidable);

        if (_gravityType is EncounterData.GravityType.NormalPlanar or EncounterData.GravityType.InversePlanar) {
            _world.setModuloRender(_bounds.Width);
        }
        
        _type = data.encounterType;
    }
    
    public void Initialize() {
        _player = new Player(_shipAcceleration);
        _inputManager.setEnabled(true);
        
        _inputManager.OnHeld(Game1.Action1, () => {
            _thrusting = true;
        });
        _inputManager.OnReleased(Game1.Action1, () => {
            _thrusting = false;
        });
        
        _projectiles.Add(_player.shoot());
        _inputManager.OnPressed(Game1.Action2, () => {
            if (_playerBullets < 4) {
                _projectiles.Add(_player.shoot());
            }
        });
        
        _inputManager.OnHeld(Game1.Action3, () => {
            _shielding = true;
        });
        _inputManager.OnReleased(Game1.Action3, () => {
            _shielding = false;
        });

        _inputManager.OnHeld(Game1.Left, () => {
            _rotation = Player.Direction.Left;
        });
        
        _inputManager.OnHeld(Game1.Right, () => {
            _rotation = Player.Direction.Right;
        });
        
        _inputManager.OnHeld(!(Game1.Left | Game1.Right), () => {
            _rotation = Player.Direction.None;
        });
        
        _world.setLines(_collidable);
        _world.AddLines(_nonCollidable);

        zoomTimer = 0;
        _world.Camera.ZoomTo(_zoom.Item1);
        
        _player.Position = _playerStartPos;
    }
    
    public void Reinitialize() {
        _inputManager.setEnabled(true);

        zoomTimer = 0;
        _world.Camera.ZoomTo(_zoom.Item1);
        _world.Camera.Position = Vector2.Zero;
        
        _player.Position = _playerStartPos;
        _player.Velocity = Vector2.Zero;
    }
    
    public void Deinitialize() {
        _inputManager.setEnabled(false);
    }

    public void LoadContent() {
        // This space intentionally left blank
    }

    public void Update(GameTime gameTime) {
        _time += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        
        if (zoomTimer < 2) {
            zoomTimer += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            return; // Don't update anything else until the zoom is done
        }
        
        if (isColliding(_player.Position) && deadTimer < 0.01f) {
            // Console.WriteLine("Player hit terrain!");
            killPlayer();
        }
        
        foreach (IEnemy enemy in _enemies) {
            enemy.Update(gameTime);
            Projectile? p = enemy.shoot();
            if (p != null) {
                _projectiles.Add(p);
            }
        }
        
        foreach (Projectile p in _projectiles) {
            p.Update(gameTime);
            if (!modulo) continue;
            if (p.Position.X < _bounds.Left) {
                p.Position += new Vector2(_bounds.Width, 0);
            }
            if (p.Position.X > _bounds.Right) {
                p.Position -= new Vector2(_bounds.Width, 0);
            }
        }

        _projectiles = _projectiles.Where(p => !p.isExpired() && (p.alignment == Projectile.Alignment.Particle || !isColliding(p.Position))).ToList();

        if (deadTimer < 0.01f && !_shielding && _projectiles
            .Where(p => p.alignment == Projectile.Alignment.Enemy)
            .Any(p => Vector2.DistanceSquared(p.Position, _player.Position) < 100)) {
            // Console.WriteLine("Player hit by enemy projectile!");
            killPlayer();
        }
        
        _projectiles = _projectiles.Where(p => !(p.alignment == Projectile.Alignment.Enemy && _shielding && Vector2.DistanceSquared(p.Position, _player.Position) < 100)).ToList();
        
        if (_enemies.Count == 0) {
            completed = true;
        }

        if (deadTimer > 0) {
            deadTimer -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
            if (deadTimer < 0) {
                _world.Camera.Position = Vector2.Zero;
                zoomTimer = 0;
            }
            return;
        }
        
        foreach(IEnemy enemy in _enemies) {
            enemy.Destroyed = _projectiles.Where(p => p.alignment == Projectile.Alignment.Player)
                .Any(p => Vector2.DistanceSquared(p.Position, enemy.Position) <
                          enemy.BoundsRadius * enemy.BoundsRadius);
            
            if (!enemy.Destroyed) continue;
            
            _projectiles.AddRange(Projectile.explode(enemy.Position, 5));
            Game1.Instance.addScore(100);
        }
        
        _enemies = _enemies.Where(enemy => !enemy.Destroyed).ToList();

        if (!_enemies.Any(enemy => enemy is Bunker)) {
            completed = true;
        }
        
        _player.Update(gameTime, _thrusting, _rotation, _shielding);

        if (modulo) {
            if (_player.Position.X < _bounds.Left) {
                _player.Position += new Vector2(_bounds.Width, 0);
                _world.Camera.Position += new Vector2(_bounds.Width, 0);
            }
        
            if (_player.Position.X > _bounds.Right) {
                _player.Position -= new Vector2(_bounds.Width, 0);
                _world.Camera.Position -= new Vector2(_bounds.Width, 0);
            }
        }

        switch (_gravityType) {
            case EncounterData.GravityType.None:
                break;
            case EncounterData.GravityType.NormalPlanar:
                _player.applyForce(new Vector2(0, GRAVITY_CONSTANT * gameTime.ElapsedGameTime.Milliseconds / 1000f));
                break;
            case EncounterData.GravityType.NormalSpherical:
                Vector2 toCenter = _gravityCenter - _player.Position;
                toCenter.Normalize();
                _player.applyForce(toCenter * GRAVITY_CONSTANT * gameTime.ElapsedGameTime.Milliseconds / 1000f);
                break;
            case EncounterData.GravityType.InversePlanar:
                _player.applyForce(new Vector2(0, -GRAVITY_CONSTANT * gameTime.ElapsedGameTime.Milliseconds / 1000f));
                break;
            case EncounterData.GravityType.InverseSpherical:
                Vector2 toCenter2 = _gravityCenter - _player.Position;
                toCenter2.Normalize();
                _player.applyForce(toCenter2 * -GRAVITY_CONSTANT * gameTime.ElapsedGameTime.Milliseconds / 1000f);
                break;
        }
        
        if (_type == EncounterData.EncounterType.System) {
            return;
        }
        
        // If the player is too far outside the screen, exit the planet
        RectangleF visibilityBox = _world.Camera.WorldBounds;
        visibilityBox.Inflate(200, 200);
        if (!visibilityBox.Contains(new PointF(_player.Position.X, _player.Position.Y))) {
            exitFlag = true;
        }
    }

    public void BackgroundUpdate(GameTime gameTime) {
        // This space intentionally left blank
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        _world.Reset();
        
        if (deadTimer < 0.01f) {
            _world.Camera.ZoomTo(zoomAmount); // Putting this here makes the zoom freeze when the player dies

            const float edgeBuffer = 225;

            if (_gravityType is EncounterData.GravityType.NormalPlanar or EncounterData.GravityType.InversePlanar 
                && _world.Camera.WorldToScreen(_player.Position).X < edgeBuffer) {
                _world.Camera.Move(
                    new Vector2((_world.Camera.WorldToScreen(_player.Position) - new Vector2(edgeBuffer, 0)).X, 0));
            }
            
            if (_gravityType is EncounterData.GravityType.NormalPlanar or EncounterData.GravityType.InversePlanar 
                && _world.Camera.WorldToScreen(_player.Position).X > (Game1.Width / Game1.Scale) - edgeBuffer) {
                _world.Camera.Move(
                    new Vector2((_world.Camera.WorldToScreen(_player.Position) - new Vector2((Game1.Width / Game1.Scale) - edgeBuffer, 0)).X, 0));
            }
            
            _world.addTempLines(_player.getLines());
        }
        
        // _world.addTempLines(
        //     _projectiles.Where(p => p.alignment == Projectile.Alignment.Enemy)
        //         .SelectMany(p => rectangle(visibilityBox(p.Position)))
        //         .Select(l => new ColoredLine(l, Color.HotPink)));
        
        _world.addTempLines(_enemies.SelectMany(e => e.getLines()));
        
        _world.addTempLines(_projectiles.SelectMany(p => p.getLines()));

        _world.Draw(spriteBatch);
    }

    private bool isColliding(Vector2 pos) {
        Line testLine = new(pos, new Vector2(pos.X, _bounds.Bottom + 1));
        
        int collisions = _collidable.Count(line => line.Intersects(testLine));

        return collisions % 2 == 1;
    }

    protected static RectangleF bounds(IEnumerable<Line> lines) {
        (float minX, float minY, float maxX, float maxY) =
            (float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

        foreach (Line l in lines) {
            minX = Math.Min(minX, Math.Min(l.Start.X, l.End.X));
            maxX = Math.Max(maxX, Math.Max(l.Start.X, l.End.X));

            minY = Math.Min(minY, Math.Min(l.Start.Y, l.End.Y));
            maxY = Math.Max(maxY, Math.Max(l.Start.Y, l.End.Y));
        }

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    protected static RectangleF visibilityBox(Vector2 point) {
        return new RectangleF(point.X - 3, point.Y - 3, 6, 6);
    }
    
    protected static List<Line> rectangle(RectangleF bounds) {
        return new List<Line> {
            new(new Vector2(bounds.Left, bounds.Top), new Vector2(bounds.Right, bounds.Top)),
            new(new Vector2(bounds.Right, bounds.Top), new Vector2(bounds.Right, bounds.Bottom)),
            new(new Vector2(bounds.Right, bounds.Bottom), new Vector2(bounds.Left, bounds.Bottom)),
            new(new Vector2(bounds.Left, bounds.Bottom), new Vector2(bounds.Left, bounds.Top))
        };
    }

    protected void killPlayer() {
        deadTimer = 2;
        _projectiles.AddRange(Projectile.explode(_player.Position, 7));
        _player.Position = _playerStartPos;
        _player.Velocity = Vector2.Zero;
        _player.Rotation = 0;
        Game1.Instance.killPlayer();
    }

    private float zoomAmount {
        get {
            if (zoomTimer < 2) {
                return MathHelper.Lerp(_zoom.Item1, _zoom.Item2, MathF.Pow(zoomTimer / 2, 0.5f));
            }
            if (_gravityType is EncounterData.GravityType.None or EncounterData.GravityType.NormalSpherical or EncounterData.GravityType.InverseSpherical) {
                return _zoom.Item2;
            }
            
            return MathHelper.Lerp(_zoom.Item2, _zoom.Item2 * 2, 
                MathHelper.Clamp(
                    (_player.Position.Y + 600f) / 250f
                    , 0, 1));
        }
    }

    internal void setBonus(int bonus) {
        _bonus = bonus;
    }
    
    internal void addLine(ColoredLine line) {
        _world.AddLine(line);
    }
}