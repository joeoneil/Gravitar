using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Gravitar.Drawing;
using Microsoft.Xna.Framework;

namespace Gravitar.Data; 

/// <summary>
/// Stores information about a solar system, including the planets animation, the player's starting position, and the
/// names of the planets in the system. SystemData is loaded from JSON files in the Content/Data/Systems folder.
///
/// Systems are constructed from SystemData and EncounterData
/// </summary>
public class SystemData {
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; init; }
    public string[] Planets { get; init; }

    /// <summary>
    /// A list of planet animations, each planet animation is a list of frames, each frame is a list of lines, each line
    /// is a tuple of (x1, y1, x2, y2, (r, g, b))
    ///
    /// Each list also has a float at the end, which is the time that the frame should be displayed for.
    ///
    /// Frame 0 contains the lines that should be drawn on every frame.
    /// </summary>
    public (List<(float, float, float, float, (float, float, float))>, float)[][] PlanetAnimations { get; set; } = new (List<(float, float, float, float, (float, float, float))>, float)[5][];

    private List<List<(List<ColoredLine>, float)>> _planetAnimations = new(5);
    
    public (float, float)[] PlanetPositions { get; init; } = new (float, float)[5];
    
    public static SystemData FromFile(string path) {
        string json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize(json, typeof(SystemData), new JsonSerializerOptions {
            IncludeFields = true,
        }) as SystemData ?? throw new Exception("Failed to deserialize SystemData from file");
    }

    public (List<ColoredLine>, float)[][] getAnimations() {
        var animations = new (List<ColoredLine>, float)[5][];
        for (int i = 0; i < 5; i++) {
            animations[i] = new (List<ColoredLine>, float)[PlanetAnimations[i].Length];
            for (int j = 0; j < PlanetAnimations[i].Length; j++) {
                animations[i][j] = new ValueTuple<List<ColoredLine>, float>(new List<ColoredLine>(), PlanetAnimations[i][j].Item2);
                foreach ((float, float, float, float, (float, float, float)) line in PlanetAnimations[i][j].Item1) {
                    animations[i][j].Item1.Add(new ColoredLine(
                        new Vector2(line.Item1, line.Item2),
                        new Vector2(line.Item3, line.Item4),
                        new Color(line.Item5.Item1, line.Item5.Item2, line.Item5.Item3)
                    ));
                }
            }
        }
        return animations;
    }

    internal void initializeAnimations() {
        _planetAnimations = new List<List<(List<ColoredLine>, float)>>();
        for (int i = 0; i < 5; i++) {
            _planetAnimations.Add(new List<(List<ColoredLine>, float)>());
        }
    }

    internal void finalizeAnimations() {
        PlanetAnimations = new (List<(float, float, float, float, (float, float, float))>, float)[5][];
        for (int i = 0; i < 5; i++) {
            PlanetAnimations[i] = new (List<(float, float, float, float, (float, float, float))>, float)[_planetAnimations[i].Count];
            for (int j = 0; j < _planetAnimations[i].Count; j++) {
                PlanetAnimations[i][j] = 
                    new ValueTuple<List<(float, float, float, float, (float, float, float))>, float>
                        (new List<(float, float, float, float, (float, float, float))>(), _planetAnimations[i][j].Item2);
                foreach (ColoredLine line in _planetAnimations[i][j].Item1) {
                    PlanetAnimations[i][j].Item1.Add((line.Start.X, line.Start.Y, line.End.X, line.End.Y, (line.Color.R / 255, line.Color.G / 255, line.Color.B / 255)));
                }
            }
        }
    }
    
    public List<Vector2> getPlanetPositions() {
        return PlanetPositions.Select(position => new Vector2(position.Item1, position.Item2)).ToList();
    }
    
    internal void addLine(ColoredLine line, int planet, int frame) {
        (float x1, float y1, float x2, float y2, (float r, float g, float b)) = 
            (line.Start.X, line.Start.Y, 
                line.End.X, line.End.Y, 
                (
                    (float)line.Color.R / 255, 
                    (float)line.Color.G / 255, 
                    (float)line.Color.B / 255
                )
            );
            PlanetAnimations[planet][frame].Item1.Add((x1, y1, x2, y2, (r, g, b)));
    }
}