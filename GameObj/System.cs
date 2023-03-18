using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Gravitar.Data;
using Gravitar.Drawing;
using Gravitar.Shape;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Gravitar.GameObj;

public class System : Encounter {
    private readonly (List<ColoredLine>, float)[][] _animations;

    private float _animationTime;

    private readonly List<string> _planetNames;
    private readonly List<Encounter> _planets;
    private readonly GraphicsResource _graphicsResource;
    private List<RectangleF> _planetBounds;
    private int? _currentPlanet;

    private readonly List<Vector2> _planetPositions;

    private readonly List<Line> _playerSpawn = new(
        new[] {
            new Line(new Vector2(0, -30), new Vector2(22, -22)),
            new Line(new Vector2(22, -22), new Vector2(30, 0)),
            new Line(new Vector2(30, 0), new Vector2(22, 22)),
            new Line(new Vector2(22, 22), new Vector2(0, 30)),
            new Line(new Vector2(0, 30), new Vector2(-22, 22)),
            new Line(new Vector2(-22, 22), new Vector2(-30, 0)),
            new Line(new Vector2(-30, 0), new Vector2(-22, -22)),
            new Line(new Vector2(-22, -22), new Vector2(0, -30)),
        }
    );

    private readonly List<Line> _star = new(
        new[] {
            new Line(new Vector2(0, -30), new Vector2(7, -7)),
            new Line(new Vector2(7, -7), new Vector2(30, 0)),
            new Line(new Vector2(30, 0), new Vector2(7, 7)),
            new Line(new Vector2(7, 7), new Vector2(0, 30)),
            new Line(new Vector2(0, 30), new Vector2(-7, 7)),
            new Line(new Vector2(-7, 7), new Vector2(-30, 0)),
            new Line(new Vector2(-30, 0), new Vector2(-7, -7)),
            new Line(new Vector2(-7, -7), new Vector2(0, -30)),
        }
    );
    
    public new int bonus => _currentPlanet == null ? 0 : _planets[_currentPlanet.Value].bonus;

    public new bool completed {
        get {
            return !_planets.Any(p => p != null && !p.completed);
        }
    }

public System(SystemData systemData, EncounterData encounterData, GraphicsResource graphicsResource) : base(encounterData, graphicsResource) {
        this._animations = systemData.getAnimations();
        this._planetPositions = systemData.getPlanetPositions();
        this._planetNames = systemData.Planets.ToList();
        this._graphicsResource = graphicsResource;
        _planets = new List<Encounter>();
    }
    
    public new void Initialize() {
        _currentPlanet = null;
        
        _planetBounds = _planetPositions
            .Select(visibilityBox)
            .Select(rf => {
                rf.Inflate(25, 25);
                return rf;
            })
            .ToList();
        
        base.Initialize();
    }
    
    public new void Reinitialize() {
        _currentPlanet = null;
        base.Reinitialize();
    }
    
    public new void LoadContent() {
        for (int i = 0; i < _planetNames.Count; i++) {
            _planets.Add(null); // TODO: remove this line
            if (i != 3 && i != 0 && i != 1) continue; // TODO: remove this line (once all planets are implemented)
            _planets[i] = new Encounter(EncounterData.FromFile($"Data/Encounter/{_planetNames[i]}.json"), _graphicsResource);
            _planets[i].Initialize();
            _planets[i].Deinitialize(); // disable input manager
            _planets[i].LoadContent();
        }
        
        base.LoadContent();
    }

    public new void Update(GameTime gameTime) {
        _animationTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        
        if (_currentPlanet != null && _planets[_currentPlanet.Value].exitFlag) {
            _planets[_currentPlanet.Value].Deinitialize();
            _planets[_currentPlanet.Value].exitFlag = false;
            if (_planets[_currentPlanet.Value].completed) {
                Game1.Instance.addScore(_planets[_currentPlanet.Value].bonus);
                _planets[_currentPlanet.Value] = null;
                _projectiles.AddRange(Projectile.explode(_planetPositions[_currentPlanet.Value], 9));
                if (this.completed) {
                    // I don't expect anyone to ever clear all planets without dying
                    // but just in case
                    this.recreatePlanets();
                    Game1.Instance.addScore(10000); // because you deserve it
                }
            }
            Reinitialize();
        }
        
        if (_currentPlanet != null) {
            _planets[_currentPlanet.Value].Update(gameTime);
            return;
        }
        
        PointF _playerPos = new (_player.Position.X, _player.Position.Y);

        for (int i = 0; i < 5; i++) {
            if (!_planetBounds[i].Contains(_playerPos) || _planets[i] == null) {
                continue;
            }
            _currentPlanet = i;
            _planets[_currentPlanet.Value].Reinitialize();
            Deinitialize();
            break;
        }

        base.Update(gameTime);
    }
    
    public new void BackgroundUpdate(GameTime gameTime) {
        foreach (Encounter planet in _planets) {
            planet.BackgroundUpdate(gameTime);
        }
        
        base.BackgroundUpdate(gameTime);
    }
    
    public new void Draw(GameTime gameTime, SpriteBatch spriteBatch){
        if (_currentPlanet != null) {
            _planets[_currentPlanet.Value].Draw(gameTime, spriteBatch);
            return;
        }
        
        for (int i = 0; i < 5; i++) {
            if (_planets[i] == null) continue;
            _world.addTempLines(rectangle(_planetBounds[i]).Select(
                line => new ColoredLine(line, i switch {
                    0 => Color.Red,
                    1 => Color.Green,
                    2 => Color.Blue,
                    3 => Color.Yellow,
                    4 => Color.Purple,
                    _ => Color.White
                })
            ));
        }
        
        _world.addTempLines(_playerSpawn.Select(
            line => new ColoredLine(line.Start + _playerStartPos, line.End + _playerStartPos, Color.White)
        ));

        Color innerColor = ((int)(_animationTime * 8) % 8) switch {
            0 => Color.Red,
            1 => Color.Orange,
            2 => Color.Yellow,
            3 => Color.Green,
            4 => Color.Blue,
            5 => Color.Indigo,
            6 => Color.Violet,
            7 => Color.Purple,
            _ => Color.White
        };
        
        _world.addTempLines(_star.Select(
            line => new ColoredLine(line.Start + _gravityCenter, line.End + _gravityCenter, Color.Yellow)));
        
        _world.addTempLines(_playerSpawn.Select(
            line => new ColoredLine((line.Start * 0.75f) + _playerStartPos, (line.End * 0.75f) + _playerStartPos, innerColor)
            ));
        
        base.Draw(gameTime, spriteBatch);
    }

    private void recreatePlanets() {
        _planets.Clear();
        for (int i = 0; i < _planetNames.Count; i++) {
            _planets.Add(null); // TODO: remove this line
            if (i != 3 && i != 0) continue; // TODO: remove this line
            _planets[i] = new Encounter(EncounterData.FromFile($"Data/Encounter/{_planetNames[i]}.json"), _graphicsResource);
            _planets[i].Initialize();
            _planets[i].Deinitialize(); // disable input manager
            _planets[i].LoadContent();
        }
    }
}
