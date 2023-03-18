using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gravitar; 

public interface IMenu {
    public void Initialize();
    
    public void Reinitialize();
    
    public void Deinitialize();
    
    public void LoadContent();
    
    public void Update(GameTime gameTime);
    
    public void BackgroundUpdate(GameTime gameTime);

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}