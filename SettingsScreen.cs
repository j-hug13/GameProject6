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
    public enum MoveSpeed
    {
        VerySlow,
        Slow,
        Normal,
        Fast,
        VeryFast
    }

    public class SettingsScreen
    {
        private SettingsManager settings;

        private MouseState currentMouse;
        private MouseState previousMouse;

        private Rectangle backButtonBounds;
        private Rectangle fullscreenButtonBounds;
        private Rectangle windowedButtonBounds;
        private Rectangle leftButtonBounds;
        private Rectangle rightButtonBounds;
        private Rectangle resolutionOneButtonBounds;
        private Rectangle resolutionTwoButtonBounds;
        private Rectangle resolutionThreeButtonBounds;

        private Texture2D selectedButton;
        private Texture2D normalButton;

        private bool isFullscreen = false;

        private SpriteFont spriteFont;

        private MainController game;

        private List<Point> resolutions = new List<Point>()
        {
            new Point(1280, 720),
            new Point(1600, 900),
            new Point(1920, 1080)
        };
        private int resolutionIndex = 0;

        private const int virtualWidth = 1280;
        private const int virtualHeight = 720;

        private MoveSpeed currentSpeed = MoveSpeed.Normal;
        public MoveSpeed SelectedMoveSpeed => currentSpeed;

        private Point GetScaledMousePos()
        {
            Matrix inv = Matrix.Invert(game.UIScaleMatrix);
            Vector2 v = Vector2.Transform(currentMouse.Position.ToVector2(), inv);
            Point pos = new Point((int)v.X, (int)v.Y);
            return pos;
        }

        private void GetCurrentSpeed(int direction)
        {
            int t = (int)currentSpeed + direction;
            if (t < 0)
            {
                t = 4;
            }
            if (t > 4)
            {
                t = 0;
            }

            currentSpeed = (MoveSpeed)t;

            settings.MoveSpeed = currentSpeed;
            settings.Save();
        }

        public void LoadContent(ContentManager content, MainController game, GraphicsDevice graphics)
        {
            // ===== Load settings from file ===== //
            settings = SettingsManager.Load();

            currentSpeed = settings.MoveSpeed;
            isFullscreen = settings.IsFullscreen;
            resolutionIndex = settings.ResolutionIndex;
            // ================================== //

            Vector2 center = new Vector2(virtualWidth / 2, virtualHeight / 2);
            this.game = game;
            spriteFont = content.Load<SpriteFont>("bangers");

            selectedButton = content.Load<Texture2D>("SettingsScreen/selectedButton");
            normalButton = content.Load<Texture2D>("SettingsScreen/button");

            Vector2 backSize = spriteFont.MeasureString("Back");
            backButtonBounds = new Rectangle(20, 20, (int)backSize.X, (int)backSize.Y);

            Vector2 leftButtonSize = spriteFont.MeasureString("<");
            leftButtonBounds = new Rectangle(630, 175, (int)leftButtonSize.X, (int)leftButtonSize.Y);
            Vector2 rightButtonSize = spriteFont.MeasureString(">");
            rightButtonBounds = new Rectangle(960, 175, (int)rightButtonSize.X, (int)rightButtonSize.Y);

            Vector2 fullscreenButtonSize = spriteFont.MeasureString("Fullscreen");
            fullscreenButtonBounds = new Rectangle(335, 330, (int)fullscreenButtonSize.X, (int)fullscreenButtonSize.Y);
            Vector2 windowedButtonSize = spriteFont.MeasureString("Windowed");
            windowedButtonBounds = new Rectangle(745, 330, (int)windowedButtonSize.X, (int)windowedButtonSize.Y);

            Vector2 resolutionOneButtonSize = spriteFont.MeasureString("1280 x 720");
            resolutionOneButtonBounds = new Rectangle(505, 485, (int)resolutionOneButtonSize.X, (int)resolutionOneButtonSize.Y);
            Vector2 resolutionTwoButtonSize = spriteFont.MeasureString("1600 x 900");
            resolutionTwoButtonBounds = new Rectangle(880, 485, (int)resolutionTwoButtonSize.X, (int)resolutionTwoButtonSize.Y);
            Vector2 resolutionThreeButtonSize = spriteFont.MeasureString("1920 x 1080");
            resolutionThreeButtonBounds = new Rectangle(680, 570, (int)resolutionThreeButtonSize.X, (int)resolutionThreeButtonSize.Y);
        }

        public GameState? Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (backButtonBounds.Contains(GetScaledMousePos()))
                {
                    settings.Save();
                    return GameState.Menu;
                }

                if (leftButtonBounds.Contains(GetScaledMousePos()))
                {
                    GetCurrentSpeed(-1);
                }
                if (rightButtonBounds.Contains(GetScaledMousePos()))
                {
                    GetCurrentSpeed(1);
                }

                if (fullscreenButtonBounds.Contains(GetScaledMousePos()) && isFullscreen == false)
                {
                    isFullscreen = true;

                    settings.IsFullscreen = true;
                    settings.Save();

                    game.SetScreenSize(true);
                }
                if (windowedButtonBounds.Contains(GetScaledMousePos()) && isFullscreen == true)
                {
                    MainController.ScreenWidth = resolutions[resolutionIndex].X;
                    MainController.ScreenHeight = resolutions[resolutionIndex].Y;
                    isFullscreen = false;

                    settings.IsFullscreen = false;
                    settings.Save();

                    game.SetScreenSize(false);
                }

                if (resolutionOneButtonBounds.Contains(GetScaledMousePos()) && isFullscreen == false && resolutionIndex != 0)
                {
                    resolutionIndex = 0;
                    Point res = resolutions[0];
                    MainController.ScreenWidth = res.X;
                    MainController.ScreenHeight = res.Y;

                    settings.ResolutionIndex = resolutionIndex;
                    settings.Save();

                    game.SetScreenSize(false);
                }
                if (resolutionTwoButtonBounds.Contains(GetScaledMousePos()) && isFullscreen == false && resolutionIndex != 1)
                {
                    resolutionIndex = 1;
                    Point res = resolutions[1];
                    MainController.ScreenWidth = res.X;
                    MainController.ScreenHeight = res.Y;

                    settings.ResolutionIndex = resolutionIndex;
                    settings.Save();

                    game.SetScreenSize(false);
                }
                if (resolutionThreeButtonBounds.Contains(GetScaledMousePos()) && isFullscreen == false && resolutionIndex != 2)
                {
                    resolutionIndex = 2;
                    Point res = resolutions[2];
                    MainController.ScreenWidth = res.X;
                    MainController.ScreenHeight = res.Y;

                    settings.ResolutionIndex = resolutionIndex;
                    settings.Save();

                    game.SetScreenSize(false);
                }
            }

            return null;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            graphics.Clear(new Color(176, 124, 196));

            spriteBatch.Begin(transformMatrix: game.UIScaleMatrix);

            Vector2 titleSize = spriteFont.MeasureString("Settings");
            spriteBatch.DrawString(spriteFont, "Settings", new Vector2((virtualWidth - titleSize.X) / 2, 20), Color.Gold);

            Color backButtonColor;
            if (backButtonBounds.Contains(GetScaledMousePos()) == true)
            {
                backButtonColor = Color.Gold;
            }
            else
            {
                backButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Back", new Vector2(20, 20), backButtonColor);

            // MOVE SPEED SETTING
            spriteBatch.DrawString(spriteFont, "Move Speed:", new Vector2(295, 175), Color.White);
            Color leftButtonColor;
            if (leftButtonBounds.Contains(GetScaledMousePos()) == true)
            {
                leftButtonColor = Color.Gold;
            }
            else
            {
                leftButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "<", new Vector2(630, 175), leftButtonColor);
            Color rightButtonColor;
            if (rightButtonBounds.Contains(GetScaledMousePos()) == true)
            {
                rightButtonColor = Color.Gold;
            }
            else
            {
                rightButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, ">", new Vector2(960, 175), rightButtonColor);
            string moveSpeedText = "Normal";
            if (currentSpeed == MoveSpeed.Fast) moveSpeedText = "Fast";
            else if (currentSpeed == MoveSpeed.VeryFast) moveSpeedText = "Very Fast";
            else if (currentSpeed == MoveSpeed.Slow) moveSpeedText = "Slow";
            else if (currentSpeed == MoveSpeed.VerySlow) moveSpeedText = "Very Slow";
            Vector2 moveSpeedTextSize = spriteFont.MeasureString(moveSpeedText);
            spriteBatch.DrawString(spriteFont, moveSpeedText, new Vector2(810 - moveSpeedTextSize.X / 2, 175), Color.White);

            // FULLSCREEN OR WINDOWED SETTING
            if (isFullscreen == true)
            {
                spriteBatch.Draw(selectedButton, new Vector2(270, 340), Color.White);
                spriteBatch.Draw(normalButton, new Vector2(680, 340), Color.White);
            }
            else
            {
                spriteBatch.Draw(normalButton, new Vector2(270, 340), Color.White);
                spriteBatch.Draw(selectedButton, new Vector2(680, 340), Color.White);
            }

            Color fullscreenButtonColor;
            if (fullscreenButtonBounds.Contains(GetScaledMousePos()) == true)
            {
                fullscreenButtonColor = Color.Gold;
            }
            else
            {
                fullscreenButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Fullscreen", new Vector2(335, 330), fullscreenButtonColor);

            Color windowedButtonColor;
            if (windowedButtonBounds.Contains(GetScaledMousePos()) == true)
            {
                windowedButtonColor = Color.Gold;
            }
            else
            {
                windowedButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Windowed", new Vector2(745, 330), windowedButtonColor);

            // RESOLUTION SELECTION SETTING
            if (isFullscreen == false)
            {
                if (resolutionIndex == 1)
                {
                    spriteBatch.Draw(normalButton, new Vector2(440, 495), Color.White);
                    spriteBatch.Draw(selectedButton, new Vector2(815, 495), Color.White);
                    spriteBatch.Draw(normalButton, new Vector2(615, 580), Color.White);
                }
                else if (resolutionIndex == 2)
                {
                    spriteBatch.Draw(normalButton, new Vector2(440, 495), Color.White);
                    spriteBatch.Draw(normalButton, new Vector2(815, 495), Color.White);
                    spriteBatch.Draw(selectedButton, new Vector2(615, 580), Color.White);
                }
                else
                {
                    spriteBatch.Draw(selectedButton, new Vector2(440, 495), Color.White);
                    spriteBatch.Draw(normalButton, new Vector2(815, 495), Color.White);
                    spriteBatch.Draw(normalButton, new Vector2(615, 580), Color.White);
                }

                spriteBatch.DrawString(spriteFont, "Resolution:", new Vector2(100, 525), Color.White);
                Color resolutionOneButtonColor;
                if (resolutionOneButtonBounds.Contains(GetScaledMousePos()) == true)
                {
                    resolutionOneButtonColor = Color.Gold;
                }
                else
                {
                    resolutionOneButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "1280 x 720", new Vector2(505, 485), resolutionOneButtonColor);
                Color resolutionTwoButtonColor;
                if (resolutionTwoButtonBounds.Contains(GetScaledMousePos()) == true)
                {
                    resolutionTwoButtonColor = Color.Gold;
                }
                else
                {
                    resolutionTwoButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "1600 x 900", new Vector2(880, 485), resolutionTwoButtonColor);
                Color resolutionThreeButtonColor;
                if (resolutionThreeButtonBounds.Contains(GetScaledMousePos()) == true)
                {
                    resolutionThreeButtonColor = Color.Gold;
                }
                else
                {
                    resolutionThreeButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "1920 x 1080", new Vector2(680, 570), resolutionThreeButtonColor);
            }

            spriteBatch.End();
        }
    }
}
