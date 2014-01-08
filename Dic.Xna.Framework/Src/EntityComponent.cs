using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{
    public abstract class EntityComponent : IEntityComponent
    {

        #region Fields

        private Entity _owner;

        #endregion

        #region CONSTRUCTOR

        internal void OnAddedToEntity(Entity owner)
        {
            _owner = owner;
        }

        #endregion

        /// <summary>
        /// Called on main update thread when initializing the component. Components initialize in random order, you can retrieve other components on 
        /// self but don't inter-communicate. Wait for Start before you inter-communicate. Good for initializing fields.
        /// </summary>
        protected virtual void Initialize()
        {

        }

        /// <summary>
        /// Called on main update thread the first time the component is updated. This happens after all components have initialized so inter-communication 
        /// between components is safe.
        /// </summary>
        protected virtual void Start()
        {

        }

        #region IEntityComponent Interface

        public Entity Entity
        {
            get { return _owner; }
        }

        public Game Game
        {
            get { return _owner.Game; }
        }

        public EntityManagerComponent EntityManager
        {
            get { return _owner.EntityManager; }
        }

        public Transform Transform
        {
            get { return _owner.Transform; }
        }

        public Entity.EntityComponentCollection Components
        {
            get { return _owner.Components; }
        }

        void IEntityComponent.Initialize()
        {
            this.Initialize();
        }

        void IEntityComponent.Start()
        {
            this.Start();
        }

        public void SendMessage<T>(string msg, params object[] args) where T : class
        {
            _owner.SendMessage<T>(msg, args);
        }

        #endregion
        
        #region IDisposable Interface

        private bool _bDisposed;

        /// <summary>
        /// Override to dispose component, disposing is true when called by IDisposable interface, false when called by Destructor.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_bDisposed)
            {
                _bDisposed = true;
                if (_owner != null)
                {
                    _owner.EntityManager.DestroyComponent(this); //called after _bDisposed is set true, this way dispose isn't ran twice by DestroyEntity
                    _owner = null;
                }
                
            }
        }

        ~EntityComponent()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
