using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Picross
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PicrossGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        PicrossBoard picrossBoard;

        public PicrossGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            picrossBoard = new PicrossBoard();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            picrossBoard.resetPuzzle();

            this.IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            PicrossBoard.LoadContent(this);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        bool shiftHeld { get; set; }
        bool spaceHeld { get; set; }
        bool aHeld { get; set; }
        bool rHeld { get; set; }
        bool cHeld { get; set; }
        bool mousePressed { get; set; }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            picrossBoard.Update();

            updateKeyboard();
            updateMouse();

            base.Update(gameTime);
        }

        private void updateKeyboard()
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            shiftHeld = (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift));
            spaceHeld = (ks.IsKeyDown(Keys.Space));

            if (ks.IsKeyDown(Keys.A) && !aHeld)
                picrossBoard.StartAutoSolver();

            if (ks.IsKeyDown(Keys.R) && !rHeld)
                picrossBoard.resetPuzzle();

            if (ks.IsKeyDown(Keys.C) && !cHeld)
                picrossBoard.clearBoard();

            rHeld = (ks.IsKeyDown(Keys.R));
            cHeld = (ks.IsKeyDown(Keys.C));
            aHeld = (ks.IsKeyDown(Keys.A));
        }

        private bool isClickModified()
        {
            return shiftHeld || spaceHeld;
        }

        private void updateMouse()
        {
            MouseState ms = Mouse.GetState();

            int mx = ms.X - xOffset;
            int my = ms.Y - yOffset;

            picrossBoard.UpdateMousePosition(mx, my);

            if (ms.LeftButton == ButtonState.Released)
            {
                mousePressed = false;
            }
            else
            {
                if (!mousePressed)
                {
                    picrossBoard.processClick(isClickModified());
                }

                mousePressed = true;
            }
        }

        protected int xOffset { get { return (GraphicsDevice.Viewport.Width - picrossBoard.BoardWidth) / 2; } }
        protected int yOffset { get { return (GraphicsDevice.Viewport.Height - picrossBoard.BoardHeight) / 2; } }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();

            picrossBoard.Draw(spriteBatch, xOffset, yOffset);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
