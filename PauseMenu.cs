using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GameProject6
{
    public class PauseMenu
    {
        private SpriteFont spriteFont;
        private GraphicsDevice graphics;
        private Rectangle quitButtonBounds;

        private int width;
        private int height;

        private Color quitColor = Color.Red;

        public PauseMenu(GraphicsDevice graphicsDevice, ContentManager content) 
        {
            graphics = graphicsDevice;

            spriteFont = content.Load<SpriteFont>("bangers");

            width = graphics.Viewport.Width;
            height = graphics.Viewport.Height;

            quitButtonBounds = new Rectangle(width/2 - 60, height - 90, 110, 60);
        }

        public bool CheckQuitBounds(MouseState currentMouse, MouseState previousMouse)
        {
            if (quitButtonBounds.Contains(currentMouse.Position))
            {
                quitColor = Color.MediumVioletRed;
            }
            if (!quitButtonBounds.Contains(currentMouse.Position))
            {
                quitColor = Color.Red;
            }

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                return quitButtonBounds.Contains(currentMouse.Position);
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D fadeTexture = new Texture2D(graphics, 1, 1);
            fadeTexture.SetData(new[] { Color.Black });
            spriteBatch.Draw(fadeTexture, graphics.Viewport.Bounds, Color.Black * 0.7f);

            string pauseMenuText = "GAME PAUSED\n" +
                              "Press SPACE to Resume\n\n" +
                              "CONTROLS:\n" +
                              "Right Click: Move Camera\n" +
                              "Scroll Wheel: Zoom in/out\n" +
                              "Left Click: Select Layer\n" +
                              "A/D or Left/Right: Rotate";

            Vector2 textSize = spriteFont.MeasureString(pauseMenuText);
            Vector2 center = new Vector2(width / 2, height / 2);

            spriteBatch.DrawString(spriteFont, pauseMenuText,center - textSize / 2 - new Vector2(0, 50), Color.White);

            textSize = spriteFont.MeasureString("QUIT");
            spriteBatch.DrawString(spriteFont, "QUIT", new Vector2(center.X - textSize.X / 2, height - 25 - textSize.Y), quitColor);
        }
    }
}