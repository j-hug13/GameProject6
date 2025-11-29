using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject6
{
    public class SettingsScreen
    {
        private MouseState currentMouse;
        private MouseState previousMouse;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;

        private Rectangle backButtonBounds;
        private Rectangle onButtonBounds;
        private Rectangle offButtonBounds;

        private SpriteFont spriteFont;

        private MainController game;

        public void LoadContent(ContentManager content, MainController game, GraphicsDevice graphics)
        {
            Vector2 center = new Vector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
            this.game = game;
            spriteFont = content.Load<SpriteFont>("bangers");

            Vector2 backSize = spriteFont.MeasureString("Back");
            backButtonBounds = new Rectangle(20, 20, (int)backSize.X, (int)backSize.Y);

            Vector2 onSize = spriteFont.MeasureString("On");
            onButtonBounds = new Rectangle((int)center.X - 100, (int)center.Y, (int)onSize.X, (int)onSize.Y);

            Vector2 offSize = spriteFont.MeasureString("Off");
            offButtonBounds = new Rectangle((int)center.X + 50, (int)center.Y, (int)offSize.X, (int)offSize.Y);
        }

        public GameState? Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            previousKeyboard = currentKeyboard;
            currentKeyboard = Keyboard.GetState();

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (backButtonBounds.Contains(currentMouse.Position))
                {
                    return GameState.Menu;
                }

                if (onButtonBounds.Contains(currentMouse.Position))
                {
                    //graphics.ToggleFullScreen();
                }

                if (offButtonBounds.Contains(currentMouse.Position))
                {
                    //graphics.ToggleFullScreen();
                }
            }
            if (currentKeyboard.IsKeyDown(Keys.B) && previousKeyboard.IsKeyUp(Keys.B))
            {
                if (graphics.IsFullScreen == false)
                {
                    game.SetFullscreen(true);
                }
            }
            if (currentKeyboard.IsKeyDown(Keys.N) && previousKeyboard.IsKeyUp(Keys.N))
            {
                if (graphics.IsFullScreen == true)
                {
                    game.SetFullscreen(false);
                }
            }

            return null;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            graphics.Clear(new Color(176, 124, 196));

            spriteBatch.Begin();

            Color backButtonColor;
            if (backButtonBounds.Contains(currentMouse.Position) == true)
            {
                backButtonColor = Color.Gold;
            }
            else
            {
                backButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Back", new Vector2(20, 20), backButtonColor);

            Vector2 center = new Vector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
            Vector2 fullscreenSize = spriteFont.MeasureString("Fullscreen");
            spriteBatch.DrawString(spriteFont, "Fullscreen", new Vector2(center.X - (fullscreenSize.X / 2), center.Y - 200), Color.White);

            Color onButtonColor;
            if (onButtonBounds.Contains(currentMouse.Position) == true)
            {
                onButtonColor = Color.Gold;
            }
            else
            {
                onButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "On", new Vector2(center.X - 100, center.Y), onButtonColor);

            Color offButtonColor;
            if (offButtonBounds.Contains(currentMouse.Position) == true)
            {
                offButtonColor = Color.Gold;
            }
            else
            {
                offButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Off", new Vector2(center.X + 50, center.Y), offButtonColor);

            spriteBatch.End();
        }
    }
}
