using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameProject6
{
    public enum GameState
    {
        Menu,
        Playing,
        Leaderboards,
        Settings,
        Quit
    }

    public enum CubeType
    {
        Cube2x2,
        Cube3x3 
        //Cube4x4
    }

    public class MainController : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private GameState currentState = GameState.Menu;
        private GameState? state = null;

        private MenuScreen menuScreen;
        private GameScene gameScene;
        private LeaderboardScreen leaderboardScreen;
        private SettingsScreen settingsScreen;

        public Matrix UIScaleMatrix { get; private set; } = Matrix.Identity;
        public static int ScreenWidth = 1280;
        public static int ScreenHeight = 720;

        public CubeType SelectedCube;
        public bool ShouldLoadCube { get; set; } = false;

        public Dictionary<CubeType, Leaderboard> Leaderboards { get; private set; } = new Dictionary<CubeType, Leaderboard>();

        public MainController()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            SettingsManager settings = SettingsManager.Load();
            List<Point> resolutions = new List<Point>()
            {
                new Point(1280, 720),
                new Point(1600, 900),
                new Point(1920, 1080)
            };
            Point res = resolutions[settings.ResolutionIndex];

            ScreenWidth = res.X;
            ScreenHeight = res.Y;

            SetScreenSize(settings.IsFullscreen);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            menuScreen = new MenuScreen();
            menuScreen.LoadContent(Content, this);

            leaderboardScreen = new LeaderboardScreen();
            leaderboardScreen.LoadContent(Content, this, GraphicsDevice);

            settingsScreen = new SettingsScreen();
            settingsScreen.LoadContent(Content, this, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            switch (currentState)
            {
                case GameState.Menu:
                    state = menuScreen.Update(gameTime);
                    if (state != null)
                    {
                        currentState = state.Value;
                    }
                    break;

                case GameState.Playing:
                    if (gameScene == null)
                    {
                        gameScene = new GameScene(this, spriteBatch, SelectedCube, settingsScreen);
                        gameScene.LoadContent(Content);
                    }
                    state = gameScene.Update(gameTime);
                    if (state != null)
                    {
                        currentState = state.Value;
                        if (currentState != GameState.Playing)
                        {
                            gameScene = null;
                        }
                    }
                    break;

                case GameState.Leaderboards:
                    state = leaderboardScreen.Update(gameTime);
                    if (state != null)
                    {
                        currentState = state.Value;
                    }
                    break;

                case GameState.Settings:
                    state = settingsScreen.Update(gameTime, graphics);
                    if (state != null)
                    {
                        currentState = state.Value;
                    }
                    break;

                case GameState.Quit:
                    Exit();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(176, 124, 196));

            switch (currentState)
            {
                case GameState.Menu:
                    menuScreen.Draw(gameTime, spriteBatch, GraphicsDevice);
                    break;

                case GameState.Playing:
                    if (gameScene != null)
                    {
                        gameScene.Draw(gameTime, GraphicsDevice);
                    }
                    break;

                case GameState.Leaderboards:
                    leaderboardScreen.Draw(gameTime, spriteBatch, GraphicsDevice);
                    break;

                case GameState.Settings:
                    settingsScreen.Draw(gameTime, spriteBatch, GraphicsDevice);
                    break;
            }

            base.Draw(gameTime);
        }

        public Leaderboard GetLeaderboard(CubeType type)
        {
            if (Leaderboards.ContainsKey(type) == false)
            {
                Leaderboards[type] = new Leaderboard(type);
            }
            return Leaderboards[type];
        }

        public void SetScreenSize(bool fullscreen)
        {
            if (fullscreen == true)
            {
                graphics.IsFullScreen = true;
                var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                ScreenWidth = display.Width;
                ScreenHeight = display.Height;

                graphics.IsFullScreen = fullscreen;
                graphics.PreferredBackBufferWidth = display.Width;
                graphics.PreferredBackBufferHeight = display.Height;
            }
            else
            {
                graphics.IsFullScreen = false;
                graphics.PreferredBackBufferWidth = ScreenWidth;
                graphics.PreferredBackBufferHeight = ScreenHeight;
            }

            graphics.ApplyChanges();
            UpdateScaleMatrix();
        }

        private void UpdateScaleMatrix()
        {
            float scaleX = (float)graphics.PreferredBackBufferWidth / 1280f;
            float scaleY = (float)graphics.PreferredBackBufferHeight / 720f;

            float scale = Math.Min(scaleX, scaleY);

            UIScaleMatrix = Matrix.CreateScale(scale);
        }
    }
}
