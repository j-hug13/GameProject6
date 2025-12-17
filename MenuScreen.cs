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
    public class MenuScreen
    {
        private enum TransitionTarget
        {
            None,
            Start,
            Leaderboards,
            Settings
        }

        private MainController game;

        private MouseState currentMouse;
        private MouseState previousMouse;

        private SpriteFont spriteFont;

        private Texture2D backgroundTexture;
        private Texture2D menuButton;

        private Rectangle startButtonBounds;
        private Rectangle quitButtonBounds;
        private Rectangle leaderboardsButtonBounds;
        private Rectangle settingsButtonBounds;

        private Rectangle twoByTwoButtonBounds;
        private Rectangle threeByThreeButtonBounds;

        private Rectangle newButtonBounds;
        private Rectangle loadButtonBounds;
        private CubeType selectedCubeType;

        private Rectangle backButtonBounds;

        private bool onSelectScreen = false;
        private bool cubeTypeSelected = false;
        private bool saveExists = false;

        // === Fields For Start Transition === //
        private TransitionTarget transitionTarget = TransitionTarget.None;
        private float transitionTimer = 0f;
        private const float MaxTime = 1.5f;
        private OrbitCamera camera;
        private Vector3 initialCameraPosition;
        private Vector3 initialCameraTarget;
        private Vector3 finalCameraPosition;
        private Vector3 finalCameraTarget;
        // =================================== //

        private Point GetScaledMousePos()
        {
            Matrix inv = Matrix.Invert(game.UIScaleMatrix);
            Vector2 v = Vector2.Transform(currentMouse.Position.ToVector2(), inv);
            Point pos = new Point((int)v.X, (int)v.Y);
            return pos;
        }

        public void LoadContent(ContentManager content, MainController game)
        {
            this.game = game;
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
            quitButtonBounds = new Rectangle(770, 325, (int)quitSize.X, (int)quitSize.Y);

            Vector2 twoByTwoSize = spriteFont.MeasureString("2 x 2");
            Vector2 threeByThreeSize = spriteFont.MeasureString("3 x 3");
            twoByTwoButtonBounds = new Rectangle(465, 190, (int)twoByTwoSize.X, (int)twoByTwoSize.Y);
            threeByThreeButtonBounds = new Rectangle(745, 190, (int)threeByThreeSize.X, (int)threeByThreeSize.Y);

            Vector2 newButtonSize = spriteFont.MeasureString("New Cube");
            Vector2 loadButtonSize = spriteFont.MeasureString("Load Cube");
            newButtonBounds = new Rectangle(540, 175, (int)newButtonSize.X, (int)newButtonSize.Y);
            loadButtonBounds = new Rectangle(540, 250, (int)loadButtonSize.X, (int)loadButtonSize.Y);

            Vector2 backSize = spriteFont.MeasureString("Back");
            backButtonBounds = new Rectangle(585, 325, (int)backSize.X, (int)backSize.Y);

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

            if (transitionTarget != TransitionTarget.None)
            {
                transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float progress = MathHelper.Clamp(transitionTimer / MaxTime, 0f, 1f);

                float smooth = MathHelper.SmoothStep(0f, 1f, progress);

                camera.Position = Vector3.Lerp(initialCameraPosition, finalCameraPosition, smooth);
                camera.Target = Vector3.Lerp(initialCameraTarget, finalCameraTarget, smooth);
                camera.UpdateViewMatrix();

                if (transitionTimer >= MaxTime)
                {
                    if (transitionTarget == TransitionTarget.Start)
                    {
                        transitionTarget = TransitionTarget.None;
                        onSelectScreen = false;
                        cubeTypeSelected = false;
                        saveExists = false;
                        return GameState.Playing;
                    }
                    else if (transitionTarget == TransitionTarget.Leaderboards)
                    {
                        transitionTarget = TransitionTarget.None;
                        return GameState.Leaderboards;
                    }
                    else if (transitionTarget == TransitionTarget.Settings)
                    {
                        transitionTarget = TransitionTarget.None;
                        return GameState.Settings;
                    }
                    else
                    {
                        transitionTarget = TransitionTarget.None;
                    }
                }
                return null;
            }

            if (onSelectScreen == false)
            {
                if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                {
                    if (quitButtonBounds.Contains(GetScaledMousePos()))
                    {
                        return GameState.Quit;
                    }

                    if (startButtonBounds.Contains(GetScaledMousePos()))
                    {
                        onSelectScreen = true;
                    }

                    if (leaderboardsButtonBounds.Contains(GetScaledMousePos()))
                    {
                        transitionTarget = TransitionTarget.Leaderboards;
                        transitionTimer = 0f;
                        initialCameraPosition = camera.Position;
                        initialCameraTarget = camera.Target;
                        return null;
                    }

                    if (settingsButtonBounds.Contains(GetScaledMousePos()))
                    {
                        transitionTarget = TransitionTarget.Settings;
                        transitionTimer = 0f;
                        initialCameraPosition = camera.Position;
                        initialCameraTarget = camera.Target;
                        return null;
                    }
                }
            }
            else
            {
                if (cubeTypeSelected == false)
                {
                    if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                    {
                        if (threeByThreeButtonBounds.Contains(GetScaledMousePos()))
                        {
                            game.SelectedCube = CubeType.Cube3x3;
                            selectedCubeType = CubeType.Cube3x3;
                            cubeTypeSelected = true;

                            saveExists = SaveCubeManager.HasSave(selectedCubeType);

                            return null;
                        }

                        if (twoByTwoButtonBounds.Contains(GetScaledMousePos()))
                        {
                            game.SelectedCube = CubeType.Cube2x2;
                            selectedCubeType = CubeType.Cube2x2;
                            cubeTypeSelected = true;

                            saveExists = SaveCubeManager.HasSave(selectedCubeType);

                            return null;
                        }

                        if (backButtonBounds.Contains(GetScaledMousePos()))
                        {
                            onSelectScreen = false;
                            return null;
                        }
                    }
                }

                else
                {
                    if (saveExists == true)
                    {
                        if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                        {
                            if (newButtonBounds.Contains(GetScaledMousePos()))
                            {
                                game.ShouldLoadCube = false;
                                transitionTarget = TransitionTarget.Start;
                                transitionTimer = 0f;
                                initialCameraPosition = camera.Position;
                                initialCameraTarget = camera.Target;
                                return null;
                            }

                            if (loadButtonBounds.Contains(GetScaledMousePos()))
                            {
                                game.ShouldLoadCube = true;
                                transitionTarget = TransitionTarget.Start;
                                transitionTimer = 0f;
                                initialCameraPosition = camera.Position;
                                initialCameraTarget = camera.Target;
                                return null;
                            }

                            if (backButtonBounds.Contains(GetScaledMousePos()))
                            {
                                saveExists = SaveCubeManager.HasSave(selectedCubeType);
                                onSelectScreen = true;
                                cubeTypeSelected = false;
                                return null;
                            }
                        }
                    }

                    else if (transitionTarget == TransitionTarget.None)
                    {
                        game.ShouldLoadCube = false;
                        transitionTarget = TransitionTarget.Start;
                        transitionTimer = 0f;
                        initialCameraPosition = camera.Position;
                        initialCameraTarget = camera.Target;
                        return null;
                    }
                }
            }

            return null;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            graphics.Clear(Color.White);

            float zoomScale = 1f;
            if (transitionTarget != TransitionTarget.None)
            {
                float progress = MathHelper.Clamp(transitionTimer / MaxTime, 0f, 1f);
                zoomScale = MathHelper.SmoothStep(1f, 2.5f, progress);
            }

            Vector2 zoomCenter = Vector2.Transform(new Vector2(640f, 136f), game.UIScaleMatrix);

            Matrix finalMatrix = game.UIScaleMatrix *
                                 Matrix.CreateTranslation(-zoomCenter.X, -zoomCenter.Y, 0) *
                                 Matrix.CreateScale(zoomScale) *
                                 Matrix.CreateTranslation(zoomCenter.X, zoomCenter.Y, 0);
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                finalMatrix
            );

            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            if (onSelectScreen == false)
            {
                spriteBatch.Draw(menuButton, new Vector2(500, 185), Color.White);
                spriteBatch.Draw(menuButton, new Vector2(440, 260), Color.White);
                spriteBatch.Draw(menuButton, new Vector2(385, 335), Color.White);
                spriteBatch.Draw(menuButton, new Vector2(705, 335), Color.White);

                Color startButtonColor;
                if (startButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                {
                    startButtonColor = Color.Gold;
                }
                else
                {
                    startButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "Start", new Vector2(565, 175), startButtonColor);

                Color leaderboardsButtonColor;
                if (leaderboardsButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                {
                    leaderboardsButtonColor = Color.Gold;
                }
                else
                {
                    leaderboardsButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "Leaderboards", new Vector2(505, 250), leaderboardsButtonColor);

                Color settingsButtonColor;
                if (settingsButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                {
                    settingsButtonColor = Color.Gold;
                }
                else
                {
                    settingsButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "Settings", new Vector2(450, 325), settingsButtonColor);

                Color quitButtonColor;
                if (quitButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                {
                    quitButtonColor = Color.Gold;
                }
                else
                {
                    quitButtonColor = Color.LightGreen;
                }
                spriteBatch.DrawString(spriteFont, "Quit", new Vector2(770, 325), quitButtonColor);
            }
            else
            {
                if (cubeTypeSelected == false)
                {
                    spriteBatch.Draw(menuButton, new Vector2(400, 200), Color.White);
                    spriteBatch.Draw(menuButton, new Vector2(680, 200), Color.White);
                    spriteBatch.Draw(menuButton, new Vector2(520, 335), Color.White);

                    Color twoByTwoButtonColor;
                    if (twoByTwoButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        twoByTwoButtonColor = Color.Gold;
                    }
                    else
                    {
                        twoByTwoButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "2 x 2", new Vector2(465, 190), twoByTwoButtonColor);

                    Color threeByThreeButtonColor;
                    if (threeByThreeButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        threeByThreeButtonColor = Color.Gold;
                    }
                    else
                    {
                        threeByThreeButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "3 x 3", new Vector2(745, 190), threeByThreeButtonColor);

                    Color backButtonColor;
                    if (backButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        backButtonColor = Color.Gold;
                    }
                    else
                    {
                        backButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "Back", new Vector2(585, 325), backButtonColor);
                }

                else if (saveExists == true)
                {
                    spriteBatch.Draw(menuButton, new Vector2(475, 185), Color.White);
                    spriteBatch.Draw(menuButton, new Vector2(475, 260), Color.White);
                    spriteBatch.Draw(menuButton, new Vector2(520, 335), Color.White);

                    Color newButtonColor;
                    if (newButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        newButtonColor = Color.Gold;
                    }
                    else
                    {
                        newButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "New Cube", new Vector2(540, 175), newButtonColor);

                    Color loadButtonColor;
                    if (loadButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        loadButtonColor = Color.Gold;
                    }
                    else
                    {
                        loadButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "Load Cube", new Vector2(540, 250), loadButtonColor);

                    Color backButtonColor;
                    if (backButtonBounds.Contains(GetScaledMousePos()) == true && transitionTarget == TransitionTarget.None)
                    {
                        backButtonColor = Color.Gold;
                    }
                    else
                    {
                        backButtonColor = Color.LightGreen;
                    }
                    spriteBatch.DrawString(spriteFont, "Back", new Vector2(585, 325), backButtonColor);
                }
            }
         
            spriteBatch.End();
        }
    }
}
