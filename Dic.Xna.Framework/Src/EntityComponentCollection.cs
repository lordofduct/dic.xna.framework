using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Dic.Xna.Framework.Utils;

namespace Dic.Xna.Framework
{
    public class EntityComponentCollection : IEnumerable<IEntityComponent>
    {

        #region Fields

        private Entity _owner;

        private List<IEntityComponent> _lst = new List<IEntityComponent>();

        #endregion

        #region CONSTRUCTOR

        internal EntityComponentCollection(Entity owner)
        {
            _owner = owner;
        }

        #endregion

        #region Properties



        #endregion

        #region Methods

        public T AddComponent<T>() where T : class, IEntityComponent
        {
            T comp = this.GetComponent<T>();
            if (comp != null) return comp;

            try
            {
                comp = System.Activator.CreateInstance<T>();
            }
            catch
            {
                return null;
            }

            if (comp is EntityComponent)
            {
                _lst.Add(comp);
                (comp as EntityComponent).OnAddedToEntity(_owner);
            }
            else
            {
                var meth = ObjUtil.ExtractDelegate<Action<Entity>>(comp, EntityConstants.MSG_ONADDEDTOENTITY);
                if (meth != null)
                {
                    _lst.Add(comp);
                    meth(_owner);
                }
                else
                {
                    throw new EntityComponentMalformedException("Custom rolled IEntityComponents must contain a 'OnAddedToEntity' method present as a member.");
                }
            }

            _owner.EntityManager.RegisterComponent(comp);
            _owner.OnComponentAdd(comp);

            return comp;
        }

        public IEntityComponent AddComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            IEntityComponent comp = this.GetComponent(tp);
            if (comp != null) return comp;

            try
            {
                comp = System.Activator.CreateInstance(tp) as IEntityComponent;

            }
            catch
            {
                return null;
            }

            if (comp is EntityComponent)
            {
                _lst.Add(comp);
                (comp as EntityComponent).OnAddedToEntity(_owner);
            }
            else
            {
                var meth = ObjUtil.ExtractDelegate<Action<Entity>>(comp, EntityConstants.MSG_ONADDEDTOENTITY);
                if (meth != null)
                {
                    _lst.Add(comp);
                    meth(_owner);
                }
                else
                {
                    throw new EntityComponentMalformedException("Custom rolled IEntityComponents must contain a 'OnAddedToEntity' method present as a member.");
                }
            }

            _owner.EntityManager.RegisterComponent(comp);
            _owner.OnComponentAdd(comp);

            return comp;
        }

        public bool HasComponent<T>() where T : class, IEntityComponent
        {
            var tp = typeof(T);

            return _lst.Any((c) => c.GetType() == tp);
        }

        public bool HasComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            return _lst.Any((c) => c.GetType() == tp);
        }

        public bool HasLikeComponent<T>() where T : class, IEntityComponent
        {
            return _lst.Any((c) => c is T);
        }

        public bool HasLikeComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            return _lst.Any((c) => tp.IsAssignableFrom(c.GetType()));
        }

        public T GetComponent<T>() where T : class, IEntityComponent
        {
            var tp = typeof(T);
            return (from c in _lst where c.GetType() == tp select c as T).FirstOrDefault();
        }

        public IEntityComponent GetComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            return (from c in _lst where c.GetType() == tp select c).FirstOrDefault();
        }

        public T GetLikeComponent<T>() where T : class, IEntityComponent
        {
            return (from c in _lst where c is T select c as T).FirstOrDefault();
        }

        public IEntityComponent GetLikeComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            return (from c in _lst where tp.IsAssignableFrom(c.GetType()) select c).FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>() where T : class, IEntityComponent
        {
            return from c in _lst where c is T select c as T;
        }

        public IEnumerable<IEntityComponent> GetComponents(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            return from c in _lst where tp.IsAssignableFrom(c.GetType()) select c;
        }



        public bool RemoveComponent<T>() where T : class, IEntityComponent
        {
            var comp = this.GetComponent<T>();
            if (comp != null) return this.RemoveComponent(comp);

            return false;
        }

        public bool RemoveComponent(Type tp)
        {
            if (tp == null) throw new ArgumentNullException("tp");
            if (!typeof(IEntityComponent).IsAssignableFrom(tp)) throw new ArgumentException("Type must implement IEntityComponent.");

            var comp = this.GetComponent(tp);
            if (comp != null) return this.RemoveComponent(comp);

            return false;
        }

        public bool RemoveComponent(IEntityComponent comp)
        {
            if (_lst.Contains(comp))
            {
                _lst.Remove(comp);
                comp.Dispose();
                return true;
            }

            return false;
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerator<IEntityComponent> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        #endregion

    }
}
