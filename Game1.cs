using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Devcade.events;
using Gravitar.Data;
using Gravitar.Drawing;
using Gravitar.GameObj;
using Gravitar.Shape;
using Microsoft.Xna.Framework.Input;

namespace Gravitar;

public class Game1 : Game {
    public static Game1 Instance { get; private set; }

    public static readonly CButton Left = CButton.or(Keys.A, Keys.Left, Input.ArcadeButtons.StickLeft);
    public static readonly CButton Right = CButton.or(Keys.D, Keys.Right, Input.ArcadeButtons.StickRight);
    public static readonly CButton Up = CButton.or(Keys.W, Keys.Up, Input.ArcadeButtons.StickUp);
    public static readonly CButton Down = CButton.or(Keys.S, Keys.Down, Input.ArcadeButtons.StickDown);
    public static readonly CButton Action1 = CButton.or(Keys.Z, Input.ArcadeButtons.A1);
    public static readonly CButton Action2 = CButton.or(Keys.X, Input.ArcadeButtons.A2);
    public static readonly CButton Action3 = CButton.or(Keys.C, Input.ArcadeButtons.A3);
    public static readonly CButton Menu = CButton.or(Keys.Space, Input.ArcadeButtons.Menu);
    
    public SpriteFont Font { get; private set; }

    private Image _ui;

    private int _lives = 4;
    private int _score;
    private int _nextLife = 10000;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static int Width { get; private set; }
    public static int Height { get; private set; }
    public static float Scale { get; private set; }
    
    private readonly InputManager _inputManager;
    
    private GameObj.System _world;

    private static float x_s = 1;
    private static float y_s = 1;
    private static float x_t;
    private static float y_t;

    public Game1() {
        // Code that manually creates the terrain and writes it to a file
        // In production this would be loaded from a file
        development_code();

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.IsBorderless = true;

        _inputManager = InputManager.getInputManager("root");
        
        int maxWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int maxHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        int maxWidthByHeight = (int)(maxHeight * 9f / 21f);
        int maxHeightByWidth = (int)(maxWidth * 21f / 9f);
        
        Width = maxWidthByHeight < maxWidth ? maxWidthByHeight : maxWidth;
        Height = maxHeightByWidth < maxHeight ? maxHeightByWidth : maxHeight;
        Scale = (Width / 1080f) * 2f;
        
        _graphics.PreferredBackBufferWidth = Width;
        _graphics.PreferredBackBufferHeight = Height;
        _graphics.ApplyChanges();

        _ui = new Image(GraphicsDevice);

        Instance = this;
    }

    protected override void Initialize() {
        InputManager.OnPressedGlobal(CButton.exit, Exit);
        
        InputManager.OnPressedGlobal(Menu, () => {
            if (_lives == 0) {
                reset();
            }
        });

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _world = new GameObj.System(
            SystemData.FromFile("Data/System/system1.json"),
            EncounterData.FromFile("Data/Encounter/system1.json"),
            _spriteBatch
        );
        
        Font = Content.Load<SpriteFont>("AlphaProta");
        Font.Spacing = 1.5f;
        
        _world.Initialize();
        _world.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        InputManager.Update();

        if (_lives > 0) {
            _world.Update(gameTime);
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        
        if (_lives > 0) {
                _world.Draw(gameTime, _spriteBatch);

                _ui.Fill(Color.Transparent);

                Vector2 shipOffset = new Vector2(40, 50);
                Vector2 shipSize = new Vector2(40, 10);
                for (int i = 0; i < _lives - 1; i++) {
                    List<Line> shipLines = Player.defaultLines;
                    foreach (Line line in shipLines) {
                        _ui.drawLine(
                            new ColoredLine(
                                line.Start * 2 + shipOffset,
                                line.End * 2 + shipOffset,
                                Color.Blue
                            )
                        );
                    }

                    shipOffset += shipSize;
                }

                _spriteBatch.Draw(_ui.Texture, new Rectangle(0, 0, Width, Height), Color.White);

                // _spriteBatch.DrawString(Font, $"Score: {_score}", new Vector2(10, 10), Color.White);
                _spriteBatch.DrawString(Font, "Score", new Vector2(Width - 200, 10), Color.White, 0, Vector2.Zero,
                    Scale * 1.5f, SpriteEffects.None, 0);
                _spriteBatch.DrawString(Font, $"{_score,-5}", new Vector2(Width - 300, 10), Color.Cyan, 0, Vector2.Zero,
                    Scale * 1.5f, SpriteEffects.None, 0);

                _spriteBatch.DrawString(Font, "Next Life", new Vector2(Width - 200, 50), Color.White, 0, Vector2.Zero,
                    Scale * 1.5f, SpriteEffects.None, 0);
                _spriteBatch.DrawString(Font, $"{_nextLife,-5}", new Vector2(Width - 300, 50), Color.Cyan, 0,
                    Vector2.Zero, Scale * 1.5f, SpriteEffects.None, 0);

                _spriteBatch.DrawString(Font, "Bonus", new Vector2(Width - 200, 90), Color.White, 0, Vector2.Zero,
                    Scale * 1.5f, SpriteEffects.None, 0);
                _spriteBatch.DrawString(Font, $"{_world.bonus,-5}", new Vector2(Width - 300, 90), Color.Cyan, 0,
                    Vector2.Zero, Scale * 1.5f, SpriteEffects.None, 0);

                _spriteBatch.End();
        } else {
            _spriteBatch.DrawString(Font, "Game Over", new Vector2(Width / 2f, Height / 2f), Color.White, 0, Font.MeasureString("Game Over") / 2, Scale * 1.5f, SpriteEffects.None, 0);
            _spriteBatch.DrawString(Font, $"Score: {_score}", new Vector2(Width / 2f, Height / 2 + 50), Color.White, 0, Font.MeasureString($"Score: {_score}") / 2, Scale * 1.5f, SpriteEffects.None, 0);
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    private static ColoredLine terrain(float x1, float y1, float x2, float y2) {
        return new ColoredLine(scale(new Vector2(x1, y1)), scale(new Vector2(x2, y2)), Color.Green);
    }

    private static EnemyData bunker(float x, float y, float rotation) {
        return new EnemyData {
            type = EnemyData.Type.Bunker,
            position = scale(new Vector2(x, y)),
            rotation = rotation,
        };
    }

    private static Vector2 scale(Vector2 input) {
        return new Vector2((input.X + x_t) * x_s, (input.Y + y_t) * y_s);
    }
    
    private static void configScale(float x_scale, float y_scale, float x_translate, float y_translate) {
        x_s = x_scale;
        y_s = y_scale;
        x_t = x_translate;
        y_t = y_translate;
    }

    internal void development_code() {
        EncounterData p14ed = EncounterData.FromFile("Data/Encounter/planet1-4.json");
        p14ed.encounterType = EncounterData.EncounterType.Planet;
        File.WriteAllText("Data/Encounter/planet1-4.json", JsonSerializer.Serialize(p14ed, new JsonSerializerOptions {
            IncludeFields = true,
        }));

        EncounterData p12ed = new EncounterData {
            bonus = 4000,
            encounterType = EncounterData.EncounterType.Planet,
            name = "planet1-2",
            zoom = (0.125f, 0.55f),
            shipAcceleration = 200f,
            playerPosition = (0, -800),
            gravityType = EncounterData.GravityType.NormalPlanar,
            gravityCenter = (0, 0), // Doesn't matter
        };

        configScale(2f, 2f, -590, -216);

        p12ed.addLine(terrain(111, 204, 126, 204));
        p12ed.addLine(terrain(126, 204, 147, 187));
        p12ed.addLine(terrain(147, 187, 168, 220));
        p12ed.addLine(terrain(168, 220, 147, 248));
        p12ed.addLine(terrain(147, 248, 224, 248));
        p12ed.addLine(terrain(224, 248, 268, 193));
        p12ed.addLine(terrain(268, 193, 226, 160));
        p12ed.addLine(terrain(226, 160, 247, 149));
        p12ed.addLine(terrain(247, 149, 260, 149));
        p12ed.addLine(terrain(260, 149, 310, 189));
        p12ed.addLine(terrain(310, 189, 282, 220));
        p12ed.addLine(terrain(282, 220, 338, 220));
        p12ed.addLine(terrain(338, 220, 359, 187));
        p12ed.addLine(terrain(359, 187, 339, 149));
        p12ed.addLine(terrain(339, 149, 351, 148));
        p12ed.addLine(terrain(351, 148, 394, 166));
        p12ed.addLine(terrain(394, 166, 352, 236));
        p12ed.addLine(terrain(352, 236, 394, 259));
        p12ed.addLine(terrain(394, 259, 448, 259));
        p12ed.addLine(terrain(448, 259, 484, 204));
        p12ed.addLine(terrain(484, 204, 450, 166));
        p12ed.addLine(terrain(450, 166, 505, 182));
        p12ed.addLine(terrain(505, 182, 560, 216));
        p12ed.addLine(terrain(560, 216, 617, 216));
        p12ed.addLine(terrain(617, 216, 671, 183));
        p12ed.addLine(terrain(671, 183, 725, 204));
        p12ed.addLine(terrain(725, 204, 780, 253));
        p12ed.addLine(terrain(780, 253, 923, 253));
        p12ed.addLine(terrain(923, 253, 891, 205));
        p12ed.addLine(terrain(891, 205, 916, 205));
        p12ed.addLine(terrain(916, 205, 944, 162));
        p12ed.addLine(terrain(944, 162, 984, 204));
        p12ed.addLine(terrain(984, 204, 997, 204));
        p12ed.addLine(new ColoredLine(scale(new Vector2(111, 1000)), scale(new Vector2(111, 204)), Color.Black));
        p12ed.addLine(new ColoredLine(scale(new Vector2(111, 1000)), scale(new Vector2(997, 1000)), Color.Black));
        p12ed.addLine(new ColoredLine(scale(new Vector2(997, 1000)), scale(new Vector2(997, 204)), Color.Black));
        
        // Island
        p12ed.addLine(terrain(781, 155, 889, 155));
        p12ed.addLine(terrain(889, 155, 835, 215));
        p12ed.addLine(terrain(835, 215, 794, 204));
        p12ed.addLine(terrain(794, 204, 807, 182));
        p12ed.addLine(terrain(807, 182, 781, 155));

        p12ed.addEnemy(bunker(297, 177,  0.67474094f));
        p12ed.addEnemy(bunker(460, 168,  0.28309578f));
        p12ed.addEnemy(bunker(650, 195, -0.54581513f));
        
        File.WriteAllText("Data/Encounter/planet1-2.json", JsonSerializer.Serialize(p12ed, new JsonSerializerOptions {
            IncludeFields = true,
        }));
        
        var sd = new SystemData {
            Name = "System1",
            Planets = new[] { "planet1-1", "planet1-2", "planet1-3", "planet1-4", "planet1-5" },
        };

        sd.PlanetPositions[0] = (170, -300);
        sd.PlanetPositions[1] = (160, 310);
        sd.PlanetPositions[2] = (0, 0);
        sd.PlanetPositions[3] = (-170, -250);
        sd.PlanetPositions[4] = (0, 0);
        
        sd.initializeAnimations();
        
        // Animation data will go between here
        
        sd.finalizeAnimations();

        var ed = new EncounterData {
            name = "System1",
            zoom = (0.2f, 1.0f),
            shipAcceleration = 200f,
            playerPosition = (0, 0),
            gravityType = EncounterData.GravityType.NormalSpherical,
            gravityCenter = (70, -200),
        };

        string sdJSON = JsonSerializer.Serialize(sd, new JsonSerializerOptions {
            IncludeFields = true,
        });
        string edJSON = JsonSerializer.Serialize(ed, new JsonSerializerOptions {
            IncludeFields = true,
        });
        
        File.WriteAllText("Data/System/system1.json", sdJSON);
        File.WriteAllText("Data/Encounter/system1.json", edJSON);
    }

    public void killPlayer() {
        _lives -= 1;
    }
    
    public void addScore(int score) {
        _score += score;
        if (_score <= _nextLife) return;
        _lives += 1;
        _nextLife *= 2;
    }

    private void reset() {
        _lives = 3;
        _score = 0;
        _nextLife = 10000;
        
        _world = new GameObj.System(
            SystemData.FromFile("Data/System/system1.json"),
            EncounterData.FromFile("Data/Encounter/system1.json"),
            _spriteBatch
        );
        
        _world.Initialize();
        _world.LoadContent();
    }
}