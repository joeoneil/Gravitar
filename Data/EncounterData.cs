using System.Collections.Generic;
using System.Text.Json;
using Gravitar.Drawing;
using Microsoft.Xna.Framework;

namespace Gravitar.Data; 

/// <summary>
/// A stripped down version of the Encounter class, designed to be serialized and deserialized.
/// </summary>
public class EncounterData {
    public string name { get; set; }
    
    public List<(float, float, float, float, (float, float, float))> collidable { get; set; }
    public List<(float, float, float, float, (float, float, float))> nonCollidable { get; set; }
    
    public List<EnemyData> enemies { get; set; }
    
    public (float, float) playerPosition { get; set; }
    public float shipAcceleration { get; set; }
    public GravityType gravityType { get; set; }
    public (float, float) gravityCenter { get; set; }
    
    public int bonus { get; set; }
    
    // starting and ending zoom for the camera when the encounter is loaded
    public (float, float) zoom { get; set; }
    
    public EncounterType encounterType { get; set; }
    
    public EncounterData() {
        this.name = "";
        this.collidable = new List<(float, float, float, float, (float, float, float))>();
        this.nonCollidable = new List<(float, float, float, float, (float, float, float))>();
        
        this.enemies = new List<EnemyData>();
        
        this.shipAcceleration = 0;
        this.gravityType = GravityType.None;
    }
    
    public static EncounterData FromFile(string path) {
        string json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize(json, typeof(EncounterData), new JsonSerializerOptions {
            IncludeFields = true,
        }) as EncounterData ?? throw new System.Exception("Failed to deserialize EncounterData from file");
    }

    public enum GravityType {
        None,
        NormalPlanar,
        NormalSpherical,
        InversePlanar,
        InverseSpherical,
    }

    public enum EncounterType {
        Planet,
        System,
        Reactor,
    }

    internal void addLine(ColoredLine line, bool collidable = true) {
        (float x1, float y1, float x2, float y2, (float r, float g, float b)) = 
            (line.Start.X, line.Start.Y, 
                line.End.X, line.End.Y, 
                (
                    (float)line.Color.R / 255, 
                    (float)line.Color.G / 255, 
                    (float)line.Color.B / 255)
                );
        if (collidable) {
            this.collidable.Add((x1, y1, x2, y2, (r, g, b)));
        } else {
            this.nonCollidable.Add((x1, y1, x2, y2, (r, g, b)));
        }
    }
    
    internal void addEnemy(EnemyData enemy) {
        enemies.Add(enemy);
    }

    public List<ColoredLine> getCollidable() {
        var lines = new List<ColoredLine>();
        foreach ((float x1, float y1, float x2, float y2, (float r, float g, float b)) in collidable) {
            lines.Add(new ColoredLine(new Vector2(x1, y1), new Vector2(x2, y2), new Color(r, g, b)));
        }
        return lines;
    }

    public List<ColoredLine> getNonCollidable() {
        var lines = new List<ColoredLine>();
        foreach ((float x1, float y1, float x2, float y2, (float r, float g, float b)) in nonCollidable) {
            lines.Add(new ColoredLine(new Vector2(x1, y1), new Vector2(x2, y2), new Color(r, g, b)));
        }
        return lines;
    }
}