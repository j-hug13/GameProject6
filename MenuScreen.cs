using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameProject6.RubiksCube;
using static System.TimeZoneInfo;

namespace GameProject6
{
    public class MenuScreen
    {
        private MouseState currentMouse;
        private MouseState previousMouse;

        private SpriteFont spriteFont;

        private Texture2D backgroundTexture;
        private Texture2D menuButton;

        private Rectangle startButtonBounds;
        private Rectangle quitButtonBounds;
        private Rectangle leaderboardsButtonBounds;
        private Rectangle settingsButtonBounds;

        // === Fields For Start Transition === //
        private bool isTransitioning = false;
        private float transitionTimer = 0f;
        private const float MaxTime = 1.5f;
        private OrbitCamera camera;
        private Vector3 initialCameraPosition;
        private Vector3 initialCameraTarget;
        private Vector3 finalCameraPosition;
        private Vector3 finalCameraTarget;
        // =================================== //

        public void LoadContent(ContentManager content, MainController game)
        {
            spriteFont = content.Load<SpriteFont>("bangers");

            backgroundTexture = content.Load<Texture2D>("MenuScreen/menuScreen");
            menuButton = content.Load<Texture2D>("MenuScreen/menuButton");

            Vector2 startSize = spriteFont.MeasureString("Start");
            Vector2 quitSize = spriteFont.MeasureString("Quit");
            Vector2 leaderboardsSize = spriteFont.MeasureString("Leaderboards");
            Vector2 settingsSize = spriteFont.MeasureString("Settings");
            startButtonBounds = new Rectangle(565, 175, (int)startSize.X, (int)startSize.Y);
            leaderboardsButtonBounds = new Rectangle(505, 250, (int)leaderboardsSize.X, (int)leaderboardsSize.Y);
            settingsButtonBounds = new Rectangle(450, 325, (int)settingsSize.X, (int)settingsSize.Y);
            quitButtonBounds = new Rectangle(775, 325, (int)quitSize.X, (int)quitSize.Y);

            camera = new OrbitCamera(game.GraphicsDevice, 10f);

            initialCameraPosition = camera.Position;
            initialCameraTarget = camera.Target;

            finalCameraPosition = new Vector3(0f, 0f, 5f);
            finalCameraTarget = new Vector3(0f, 0f, 0f);
        }

        public GameState? Update(GameTime gameTime)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();

            if (isTransitioning == true)
            {
                transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float progress = MathHelper.Clamp(transitionTimer / MaxTime, 0f, 1f);

                float smooth = MathHelper.SmoothStep(0f, 1f, progress);

                camera.Position = Vector3.Lerp(initialCameraPosition, finalCameraPosition, smooth);
                camera.Target = Vector3.Lerp(initialCameraTarget, finalCameraTarget, smooth);
                camera.UpdateViewMatrix();

                if (transitionTimer >= MaxTime)
                {
                    isTransitioning = false;
                    return GameState.Playing;
                }

                return null;
            }

            if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (quitButtonBounds.Contains(currentMouse.Position))
                {
                    return GameState.Quit;
                }

                if (startButtonBounds.Contains(currentMouse.Position))
                {
                    isTransitioning = true;
                    transitionTimer = 0f;
                    initialCameraPosition = camera.Position;
                    initialCameraTarget = camera.Target;
                    return null;
                }
            }

            return null;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            graphics.Clear(Color.White);

            float zoomScale = 1f;
            if (isTransitioning == true)
            {
                float progress = MathHelper.Clamp(transitionTimer / MaxTime, 0f, 1f);
                zoomScale = MathHelper.SmoothStep(1f, 2.5f, progress);
            }

            Vector2 zoomCenter = new Vector2(640f, 136f);

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                Matrix.CreateTranslation(-zoomCenter.X, -zoomCenter.Y, 0) *
                Matrix.CreateScale(zoomScale) *
                Matrix.CreateTranslation(zoomCenter.X, zoomCenter.Y, 0)
            );

            spriteBatch.Draw(backgroundTexture, graphics.Viewport.Bounds, Color.White);
            spriteBatch.Draw(menuButton, new Vector2(500, 185), Color.White);
            spriteBatch.Draw(menuButton, new Vector2(440, 260), Color.White);
            spriteBatch.Draw(menuButton, new Vector2(385, 335), Color.White);
            spriteBatch.Draw(menuButton, new Vector2(710, 335), Color.White);

            Color startButtonColor;
            if (startButtonBounds.Contains(currentMouse.Position) == true)
            {
                startButtonColor = Color.Gold;
            }
            else
            {
                startButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Start", new Vector2(565, 175), startButtonColor);

            Color leaderboardsButtonColor;
            if (leaderboardsButtonBounds.Contains(currentMouse.Position) == true)
            {
                leaderboardsButtonColor = Color.Gold;
            }
            else
            {
                leaderboardsButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Leaderboards", new Vector2(505, 250), leaderboardsButtonColor);

            Color settingsButtonColor;
            if (settingsButtonBounds.Contains(currentMouse.Position) == true)
            {
                settingsButtonColor = Color.Gold;
            }
            else
            {
                settingsButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Settings", new Vector2(450, 325), settingsButtonColor);

            Color quitButtonColor;
            if (quitButtonBounds.Contains(currentMouse.Position) == true)
            {
                quitButtonColor = Color.Gold;
            }
            else
            {
                quitButtonColor = Color.LightGreen;
            }
            spriteBatch.DrawString(spriteFont, "Quit", new Vector2(775, 325), quitButtonColor);

            spriteBatch.End();
        }
    }
}
