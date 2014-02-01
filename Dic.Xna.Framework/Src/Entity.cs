using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Dic.Xna.Framework.Utils;

namespace Dic.Xna.Framework
{
    public sealed class Entity : IDisposable
    {

        #region Fields

        private string _name;

        private EntityManagerComponent _manager;
        private EntityComponentCollection _components;
        private Transform _transform;

        private Action<GameTime> _updateDelegates;

        #endregion

        #region CONSTRUCTOR

        public Entity(string name)
        {
            _name = name;
            _components = new EntityComponentCollection(this);
            _transform = _components.AddComponent<Transform>();
        }

        public Entity(string name, EntityManagerComponent manager)
            : this(name)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            manager.RegisterEntity(this);
        }

        internal void RegisterManager(EntityManagerComponent manager)
        {
            if (_manager != null) throw new InvalidOperationException("Can not register an Entity that is already registered.");
            if (manager == null) throw new ArgumentNullException("manager");

            _manager = manager;

            foreach (var comp in _components)
            {
                _manager.RegisterComponent(comp);
            }
        }

        internal void Update(GameTime gameTime)
        {
            if(_updateDelegates != null) _updateDelegates(gameTime);
        }

        internal void OnComponentAdd(IEntityComponent comp)
        {
            var meth = ObjUtil.ExtractDelegate<Action<GameTime>>(comp, EntityConstants.MSG_UPDATE);
            if (meth != null)
            {
                _updateDelegates += meth;
            }

            if (_manager != null)
            {
                _manager.RegisterComponent(comp);
            }
        }

        internal void OnComponentRemoved(IEntityComponent comp)
        {
            var meth = ObjUtil.ExtractDelegate<Action<GameTime>>(comp, EntityConstants.MSG_UPDATE);
            if (meth != null)
            {
                _updateDelegates -= meth;
            }
        }

        #endregion

        #region Properties

        public string Name { get { return _name; } }

        public Game Game { get { return _manager.Game; } }

        public EntityManagerComponent EntityManager { get { return _manager; } }

        public EntityComponentCollection Components { get { return _components; } }

        public Transform Transform { get { return _transform; } }

        #endregion

        #region Methods

        /// <summary>
        /// Call a method with shape of delegate T. Note this method can be slow so don't overuse.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public void SendMessage<T>(string msg, params object[] args) where T : class
        {
            Delegate messageReceiver = null;

            foreach (var comp in this.Components)
            {
                var meth = ObjUtil.ExtractDelegate<T>(comp, msg);
                if (meth != null)
                {
                    messageReceiver = Delegate.Combine(messageReceiver as Delegate, meth as Delegate);
                }
            }

            if (messageReceiver != null)
            {
                try
                {
                    messageReceiver.DynamicInvoke(args);
                }
                catch
                {

                }
            }
        }

        #endregion

        #region IDisposable Interface

        private bool _bDisposed;

        private void Dispose(bool disposing)
        {
            if (!_bDisposed)
            {
                _bDisposed = true;
                _manager.DestroyEntity(this); //called after _bDisposed is set true, this way dispose isn't ran twice by DestroyEntity

                if (disposing)
                {
                    //disposed managed stuff
                    _updateDelegates = null;

                    foreach (var comp in this.Components)
                    {
                        comp.Dispose();
                    }
                }
                //disposed unmanaged stuff - nothing unmanaged here
            }
        }

        ~Entity()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Disposed
        {
            get { return _bDisposed; }
        }

        #endregion

    }
}
