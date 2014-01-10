using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework.Src
{
    public class SpriteRenderer : EntityComponent
    {

        protected override void Initialize()
        {
            base.Initialize();

            var renderManager = this.Game.Components.OfType<SpriteRenderManagerComponent>().FirstOrDefault();
            if (renderManager == null) throw new EntityComponentException("Renderer component requires game to have a RenderManagerComponent registered with it.");
            renderManager.RegisterRenderer(this);
        }

        protected internal abstract void Draw(GameTime gameTime);

    }
}
