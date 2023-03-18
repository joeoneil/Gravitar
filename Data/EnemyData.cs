
using Microsoft.Xna.Framework;

namespace Gravitar.Data; 

public class EnemyData {
    public enum Type {
        Bunker,
    }
    
    public Type type { get; init; }
    public Vector2 position { get; init; }
    public float rotation { get; init; }
    
    public float? fireRate { get; init; }
    
    public EnemyData() {
        this.type = Type.Bunker;
        this.position = Vector2.Zero;
        this.rotation = 0;
        this.fireRate = 1;
    }
}