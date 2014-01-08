using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace Dic.Xna.Framework
{
    public abstract class DicGame : Game
    {

        #region Fields

        private EntityManagerComponent _entityManager;
        private RenderManagerComponent _renderManager;

        #endregion

        #region CONSTRUCTOR

        public DicGame()
            : base()
        {
            this.Components.Add(_entityManager = new EntityManagerComponent(this));
            this.Components.Add(_renderManager = new RenderManagerComponent(this, _entityManager));
        }

        #endregion

        #region Properties

        public EntityManagerComponent EntityManager
        {
            get { return _entityManager; }
        }

        public RenderManagerComponent RenderManager
        {
            get { return _renderManager; }
        }

        #endregion


    }
}
