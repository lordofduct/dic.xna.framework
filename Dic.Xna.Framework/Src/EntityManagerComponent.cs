using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{

    /// <summary>
    /// This class manages all entities in a game. There should only be 1 entity manager per Game. When the update cycle is suspended (Enabled is false) 
    /// the entities and their components won't be updated. This can allow for building large scenes that may take several update cycles (i.e. multi-threaded) 
    /// without the Awake, Start, and Update messages being dispatched to Entities.
    /// </summary>
    public class EntityManagerComponent : GameComponent
    {

        #region Fields

        private System.Threading.Thread _updateThread = null;
        private bool _bInStartCallLoop = false;

        private List<Entity> _entities = new List<Entity>();
        private List<Entity> _initializeCache = new List<Entity>();
        private List<IEntityComponent> _componentInitializeCache = new List<IEntityComponent>();
        private List<IEntityComponent> _startPool = new List<IEntityComponent>();

        #endregion

        #region CONSTRUCTOR

        public EntityManagerComponent(Game game)
            : base(game)
        {
            
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// This method is how entities register themselves with an EntityManager
        /// </summary>
        /// <param name="entity"></param>
        internal void RegisterEntity(Entity entity)
        {
            if (entity.EntityManager != this) throw new ArgumentException("Can only register entity with the EntityManagerComponent it was constructed with.", "entity");

            if (!_entities.Contains(entity))
            {
                if (_bInStartCallLoop && _updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
                {
                    //this means an entity was created by another component during its Start call, initialize that entity immediately and add its components to the StartPool to be called there
                    _entities.Add(entity);
                    var comps = entity.Components.ToArray(); //any components that get added during Initialize will end up in the auto-initialized in RegisterComponent
                    foreach (var comp in comps)
                    {
                        comp.Initialize();
                        _startPool.Add(comp);
                    }
                }
                else
                {
                    //defer the adding of the entity to the main list to the main update thread, this helps with thread-syncing
                    //we lock on the cache list for thread-safety
                    lock (_initializeCache)
                    {
                        _initializeCache.Add(entity);
                    }
                }
            }
        }

        internal void RegisterComponent(IEntityComponent component)
        {
            if (component == null) throw new ArgumentNullException("component");
            if (component.Entity == null) throw new EntityComponentException("Malformed IEntityComponent, not attached to an Entity.");
            if (component.EntityManager == null) throw new ArgumentException("Can only register component with the EntityManagerComponent it's entity is managed by.", "component");

            if (_entities.Contains(component.Entity))
            {
                if (_bInStartCallLoop && _updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
                {
                    //this means a component was added by another component during its Start call, initialize that component immediately and add it to the StartPool to be called there
                    component.Initialize();
                    _startPool.Add(component);
                }
                else
                {
                    //if the entity has already been initialized
                    lock (_componentInitializeCache)
                    {
                        _componentInitializeCache.Add(component);
                    }
                }
            }
        }

        public void DestroyEntity(Entity entity)
        {
            if (entity.EntityManager != this) throw new ArgumentException("Can only register entity with the EntityManagerComponent it was constructed with.", "entity");

            if (_entities.Contains(entity))
            {
                _entities.Remove(entity);
                entity.Dispose();
            }
        }

        public void DestroyComponent(IEntityComponent component)
        {
            if (component == null) throw new ArgumentNullException("component");
            if (component.Entity == null) throw new EntityComponentException("Malformed IEntityComponent, not attached to an Entity.");
            if (component.EntityManager == null) throw new ArgumentException("Can only register component with the EntityManagerComponent it's entity is managed by.", "component");

            component.Components.RemoveComponent(component);
        }

        public Entity CreateEntity()
        {
            var entity = new Entity(this);
            return entity;
        }

        public Entity CreateEntity(params Type[] componentTypes)
        {
            var entity = new Entity(this);
            foreach (var tp in componentTypes)
            {
                entity.Components.AddComponent(tp);
            }
            return entity;
        }

        public Entity Find(string name)
        {
            //TODO - implement this with transform parenting support

            foreach (var entity in _entities)
            {
                if (entity.Name == name) return entity;
            }

            return null;
        }

        #endregion

        #region Component Events

        public override void Update(GameTime gameTime)
        {
            //in this case, this means that Update was called by someone while Update is already running, this is considered BAD
            if(_updateThread != null) throw new Exception("Illegal call to Update on EntityManagerComponent.");

            base.Update(gameTime);

            _updateThread = System.Threading.Thread.CurrentThread; //store a reference to the currently running thread

            if (_initializeCache.Count > 0)
            {
                Entity[] cache;
                //lock onto the cache and get all the entities out, this way when Awake and Start are called those messages can create Entities without issue
                lock (_initializeCache)
                {
                    cache = _initializeCache.ToArray();
                    _initializeCache.Clear();
                }
                //Add to the entity list here, entities are only added to the actual list in the main update thread for thread-safety
                _entities.AddRange(cache);

                //initialize the entity
                foreach (var entity in cache)
                {
                    //initialize all components on the entity
                    var comps = entity.Components.ToArray();
                    foreach (var comp in comps)
                    {
                        comp.Initialize();
                        _startPool.Add(comp);
                    }
                }

            }

            //we keep doing this until we register empty, this catches all components that are added during the Initialize call to any component
            while (_componentInitializeCache.Count > 0)
            {
                IEntityComponent[] cache;
                lock (_componentInitializeCache)
                {
                    cache = _componentInitializeCache.ToArray();
                    _componentInitializeCache.Clear();
                }

                foreach (var comp in cache)
                {
                    comp.Initialize();
                    _startPool.Add(comp);
                }
            }

            //now call start on all that need it, we keep doing this until we register empty incase any components are registered during the call to Start
            _bInStartCallLoop = true;
            while (_startPool.Count > 0)
            {
                var pool = _startPool.ToArray();
                _startPool.Clear();
                foreach (var comp in pool)
                {
                    comp.Start();
                }
            }
            _bInStartCallLoop = false;

            foreach (var entity in _entities)
            {
                entity.Update(gameTime);
            }

            _updateThread = null;
        }

        #endregion

    }
}
