using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{
    /// <summary>
    /// This interface exists only to allow for creating component interfaces that explicitly inherit from IEntityComponent 
    /// for contractual purposes. When creating EntityComponents, always inherit from EntityComponent instead of rolling 
    /// your own.
    /// </summary>
    /// <remarks>
    /// If you MUST roll your own be sure to include a private method called 'OnAddedToEntity' that accepts an Entity as its 
    /// only parameter and returns void. In implementing this method be sure to set store this reference to the 'owner' and 
    /// use this object for the implementation of this interface. And when implementing IDisposable you MUST call 
    /// DestroyComponent on the EntityManager for the component, and also because Dispose may be called repeatedly, track 
    /// the first time it was called with a boolean and don't do anything if it's already been called once.
    /// </remarks>
    public interface IEntityComponent : IDisposable
    {

        Entity Entity { get; }
        Game Game { get; }
        EntityManagerComponent EntityManager { get; }
        Transform Transform { get; }
        EntityComponentCollection Components { get; }

        /// <summary>
        /// Called on main update thread when initializing the component. Components initialize in random order, you can retrieve other components on 
        /// self but don't inter-communicate. Wait for Start before you inter-communicate.
        /// </summary>
        void Initialize();
        /// <summary>
        /// Called on main update thread the first time the component is updated. This happens after all components have initialized so inter-communication 
        /// between components is safe.
        /// </summary>
        void Start();


        void SendMessage<T>(string msg, params object[] args) where T : class;

    }
}
