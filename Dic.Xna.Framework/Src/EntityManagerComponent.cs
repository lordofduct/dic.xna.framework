using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{

    /// <summary>
    /// This class manages all entities in a game. There should only be 1 entity manager per Game. When the update cycle is suspended (Enabled is false) 
    /// the entities and their components won't be updated. This can allow for building large scenes that may take several update cycles (i.e. multi-threaded) 
    /// without the Awake, Start, and Update messages being dispatched to Entities.
    /// </summary>
    /// <remarks>
    /// This little bastard is slightly annoying because there are multiple collections that need to be synced properly depending on the thread that accesses it. 
    /// The big problem arises from the fact that a lot of the methods that modify the collections are called by the dependent code that is called during the Update 
    /// method. This causes the Update method to lock itself up if not handled properly. Hence the weird locking spaghetti nonsense in here.
    /// </remarks>
    public class EntityManagerComponent : GameComponent
    {

        #region Fields

        /// <summary>
        /// Rules:
        /// lock in phases None, InitializingPhase, StartingPhase
        /// never modify Entities list in UpdatingPhase
        /// </summary>
        private enum UpdatePhase
        {
            None = 0,
            InitializingPhase = 1,
            StartingPhase = 2,
            UpdatingPhase = 3,
            CleanUp = 4
        }

        private System.Threading.Thread _updateThread = null;
        private UpdatePhase _phase = UpdatePhase.None;

        private object _lock = new object();
        private List<Entity> _entities = new List<Entity>();
        private List<Entity> _initializeCache = new List<Entity>();
        private List<IEntityComponent> _componentInitializeCache = new List<IEntityComponent>();
        private ConcurrentQueue<IEntityComponent> _startPool = new ConcurrentQueue<IEntityComponent>();
        private List<Entity> _deadEntityList = new List<Entity>();

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

            if (_updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
            {
                //occurred during the main update thread while this Update method is running
                switch (_phase)
                {
                    case UpdatePhase.StartingPhase:
                        if (!_entities.Contains(entity))
                        {
                            _entities.Add(entity); //safe to add here, entities isn't modified during startcallloop
                            var comps = entity.Components.ToArray(); //any components that get added during Initialize will end up in the auto-initialized in RegisterComponent
                            foreach (var comp in comps)
                            {
                                comp.Initialize();
                                _startPool.Enqueue(comp);
                            }
                        }
                        break;
                    case UpdatePhase.None:
                    case UpdatePhase.InitializingPhase:
                    case UpdatePhase.UpdatingPhase:
                    case UpdatePhase.CleanUp:
                        lock (_lock)
                        {
                            if (!_entities.Contains(entity))
                            {
                                _initializeCache.Add(entity);
                            }
                        }
                        break;
                }
            }
            else
            {
                //happened outside of the main update loop, just lock and add
                lock (_lock)
                {
                    if (!_entities.Contains(entity))
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

            if (_updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
            {
                //occurred during the main update thread while this Update method is running
                switch (_phase)
                {
                    case UpdatePhase.StartingPhase:
                        if (_entities.Contains(component.Entity))
                        {
                            component.Initialize();
                            _startPool.Enqueue(component);
                        }
                        break;
                    case UpdatePhase.None:
                    case UpdatePhase.InitializingPhase:
                    case UpdatePhase.UpdatingPhase:
                    case UpdatePhase.CleanUp:
                        lock (_lock)
                        {
                            if (_entities.Contains(component.Entity))
                            {
                                _componentInitializeCache.Add(component);
                            }
                        }
                        break;
                }
            }
            else
            {
                //happened outside of the main update loop, just lock and add if entity has already been initialized
                lock (_lock)
                {
                    if (_entities.Contains(component.Entity))
                    {
                        _componentInitializeCache.Add(component);
                    }
                }
            }
        }

        public void DestroyEntity(Entity entity)
        {
            if (entity.EntityManager != this) throw new ArgumentException("Can only register entity with the EntityManagerComponent it was constructed with.", "entity");

            if (_updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
            {
                //occurred during the main update thread while this Update method is running
                switch (_phase)
                {
                    case UpdatePhase.None:
                    case UpdatePhase.InitializingPhase:
                    case UpdatePhase.StartingPhase:
                    case UpdatePhase.CleanUp:
                        //dispose the entity before removing it from anything, otherwise the components won't be able to properly be destroyed (this dispose calls dispose on all components which subsequently calls DestroyComponent)
                        entity.Dispose();
                        lock (_lock)
                        {
                            _entities.Remove(entity);
                            _initializeCache.Remove(entity);
                        }
                        break;
                    case UpdatePhase.UpdatingPhase:
                        //we can't modify the entity list during this phase, so delay until afterward
                        entity.Dispose();
                        lock (_lock)
                        {
                            if (_entities.Contains(entity))
                            {
                                _deadEntityList.Add(entity);
                            }
                            _initializeCache.Remove(entity);
                        }
                        break;
                }
            }
            else
            {
                //dispose the entity before removing it from anything, otherwise the components won't be able to properly be destroyed (this dispose calls dispose on all components which subsequently calls DestroyComponent)
                entity.Dispose();
                if (_phase == UpdatePhase.UpdatingPhase)
                {
                    lock (_lock)
                    {
                        if (_entities.Contains(entity))
                        {
                            _deadEntityList.Add(entity);
                        }
                        _initializeCache.Remove(entity);
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        _entities.Remove(entity);
                        _initializeCache.Remove(entity);
                    }
                }
            }
        }

        public void DestroyComponent(IEntityComponent component)
        {
            if (component == null) throw new ArgumentNullException("component");
            if (component.Entity == null) throw new EntityComponentException("Malformed IEntityComponent, not attached to an Entity.");
            if (component.EntityManager == null) throw new ArgumentException("Can only register component with the EntityManagerComponent it's entity is managed by.", "component");

            component.Components.RemoveComponent(component);

            if (_updateThread != null && System.Threading.Thread.CurrentThread == _updateThread)
            {
                //occurred during the main update thread while this Update method is running
                switch (_phase)
                {
                    case UpdatePhase.None:
                    case UpdatePhase.InitializingPhase:
                    case UpdatePhase.StartingPhase:
                    case UpdatePhase.UpdatingPhase:
                    case UpdatePhase.CleanUp:
                        lock (_lock)
                        {
                            _componentInitializeCache.Remove(component);
                        }
                        break;
                }
            }
            else
            {
                lock (_lock)
                {
                    _componentInitializeCache.Remove(component);
                }
            }

            component.Dispose();
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

            //initialize all all entities and their components
            _phase = UpdatePhase.InitializingPhase;
            _updateThread = System.Threading.Thread.CurrentThread; //store a reference to the currently running thread

            //we keep doing this until we register empty, this catches all entities created during the Initialize call to any component
            while (_initializeCache.Count > 0)
            {
                Entity[] cache;
                //lock onto the cache and get all the entities out, this way when Awake and Start are called those messages can create Entities without issue
                lock (_lock)
                {
                    cache = _initializeCache.ToArray();
                    _initializeCache.Clear();
                    //Add to the entity list here, entities are only added to the actual list in the main update thread for thread-safety
                    _entities.AddRange(cache);
                }

                //initialize the entity
                foreach (var entity in cache)
                {
                    //initialize all components on the entity
                    var comps = entity.Components.ToArray();
                    foreach (var comp in comps)
                    {
                        comp.Initialize();
                        _startPool.Enqueue(comp);
                    }
                }

            }

            //we keep doing this until we register empty, this catches all components that are added during the Initialize call to any component
            while (_componentInitializeCache.Count > 0)
            {
                IEntityComponent[] cache;
                lock (_lock)
                {
                    cache = _componentInitializeCache.ToArray();
                    _componentInitializeCache.Clear();
                }

                foreach (var comp in cache)
                {
                    comp.Initialize();
                    _startPool.Enqueue(comp);
                }
            }

            //now call start on all that need it, we keep doing this until we register empty incase any components are registered during the call to Start
            _phase = UpdatePhase.StartingPhase;
            while (_startPool.Count > 0)
            {
                IEntityComponent comp;
                if (_startPool.TryDequeue(out comp))
                {
                    comp.Start();
                }
            }

            //perform update on all live entities
            _phase = UpdatePhase.UpdatingPhase;
            foreach (var entity in _entities)
            {
                if (!entity.Disposed) entity.Update(gameTime);
            }

            //clean dead entity list
            _phase = UpdatePhase.CleanUp;
            while (_deadEntityList.Count > 0)
            {
                Entity[] cache;
                lock(_lock)
                {
                    cache = _deadEntityList.ToArray();
                    _deadEntityList.Clear();
                }
                foreach (var entity in cache)
                {
                    _entities.Remove(entity);
                }
            }

            _updateThread = null;
            _phase = UpdatePhase.None;
        }

        #endregion

    }
}
