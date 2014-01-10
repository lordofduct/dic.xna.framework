using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{

    /// <summary>
    /// Component that facilitates rendering an entity. Game MUST have a RenderManagerComponent (or component that 
    /// inherits from RenderManagerComponent) registered with it. If more than 1 type of RenderManagerComponent is 
    /// connected to the game the first one found will be used.
    /// </summary>
    /// <remarks>
    /// TODO - consider the possible need to support more than 1 RenderManagerComponent on the Game (various types 
    /// for instance?). We can give each RenderManagerComponent a id tag, and than the Renderer EntityComponent has 
    /// its own renderer tag which flags which RenderManager it should be associated with.
    /// </remarks>
    public abstract class Renderer : EntityComponent
    {
        protected override void Initialize()
        {
            base.Initialize();

            var renderManager = this.Game.Components.OfType<RenderManagerComponent>().FirstOrDefault();
            if (renderManager == null) throw new EntityComponentException("Renderer component requires game to have a RenderManagerComponent registered with it.");
            renderManager.RegisterRenderer(this);
        }

        protected internal abstract void Draw(GameTime gameTime);

    }

}
