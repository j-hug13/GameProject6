using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GameProject6
{
    public class WinScreen
    {
        private SpriteFont spriteFont;
        private GraphicsDevice graphics;
        private Rectangle returnButtonBounds;

        private int virtualWidth = 1280;
        private int virtualHeight = 720;

        private Color quitColor = Color.Red;

        // ===== Fields for Leaderboard ===== //
        private MainController game;
        private string playerName = "";
        private bool enteringName = false;
        private TimeSpan solveTime;
        private CubeType cubeType;
        private bool nameEntered = false;
        private LeaderboardEntry newestEntry;
        private bool newestEntryShown = false;
        // ================================== //

        private Point GetScaledMousePos(MouseState currentMouse)
        {
            Matrix inv = Matrix.Invert(game.UIScaleMatrix);
            Vector2 v = Vector2.Transform(currentMouse.Position.ToVector2(), inv);
            Point pos = new Point((int)v.X, (int)v.Y);
            return pos;
        }

        public WinScreen(GraphicsDevice graphicsDevice, ContentManager content, MainController controller)
        {
            graphics = graphicsDevice;

            spriteFont = content.Load<SpriteFont>("bangers");

            Vector2 buttonSize = spriteFont.MeasureString("Return To Menu");
            Vector2 buttonPos = new Vector2((virtualWidth / 2) - (buttonSize.X / 2), virtualHeight - 20 - buttonSize.Y);
            returnButtonBounds = new Rectangle((int)buttonPos.X, (int)buttonPos.Y, (int)buttonSize.X, (int)buttonSize.Y);
            
            game = controller;
        }

        public bool CheckButtonBounds(MouseState currentMouse, MouseState previousMouse)
        {
            if (returnButtonBounds.Contains(GetScaledMousePos(currentMouse)) == true)
            {
                quitColor = Color.MediumVioletRed;
            }
            if (returnButtonBounds.Contains(GetScaledMousePos(currentMouse)) == false)
            {
                quitColor = Color.Red;
            }

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                return returnButtonBounds.Contains(GetScaledMousePos(currentMouse));
            }
            return false;
        }

        public void StartNameEntry(CubeType type, TimeSpan finalTime)
        {
            cubeType = type;
            solveTime = finalTime;
            enteringName = true;
            nameEntered = false;
            playerName = "";
        }

        public void Update(GameTime gameTime, KeyboardState current, KeyboardState previous)
        {
            if (enteringName == false || nameEntered == true)
            {
                return;
            }
            foreach (Keys key in current.GetPressedKeys())
            {
                if (previous.IsKeyUp(key) == true)
                {
                    if (key == Keys.Back && playerName.Length > 0)
                    {
                        playerName = playerName.Substring(0, playerName.Length - 1);
                    }
                    else if (key == Keys.Enter && playerName.Length > 0)
                    {
                        var leaderboard = game.GetLeaderboard(cubeType);
                        newestEntry = leaderboard.AddEntry(playerName, (float)solveTime.TotalSeconds);

                        nameEntered = true;
                        enteringName = false;
                    }
                    else
                    {
                        string k = key.ToString();

                        if (k.Length == 1 && playerName.Length < 12)
                        {
                            playerName += k;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, string time)
        {
            Texture2D fadeTexture = new Texture2D(graphics, 1, 1);
            fadeTexture.SetData(new[] { Color.Black });
            spriteBatch.Draw(fadeTexture, graphics.Viewport.Bounds, Color.Black * 0.7f);

            Vector2 center = new Vector2(virtualWidth / 2, virtualHeight / 2);

            if (enteringName == true)
            {
                string prompt = "Congratulations!\n" + "You Solved the Puzzle!\n" + "Total " + time + "\n\nEnter Your Name:";
                Vector2 promptSize = spriteFont.MeasureString(prompt);

                spriteBatch.DrawString(spriteFont, prompt, center - new Vector2(promptSize.X / 2, 250), Color.White);

                spriteBatch.DrawString(spriteFont, playerName + "_", center + new Vector2(100, 35), Color.Gold);

                spriteBatch.DrawString(spriteFont, "Press ENTER to submit", center - new Vector2(promptSize.X / 2, 60 - promptSize.Y / 2), Color.White * 0.7f);

                return;
            }

            var leaderboard = game.GetLeaderboard(cubeType);
            string titleText = "";
            if (cubeType == CubeType.Cube2x2)
            {
                titleText = "2x2 Cube Leaderboard";
            }
            else if (cubeType == CubeType.Cube3x3)
            {
                titleText = "3x3 Cube Leaderboard";
            }
            /*
            else if (cubeType == CubeType.Cube4x4)
            {
                titleText = "4x4 Cube Leaderboard";
            }
            */
            Vector2 titleSize = spriteFont.MeasureString(titleText);
            spriteBatch.DrawString(spriteFont, titleText, new Vector2(center.X - titleSize.X / 2, 20), Color.DarkGray);

            Color entryColor = Color.White;
            string line = "";
            for (int i = 0; i < 6; i++)
            {
                entryColor = Color.White;
                line = $"{i + 1}. _______________________";
                if (leaderboard.Entries.Count > i)
                {
                    var entry = leaderboard.Entries[i];
                    float x = entry.Time;
                    TimeSpan entryTime = TimeSpan.FromSeconds((double)(new decimal(x)));
                    line = $"{i + 1}. {entry.PlayerName} - {entryTime.Hours:00} : {entryTime.Minutes:00} : {entryTime.Seconds:00} . {entryTime.Milliseconds / 10:00}";
                    if (entry == newestEntry)
                    {
                        entryColor = Color.Gold;
                        newestEntryShown = true;
                    }
                }

                spriteBatch.DrawString(spriteFont, line, new Vector2(center.X - 350, titleSize.Y + 30 + (i * 75)), entryColor);
            }
            entryColor = Color.White;
            line = "7. _______________________";
            if (leaderboard.Entries.Count > 6 && newestEntryShown == false)
            {
                float x = newestEntry.Time;
                TimeSpan entryTime = TimeSpan.FromSeconds((double)(new decimal(x)));
                int index = leaderboard.Entries.IndexOf(newestEntry);
                line = $"{index + 1}. {newestEntry.PlayerName} - {entryTime.Hours:00} : {entryTime.Minutes:00} : {entryTime.Seconds:00} . {entryTime.Milliseconds / 10:00}";
                entryColor = Color.Gold;
            }
            spriteBatch.DrawString(spriteFont, line, new Vector2(center.X - 350, titleSize.Y + 30 + (450)), entryColor);

            Vector2 textSize = spriteFont.MeasureString("Return To Menu");
            spriteBatch.DrawString(spriteFont, "Return To Menu", new Vector2(center.X - textSize.X / 2, virtualHeight - 20 - textSize.Y), quitColor);

        }
    }
}
