using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace universe
{
   public class Cursor : DrawableGameComponent
    {
        #region Fields and Properties

        // this constant controls how fast the gamepad moves the cursor. this constant
        // is in pixels per second.
        const float CursorSpeed = 400.0f;

        // this spritebatch is created internally, and is used to draw the cursor.
        SpriteBatch spriteBatch;

        // this is the sprite that is drawn at the current cursor position.
        // textureCenter is used to center the sprite when drawing.
        Texture2D cursorTexture;
        Vector2 textureCenter;

        // Position is the cursor position, and is in screen space. 
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }
        #endregion

       public Cursor(Game game):base(game)
        {

        }

       protected override void LoadContent()
       {
           cursorTexture = Game.Content.Load<Texture2D>("Textures\\star");
           textureCenter = new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2);

           spriteBatch = new SpriteBatch(GraphicsDevice);

           // we want to default the cursor to start in the center of the screen
           Viewport vp = GraphicsDevice.Viewport;
           position.X = vp.X + (vp.Width / 2);
           position.Y = vp.Y + (vp.Height / 2);
           base.LoadContent();
       }

       public override void Update(GameTime gameTime)
       {
           MouseState mouseState = Mouse.GetState();
           position.X = mouseState.X;
           position.Y = mouseState.Y;
       }

       public override void Draw(GameTime gameTime)
       {
           spriteBatch.Begin();

           // use textureCenter as the origin of the sprite, so that the cursor is 
           // drawn centered around Position.
           spriteBatch.Draw(cursorTexture, Position, null, Color.White, 0.0f,
               textureCenter, 1.0f, SpriteEffects.None, 0.0f);

           spriteBatch.End();
       }

       public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
       {
           // create 2 positions in screenspace using the cursor position. 0 is as
           // close as possible to the camera, 1 is as far away as possible.
           Vector3 nearSource = new Vector3(Position, 0f);
           Vector3 farSource = new Vector3(Position, 1f);

           // use Viewport.Unproject to tell what those two screen space positions
           // would be in world space. we'll need the projection matrix and view
           // matrix, which we have saved as member variables. We also need a world
           // matrix, which can just be identity.
           Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource,
               projectionMatrix, viewMatrix, Matrix.Identity);

           Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farSource,
               projectionMatrix, viewMatrix, Matrix.Identity);

           // find the direction vector that goes from the nearPoint to the farPoint
           // and normalize it....
           Vector3 direction = farPoint - nearPoint;
           direction.Normalize();

           // and then create a new ray using nearPoint as the source.
           return new Ray(nearPoint, direction);
       }
    }
}
