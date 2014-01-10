using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dic.Xna.Framework.Src
{
    public class SpriteRenderManagerComponent : DrawableGameComponent
    {

        #region Fields

        private EntityManagerComponent _entityManager;

        private List<SpriteRenderer> _renderers = new List<SpriteRenderer>();

        #endregion

        #region CONSTRUCTOR

        public SpriteRenderManagerComponent(Game game, EntityManagerComponent entityManager)
            : base(game)
        {
            _entityManager = entityManager;
        }

        #endregion

        #region Properties

        public EntityManagerComponent EntityManager
        {
            get { return _entityManager; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return base.Game.GraphicsDevice; }
        }

        #endregion

        #region Methods

        internal void RegisterRenderer(SpriteRenderer renderer)
        {
            if (_entityManager != renderer.EntityManager) throw new ArgumentException("Can only register renderer with the RenderManagerComponent associated with the EntityManagerComponent that manages the renderer's entity.", "renderer");

            if (!_renderers.Contains(renderer))
            {
                _renderers.Add(renderer);
            }
        }

        #endregion

        #region Component Events

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


        }

        #endregion

    }
}
