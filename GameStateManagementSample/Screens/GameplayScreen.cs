#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;
        Texture2D spaceShip;
        Texture2D backgroundTexture;
        Texture2D starTexture;

        Vector2 playerPosition = new Vector2(640, 360);
        Vector2 starPosition = new Vector2(1000, 360);

        Rectangle playerBoundingBox;
        Rectangle starBoundingBox;

        Random random = new Random();

        int score = 0;

        float pauseAlpha;
        float playerSpeed;
        float playerRot;
        float playerRotDeg;
        
        Stopwatch s = new Stopwatch();
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("background (2)");

            gameFont = content.Load<SpriteFont>("gamefont");
            spaceShip = content.Load<Texture2D>("2");
            starTexture = content.Load<Texture2D>("p_Sprite_0");

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
            {
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
                GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            }
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                playerBoundingBox = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)(spaceShip.Width * 0.2), (int)(spaceShip.Height * 0.2));
                starBoundingBox = new Rectangle((int)starPosition.X, (int)starPosition.Y, (int)(starTexture.Width * 0.2), (int)(starTexture.Height * 0.2));
                if (playerBoundingBox.Intersects(starBoundingBox))
                {
                    starPosition = new Vector2(random.Next(100, 1100), random.Next(100, 600));
                    s.Start();
                    GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                    score++;
                }
                if (s.Elapsed > TimeSpan.FromSeconds(0.5))
                {
                    GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
                    s.Stop();
                    s.Reset();
                }

                if (score == 20)
                {
                    ScreenManager.AddScreen(new WinGameMenuScreen(), ControllingPlayer);
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

#if WINDOWS
                if (keyboardState.IsKeyDown(Keys.Left))
                    playerRotDeg -= 1f;

                if (keyboardState.IsKeyDown(Keys.Right))
                    playerRotDeg += 1f;

                if (keyboardState.IsKeyDown(Keys.Up))
                    playerSpeed += 0.1f;

                if (keyboardState.IsKeyDown(Keys.Down))
                    playerSpeed -= 0.1f;

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                playerRotDeg += thumbstick.X * 2;

                playerSpeed += gamePadState.Triggers.Right / 2;
                playerSpeed -= gamePadState.Triggers.Left / 5;
#endif
                setRotationAngle(playerRotDeg);
                movePlayer(playerSpeed);
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            SpriteBatch backgroundSpriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            backgroundSpriteBatch.Begin();

            backgroundSpriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            backgroundSpriteBatch.End();

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(spaceShip, playerPosition, null, Color.White, playerRot + (float)Math.PI / 2, new Vector2(256, 256),
                            0.2f, SpriteEffects.None, 0);
            spriteBatch.Draw(starTexture, starPosition, null, Color.White, 0, new Vector2(256, 256),
                            0.2f, SpriteEffects.None, 0);

            string text = "Score: " + score;
            
            spriteBatch.DrawString(gameFont, text, new Vector2((float)((ScreenManager.GraphicsDevice.Viewport.Width / 2) - (gameFont.MeasureString(text).X / 2)), (float)(ScreenManager.GraphicsDevice.Viewport.Height * 0.1)), Color.White);
            spriteBatch.End();


            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        public void setRotationAngle(float angle)
        {
            playerRot = MathHelper.ToRadians(angle);
        }

        public void movePlayer(float speed)
        {
            if (speed > 5)
                playerSpeed = 5;
            if (speed < -5)
                playerSpeed = -5;
            Vector2 direction = new Vector2((float)Math.Cos(playerRot), (float)Math.Sin(playerRot));
            direction.Normalize();
            playerPosition += direction * speed;
            int rightSide = 1280 - 45;
            int leftSide = 45, topSide = 45;
            int botSide = 720 - 45;
            if (playerPosition.X > rightSide)
            {
                playerPosition.X = rightSide;
                playerSpeed = 0;
            }
            if (playerPosition.X < leftSide)
            {
                playerPosition.X = leftSide;
                playerSpeed = 0;
            }
            if (playerPosition.Y > botSide)
            {
                playerPosition.Y = botSide;
                playerSpeed = 0;
            }
            if (playerPosition.Y < topSide)
            {
                playerPosition.Y = topSide;
                playerSpeed = 0;
            }
            //Vibration
            if (playerPosition.X >= rightSide)
            {
                GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                s.Start();
            }
            if (playerPosition.X <= leftSide)
            {
                GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                s.Start();
            }
            if (playerPosition.Y >= botSide)
            {
                GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                s.Start();
            }
            if (playerPosition.Y <= topSide)
            {
                GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
                s.Start();
            }
        }

        #endregion
    }
}
