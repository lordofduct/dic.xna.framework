﻿#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

using Dic.Xna.Framework;
#endregion

namespace GameEntityTutorial
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ExampleGame : Game
    {

        #region Fields

        GraphicsDeviceManager graphics;

        private EntityManagerComponent _entityManager;
        private RenderManagerComponent _renderManager;

        #endregion

        #region CONSTRUCTOR

        public ExampleGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.Components.Add(_entityManager = new EntityManagerComponent(this));
            this.Components.Add(_renderManager = new RenderManagerComponent(this, _entityManager));
        }

        #endregion

        #region Properties

        public EntityManagerComponent EntityManager { get { return _entityManager; } }

        public RenderManagerComponent RenderManager { get { return _renderManager; } }

        #endregion

        #region Game Events

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);


        }

        #endregion

    }
}
