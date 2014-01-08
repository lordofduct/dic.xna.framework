#region Using Statements
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
    public class ExampleGame : DicGame
    {
        GraphicsDeviceManager graphics;

        public ExampleGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            const int CNT = 1000000;

            //for (int i = 0; i < CNT; i++)
            //{
            //    var ent = this.EntityManager.CreateEntity();
            //}

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

            if (_iIteration == 3)
            {

                const int CNT = 1000000;
                long t;

                var ent1 = this.EntityManager.CreateEntity();
                var ent2 = this.EntityManager.CreateEntity();
                var ent3 = this.EntityManager.CreateEntity();

                ent1.Transform.Children.Add(ent2.Transform);
                ent2.Transform.Children.Add(ent3.Transform);

                ent1.Transform.LocalPosition = new Vector3(3f, 5f, 1.2f);
                ent2.Transform.LocalPosition = new Vector3(1.2f, 11f, 1.7f);

                var rand = new Random();

                t = DateTime.Now.Ticks;

                for (int i = 0; i < CNT; i++)
                {
                    var v = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                    ent3.Transform.LocalPosition = v;
                }

                t = DateTime.Now.Ticks - t;

                Console.WriteLine("LocalPosition: " + ((double)t / (double)TimeSpan.TicksPerSecond).ToString() + " seconds");

                t = DateTime.Now.Ticks;

                for (int i = 0; i < CNT; i++)
                {
                    var v = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                    ent3.Transform.Position = v;
                }

                t = DateTime.Now.Ticks - t;

                Console.WriteLine("WorldPosition: " + ((double)t / (double)TimeSpan.TicksPerSecond).ToString() + " seconds");

            }

            _iIteration++;

            base.Update(gameTime);


        }

        private int _iIteration = 0;

    }
}
