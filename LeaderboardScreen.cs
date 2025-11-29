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
    public class LeaderboardScreen
    {
        private MouseState currentMouse;
        private MouseState previousMouse;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;

        private SpriteFont spriteFont;

        private Rectangle backButtonBounds;
        private Rectangle rightArrowBounds;
        private Rectangle leftArrowBounds;

        private MainController game;

        private CubeType currentCube = CubeType.Cube3x3;

        private float scrollOffset = 0f;
        private float maxScroll = 0f;
        private const float IndividualEntryHeight = 70f;

        public void LoadContent(ContentManager content, MainController game, GraphicsDevice graphics)
        {
            this.game = game;
            spriteFont = content.Load<SpriteFont>("bangers");

            Vector2 backSize = spriteFont.MeasureString("Back");
            backButtonBounds = new Rectangle(20, 20, (int)backSize.X, (int)backSize.Y);

            Vector2 arrowSize = spriteFont.MeasureString("<");
            leftArrowBounds = new Rectangle(30, (graphics.Viewport.Height - (int)arrowSize.Y) / 2, (int)arrowSize.X, (int)arrowSize.Y);
            arrowSize = spriteFont.MeasureString(">");
            rightArrowBounds = new Rectangle(graphics.Viewport.Width - (int)arrowSize.X - 30, (graphics.Viewport.Height - (int)arrowSize.Y) / 2, (int)arrowSize.X, (int)arrowSize.Y);
        }

        public GameState? Update(GameTime gameTime)
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

                if (rightArrowBounds.Contains(currentMouse.Position))
                {
                    GetCurrentCube(1);
                }

                if (leftArrowBounds.Contains(currentMouse.Position))
                {
                    GetCurrentCube(-1);
                }
            }

            if ((currentKeyboard.IsKeyDown(Keys.Left) && previousKeyboard.IsKeyUp(Keys.Left)) || (currentKeyboard.IsKeyDown(Keys.A) && previousKeyboard.IsKeyUp(Keys.A)))
            {
                GetCurrentCube(-1);
            }
            if ((currentKeyboard.IsKeyDown(Keys.Right) && previousKeyboard.IsKeyUp(Keys.Right)) || (currentKeyboard.IsKeyDown(Keys.D) && previousKeyboard.IsKeyUp(Keys.D)))
            {
                GetCurrentCube(1);
            }

            int scroll = currentMouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
            if (currentKeyboard.IsKeyDown(Keys.Up) || currentKeyboard.IsKeyDown(Keys.W))
            {
                scroll += 50;
            }
            if (currentKeyboard.IsKeyDown(Keys.Down) || currentKeyboard.IsKeyDown(Keys.S))
            {
                scroll -= 50;
            }
            if (scroll != 0)
            {
                scrollOffset -= scroll * 0.25f;
                if (scrollOffset < 0)
                {
                    scrollOffset = 0;
                }
                if (scrollOffset > maxScroll)
                {
                    scrollOffset = maxScroll;
                }
            }

            return null;
        }

        private void GetCurrentCube(int direction)
        {
            int t = (int)currentCube + direction;
            if (t < 0)
            {
                t = 2;
            }
            if (t > 2)
            {
                t = 0;
            }

            currentCube = (CubeType)t;
            scrollOffset = 0;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            graphics.Clear(new Color(176, 124, 196));

            var leaderboard = game.GetLeaderboard(currentCube);
            int count = leaderboard.Entries.Count;
            if(leaderboard.Entries.Count < 10)
            {
                count = 10;
            }

            maxScroll = Math.Max(0, (count * IndividualEntryHeight) - (graphics.Viewport.Height - 200));

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

            Vector2 arrowSize = spriteFont.MeasureString("<");
            Color leftArrowColor;
            if (leftArrowBounds.Contains(currentMouse.Position) == true)
            {
                leftArrowColor = Color.Gold;
            }
            else
            {
                leftArrowColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "<", new Vector2(30, (graphics.Viewport.Height - arrowSize.Y) / 2), leftArrowColor);

            Color rightArrowColor;
            if (rightArrowBounds.Contains(currentMouse.Position) == true)
            {
                rightArrowColor = Color.Gold;
            }
            else
            {
                rightArrowColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, ">", new Vector2(graphics.Viewport.Width - arrowSize.X - 30, (graphics.Viewport.Height - arrowSize.Y) / 2), rightArrowColor);


            string titleText = "";
            if (currentCube == CubeType.Cube2x2)
            {
                titleText = "2x2 Cube Leaderboard";
            }
            else if (currentCube == CubeType.Cube3x3)
            {
                titleText = "3x3 Cube Leaderboard";
            }
            else if (currentCube == CubeType.Cube4x4)
            {
                titleText = "4x4 Cube Leaderboard";
            }
            Vector2 titleSize = spriteFont.MeasureString(titleText);
            spriteBatch.DrawString(spriteFont, titleText, new Vector2((graphics.Viewport.Width - titleSize.X) / 2, 20), Color.Gold);

            spriteBatch.End();

            RasterizerState rs = new RasterizerState() { ScissorTestEnable = true };
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, rs);

            // scroll bounds
            int clipX = 150;
            int clipY = 120;                          
            int clipWidth = graphics.Viewport.Width - 100; 
            int clipHeight = graphics.Viewport.Height - 200;

            graphics.ScissorRectangle = new Rectangle(clipX, clipY, clipWidth, clipHeight);

            float startY = 120 - scrollOffset;

            for (int i = 0; i < leaderboard.Entries.Count; i++)
            {
                var entry = leaderboard.Entries[i];
                float x = entry.Time;
                TimeSpan entryTime = TimeSpan.FromSeconds((double)(new decimal(x)));
                string line = $"{i + 1}. {entry.PlayerName} - {entryTime.Hours:00} : {entryTime.Minutes:00} : {entryTime.Seconds:00} . {entryTime.Milliseconds / 10:00}";
                spriteBatch.DrawString(spriteFont, line, new Vector2(200, startY + i * IndividualEntryHeight), Color.White);
            }
            if (leaderboard.Entries.Count < 10)
            {
                for(int i = leaderboard.Entries.Count; i < 10; i++)
                {
                    string line = $"{i + 1}. ______________________________";
                    spriteBatch.DrawString(spriteFont, line, new Vector2(200, startY + i * IndividualEntryHeight), Color.White);
                }
            }

            spriteBatch.End();
        }
    }
}
