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
        private Rectangle menuButtonBounds;

        private int virtualWidth = 1280;
        private int virtualHeight = 720;

        private MainController game;

        private Color menuButtonColor = Color.Red;

        private Texture2D fadeTexture;

        private Point GetScaledMousePos(MouseState currentMouse)
        {
            Matrix inv = Matrix.Invert(game.UIScaleMatrix);
            Vector2 v = Vector2.Transform(currentMouse.Position.ToVector2(), inv);
            Point pos = new Point((int)v.X, (int)v.Y);
            return pos;
        }

        public PauseMenu(GraphicsDevice graphicsDevice, ContentManager content, MainController game) 
        {
            graphics = graphicsDevice;

            spriteFont = content.Load<SpriteFont>("bangers");

            this.game = game;

            fadeTexture = new Texture2D(graphics, 1, 1);
            fadeTexture.SetData(new[] { Color.Black });

            Vector2 textSize = spriteFont.MeasureString("Save And Return to Menu");
            menuButtonBounds = new Rectangle(virtualWidth/2 - 60, virtualHeight - 90, (int)textSize.X, (int)textSize.Y);
        }

        public bool CheckReturnBounds(MouseState currentMouse, MouseState previousMouse)
        {
            if (menuButtonBounds.Contains(GetScaledMousePos(currentMouse)))
            {
                menuButtonColor = Color.MediumVioletRed;
            }
            if (!menuButtonBounds.Contains(GetScaledMousePos(currentMouse)))
            {
                menuButtonColor = Color.Red;
            }

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                return menuButtonBounds.Contains(GetScaledMousePos(currentMouse));
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(fadeTexture, graphics.Viewport.Bounds, Color.Black * 0.7f);

            string pauseMenuText = "GAME PAUSED\n" +
                              "Press SPACE to Resume\n\n" +
                              "CONTROLS:\n" +
                              "Right Click: Move Camera\n" +
                              "Scroll Wheel: Zoom in/out\n" +
                              "Left Click: Select Layer\n" +
                              "A/D or Left/Right: Rotate";

            Vector2 textSize = spriteFont.MeasureString(pauseMenuText);
            Vector2 center = new Vector2(virtualWidth / 2, virtualHeight / 2);

            spriteBatch.DrawString(spriteFont, pauseMenuText,center - textSize / 2 - new Vector2(0, 50), Color.White);

            textSize = spriteFont.MeasureString("Save And Return to Menu");
            spriteBatch.DrawString(spriteFont, "Save And Return to Menu", new Vector2(center.X - textSize.X / 2, virtualHeight - 25 - textSize.Y), menuButtonColor);
        }
    }
}