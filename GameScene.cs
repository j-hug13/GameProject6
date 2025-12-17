using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

namespace GameProject6
{
    public class GameScene
    {
        private MainController game;
        private GraphicsDevice controllerGraphics;
        private SettingsScreen settings;

        private dynamic rubiksCube;
        private OrbitCamera camera;
        private KeyboardState currentKeyboard;
        private KeyboardState previousKeyboard;
        private MouseState currentMouse;
        private MouseState previousMouse;
        private CubeType cubeType;

        private bool shouldStartScramble = false;
        private TimeSpan scrambleDelayTimer = TimeSpan.Zero;
        private static readonly TimeSpan InitialScrambleDelay = TimeSpan.FromSeconds(0.5);

        // === Fields for UI / In-Game Pause Menu === //
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private TimeSpan solvingTime = TimeSpan.Zero;
        private bool isPaused = false;
        private PauseMenu pauseMenu;
        private bool hasPlayerStarted = false;

        private Texture2D pauseButton;
        private Rectangle pauseButtonBounds;

        private WinScreen winScreen;
        private bool onWinScreen = false;
        // ========================================== //

        private bool isSolved = false;
        private TimeSpan timeToSolve = TimeSpan.Zero;

        public float RotationSpeed = MathHelper.Pi * 2.0f;

        private Point GetScaledMousePos()
        {
            Matrix inv = Matrix.Invert(game.UIScaleMatrix);
            Vector2 v = Vector2.Transform(currentMouse.Position.ToVector2(), inv);
            Point pos = new Point((int)v.X, (int)v.Y);
            return pos;
        }

        private Ray CalculateRay(Vector2 mousePosition, Matrix view, Matrix projection)
        {
            Viewport viewport = controllerGraphics.Viewport;

            Vector3 nearPoint = viewport.Unproject(new Vector3(mousePosition, 0), projection, view, Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(new Vector3(mousePosition, 1), projection, view, Matrix.Identity);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            Ray r = new Ray(nearPoint, direction);

            return r;
        }

        public GameScene(MainController g, SpriteBatch sb, CubeType type, SettingsScreen settingsScreen)
        {
            game = g;
            controllerGraphics = game.GraphicsDevice;
            spriteBatch = sb;

            settings = settingsScreen;

            cubeType = type;
        }

        public void LoadContent(ContentManager content)
        {
            spriteFont = content.Load<SpriteFont>("bangers");

            RotationSpeed = MathHelper.Pi * 2.5f;
            if (settings.SelectedMoveSpeed == MoveSpeed.Fast) RotationSpeed = MathHelper.Pi * 5.0f;
            else if (settings.SelectedMoveSpeed == MoveSpeed.VeryFast) RotationSpeed = MathHelper.Pi * 10.0f;
            else if (settings.SelectedMoveSpeed == MoveSpeed.Slow) RotationSpeed = MathHelper.Pi * 1.5f;
            else if (settings.SelectedMoveSpeed == MoveSpeed.VerySlow) RotationSpeed = MathHelper.Pi * 0.5f;

            if (cubeType == CubeType.Cube2x2) 
            {
                rubiksCube = new RubiksCube2x2(controllerGraphics, RotationSpeed);
            }
            else
            {
                rubiksCube = new RubiksCube3x3(controllerGraphics, RotationSpeed);
            }
            camera = new OrbitCamera(controllerGraphics, 10f);

            rubiksCube.Effect.View = camera.View;
            rubiksCube.Effect.Projection = camera.Projection;

            pauseMenu = new PauseMenu(controllerGraphics, content, game);
            pauseButton = content.Load<Texture2D>("GameScene/pauseButton");
            pauseButtonBounds = new Rectangle(1185, 20, pauseButton.Width, pauseButton.Height);

            winScreen = new WinScreen(controllerGraphics, content, game);

            solvingTime = TimeSpan.Zero;
            hasPlayerStarted = false;
            shouldStartScramble = false;
            scrambleDelayTimer = TimeSpan.Zero;

            if (game.ShouldLoadCube == true && SaveCubeManager.HasSave(cubeType) == true)
            {
                LoadSavedGame();
                game.ShouldLoadCube = false;
            }
            else
            {
                shouldStartScramble = true;
            }
        }

        public GameState? Update(GameTime gameTime)
        {
            currentKeyboard = Keyboard.GetState();
            currentMouse = Mouse.GetState();

            if (onWinScreen == false)
            {
                if (currentKeyboard.IsKeyDown(Keys.Space) == true && previousKeyboard.IsKeyUp(Keys.Space) == true)
                {
                    isPaused = !isPaused;
                }
                if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released && pauseButtonBounds.Contains(GetScaledMousePos()))
                {
                    isPaused = !isPaused;
                }
            }

            if (isPaused == false && shouldStartScramble == true)
            {
                scrambleDelayTimer += gameTime.ElapsedGameTime;
                if (scrambleDelayTimer >= InitialScrambleDelay)
                {
                    rubiksCube.Scramble();
                    shouldStartScramble = false;
                }
            }

            if (isPaused == true)
            {
                if (pauseMenu.CheckReturnBounds(currentMouse, previousMouse) == true)
                {
                    SaveGame();
                    return GameState.Menu;
                }
            }
            if (isSolved == true)
            {
                winScreen.Update(gameTime, currentKeyboard, previousKeyboard);
                if (winScreen.CheckButtonBounds(currentMouse, previousMouse) == true)
                {
                    return GameState.Menu;
                }
            }

            else
            {
                if (isPaused == false && hasPlayerStarted == true)
                {
                    solvingTime += gameTime.ElapsedGameTime;

                    if (rubiksCube.IsSolved() == true)
                    {
                        isSolved = true;
                        timeToSolve = solvingTime;

                        SaveCubeManager.Clear(cubeType);

                        onWinScreen = true;
                        winScreen.StartNameEntry(cubeType, timeToSolve);
                    }
                }

                if (rubiksCube.IsRotating == false)
                {
                    camera.Update();

                    if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                    {
                        Ray selection = CalculateRay(currentMouse.Position.ToVector2(), camera.View, camera.Projection);
                        var hit = rubiksCube.Intersect(selection);

                        if (hit.Hit == true)
                        {
                            rubiksCube.SelectLayer(hit.GridPos, hit.HitNormal);
                        }
                        else
                        {
                            rubiksCube.IsLayerSelected = false;
                        }
                    }

                    if (rubiksCube.IsLayerSelected == true)
                    {
                        int direction = 0;

                        if ((currentKeyboard.IsKeyDown(Keys.Right) && previousKeyboard.IsKeyUp(Keys.Right)) || (currentKeyboard.IsKeyDown(Keys.D) && previousKeyboard.IsKeyUp(Keys.D)))
                        {
                            direction = 1;
                        }
                        else if ((currentKeyboard.IsKeyDown(Keys.Left) && previousKeyboard.IsKeyUp(Keys.Left)) || (currentKeyboard.IsKeyDown(Keys.A) && previousKeyboard.IsKeyUp(Keys.A)))
                        {
                            direction = -1;
                        }

                        if (direction != 0)
                        {
                            hasPlayerStarted = true;

                            int adjustedDirection = rubiksCube.GetAdjustedDirection(direction, camera.Position);

                            rubiksCube.StartRotation(rubiksCube.SelectedAxis, rubiksCube.SelectedLayer, adjustedDirection);

                            rubiksCube.rotationSpeed = RotationSpeed;
                        }
                    }
                }
                rubiksCube.Update(gameTime);
            }
            
            previousKeyboard = currentKeyboard;
            previousMouse = currentMouse;

            return null;
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphics)
        {
            graphics.Clear(new Color(176, 124, 196));

            // Switch from the 2D menu graphics to the 3D game graphics
            graphics.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);

            graphics.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            // Not really sure what this means but google says Z-axis buffering 
            graphics.DepthStencilState = DepthStencilState.Default;

            rubiksCube.Effect.View = camera.View;
            rubiksCube.Effect.Projection = camera.Projection;

            rubiksCube.Draw(rubiksCube.Effect, rubiksCube.World);

            spriteBatch.Begin(transformMatrix: game.UIScaleMatrix);

            TimeSpan displayTime = solvingTime;
            if (isSolved == true)
            {
                displayTime = timeToSolve;
            }
            string timeToString = $"Time - {displayTime.Hours:00} : {displayTime.Minutes:00} : {displayTime.Seconds:00} . {displayTime.Milliseconds / 10:00}";
            spriteBatch.DrawString(spriteFont, timeToString, new Vector2(20, 20), Color.Gold);
            
            if(isSolved == true)
            {
                winScreen.Draw(spriteBatch, timeToString);
            }
            
            if (isPaused == true)
            {
                pauseMenu.Draw(spriteBatch);
            }
            spriteBatch.Draw(pauseButton, new Vector2(1185, 20), Color.White);

            spriteBatch.End();
        }

        #region Save and Load Cube
        private void SaveGame()
        {
            if (hasPlayerStarted == false || rubiksCube.IsRotating == true)
            {
                return;
            }

            SaveCubeState state = rubiksCube.GetState(solvingTime.TotalSeconds);

            SaveCubeManager.Save(cubeType, state);
        }

        private void LoadSavedGame()
        {
            var state = SaveCubeManager.Load(cubeType);
            if (state == null)
            {
                return;
            }

            rubiksCube.LoadState(state);
            solvingTime = TimeSpan.FromSeconds(state.SolvingTime);
            hasPlayerStarted = true;
            shouldStartScramble = false;
            scrambleDelayTimer = TimeSpan.Zero;
        }
        #endregion
    }
}
