using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dic.Xna.Framework
{
    public class EntityComponentException : Exception
    {

        public EntityComponentException()
            : base()
        {

        }

        public EntityComponentException(string msg)
            : base(msg)
        {

        }

        public EntityComponentException(string msg, Exception innerException)
            : base(msg, innerException)
        {

        }

    }

    public class EntityComponentMalformedException : EntityComponentException
    {
        
        public EntityComponentMalformedException()
            : base()
        {

        }

        public EntityComponentMalformedException(string msg)
            : base(msg)
        {

        }

        public EntityComponentMalformedException(string msg, Exception innerException)
            : base(msg, innerException)
        {

        }

    }
}
