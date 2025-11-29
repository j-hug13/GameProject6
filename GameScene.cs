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

        private RubiksCube3x3 rubiksCube;
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

        private Ray CalculateRay(Vector2 mousePosition, Matrix view, Matrix projection)
        {
            Viewport viewport = controllerGraphics.Viewport;

            Vector3 nearPoint = viewport.Unproject(new Vector3(mousePosition, 0), projection, view, Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(new Vector3(mousePosition, 1), projection, view, Matrix.Identity);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            Ray r = new Ray(nearPoint, direction);

            return r;
        }

        public GameScene(MainController g, SpriteBatch sb, CubeType type)
        {
            game = g;
            controllerGraphics = game.GraphicsDevice;
            spriteBatch = sb;                     

            cubeType = type;
        }

        public void LoadContent(ContentManager content)
        {
            spriteFont = content.Load<SpriteFont>("bangers");

            rubiksCube = new RubiksCube3x3(controllerGraphics);
            camera = new OrbitCamera(controllerGraphics, 10f);

            rubiksCube.Effect.View = camera.View;
            rubiksCube.Effect.Projection = camera.Projection;

            pauseMenu = new PauseMenu(controllerGraphics, content);
            pauseButton = content.Load<Texture2D>("GameScene/pauseButton");
            pauseButtonBounds = new Rectangle(1185, 20, pauseButton.Width, pauseButton.Height);

            winScreen = new WinScreen(controllerGraphics, content, game);

            shouldStartScramble = true;
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
                if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released && pauseButtonBounds.Contains(currentMouse.Position))
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
                if (pauseMenu.CheckQuitBounds(currentMouse, previousMouse) == true)
                {
                    game.Exit();
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
                if (hasPlayerStarted == true)
                {
                    solvingTime += gameTime.ElapsedGameTime;

                    if (rubiksCube.IsSolved() == true)
                    {
                        isSolved = true;
                        timeToSolve = solvingTime;

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
                        RubiksCube3x3.HitInfo hit = rubiksCube.Intersect(selection);

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

                            rubiksCube.rotationSpeed = RubiksCube3x3.NormalRotationSpeed;
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

            spriteBatch.Begin();

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
    }
}
