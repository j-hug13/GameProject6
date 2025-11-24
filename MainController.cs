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
        //Leaderboards,
        //Settings,
        Quit
    }

    public enum CubeType
    {
        Cube2x2,
        Cube3x3, 
        Cube4x4
    }

    public class MainController : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private GameState currentState = GameState.Menu;
        private GameState? state = null;

        private MenuScreen menuScreen;
        private GameScene gameScene;

        public static int ScreenWidth = 1280;
        public static int ScreenHeight = 720;

        public Dictionary<CubeType, Leaderboard> Leaderboards { get; private set; } = new Dictionary<CubeType, Leaderboard>();

        public MainController()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            menuScreen = new MenuScreen();
            menuScreen.LoadContent(Content, this);
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
                        gameScene = new GameScene(this, spriteBatch, CubeType.Cube3x3);
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
    }
}
