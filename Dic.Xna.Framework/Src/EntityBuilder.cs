using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dic.Xna.Framework
{
    public class EntityBuilder
    {

        #region Fields

        private EntityManagerComponent _manager;
        private List<Entity> _cache = new List<Entity>();

        #endregion

        #region CONSTRUCTOR

        public EntityBuilder(EntityManagerComponent manager)
        {
            _manager = manager;
        }

        #endregion

        #region Properties

        public EntityManagerComponent Manager { get { return _manager; } }

        #endregion

        #region Methods

        public Entity CreateEntity(string name = null)
        {
            var ent = new Entity(name, _manager);
            _cache.Add(ent);
            return ent;
        }

        public Entity CreateEntity(string name, params Type[] componentTypes)
        {
            var ent = new Entity(name, _manager);
            foreach (var tp in componentTypes)
            {
                ent.Components.AddComponent(tp);
            }
            _cache.Add(ent);
            return ent;
        }

        public void RegisterEntities()
        {
            var arr = _cache.ToArray();
            _cache.Clear();

            foreach (var ent in arr)
            {
                _manager.RegisterEntity(ent);
            }
        }

        #endregion

    }
}
