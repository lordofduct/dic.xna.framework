using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dic.Xna.Framework
{
    public class RenderManagerComponent : DrawableGameComponent
    {

        #region Fields

        private EntityManagerComponent _entityManager;

        private List<Renderer> _renderers = new List<Renderer>();

        private Color _backgroundColor = Color.CornflowerBlue;

        #endregion

        #region CONSTRUCTOR

        public RenderManagerComponent(Game game, EntityManagerComponent entityManager)
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

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        #endregion

        #region Methods

        internal void RegisterRenderer(Renderer renderer)
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

            //must be called before draw to allow proper depth buffer
            this.GraphicsDevice.BlendState = BlendState.Opaque;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //Depending on 3D content, may also want to set:
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


            this.GraphicsDevice.Clear(_backgroundColor);

            foreach (var renderer in _renderers)
            {
                renderer.Draw(gameTime);
            }
        }

        #endregion

    }
}
