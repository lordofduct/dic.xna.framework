using System;
using System.Collections.Generic;
using System.Linq;

namespace Dic.Xna.Framework
{
    public class Notification
    {

        #region Fields

        private object _sender;
        private Entity _entity;

        #endregion

        #region CONSTRUCTOR

        public Notification()
        {

        }

        #endregion

        #region Properties

        public object Sender { get { return _sender; } }
        public Entity Entity { get { return _entity; } }

        #endregion

        #region Methods

        internal void SetSender(object sender)
        {
            _sender = sender;
            if (sender is Entity)
                _entity = sender as Entity;
            else if (sender is IEntityComponent)
                _entity = (sender as IEntityComponent).Entity;
            else
                _entity = null;
        }

        #endregion

    }


}
