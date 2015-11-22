#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class ControlsMenuScreen : MenuScreen
    {
        #region Fields

        ContentManager content;
        Texture2D controlsTexture;

        #endregion

        #region Initialization
        

        /// <summary>
        /// Constructor.
        /// </summary>
        public ControlsMenuScreen()
            : base("Controls")
        {
            MenuEntry back = new MenuEntry("Back");
            // Hook up menu event handlers.
            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(back);
        }
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            controlsTexture = content.Load<Texture2D>("Controls");
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }
        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 center = new Vector2(viewport.Width / 2, viewport.Height / 2 + 100);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.Draw(controlsTexture, center, null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha), 0, new Vector2(400, 300), 1, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion
    }
}
