using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Dic.Xna.Framework.Collections;

namespace Dic.Xna.Framework.Src
{
    public class NotificationGameComponent : GameComponent
    {

        #region Fields

        private WeakKeyDictionary<object, NotificationHandlerCollection> _senderSpecificNotificationHandlers = new WeakKeyDictionary<object, NotificationHandlerCollection>();
        private NotificationHandlerCollection _globalNotificationHandlers = new NotificationHandlerCollection();

        private TimeSpan _purgeDelay;
        private TimeSpan _lastPurge;

        #endregion

        #region CONSTRUCTOR

        public NotificationGameComponent(Game game)
            : base(game)
        {
            _purgeDelay = TimeSpan.FromMinutes(5.0);
        }

        public NotificationGameComponent(Game game, TimeSpan delay)
            : base(game)
        {
            _purgeDelay = delay;
        }

        #endregion

        #region Properties

        public TimeSpan PurgeDelay
        {
            get { return _purgeDelay; }
            set { _purgeDelay = value; }
        }

        #endregion

        #region Methods

        public void RegisterObserver<T>(NotificationHandler<T> handler, bool useWeakReference = false) where T : Notification
        {
            _globalNotificationHandlers.RegisterObserver<T>(handler, useWeakReference);
        }

        public void RemoveObserver<T>(NotificationHandler<T> handler) where T : Notification
        {
            _globalNotificationHandlers.RemoveObserver<T>(handler);
        }

        public void RegisterObserver<T>(object sender, NotificationHandler<T> handler, bool useWeakReference = false) where T : Notification
        {
            _senderSpecificNotificationHandlers.Clean();

            NotificationHandlerCollection coll;
            if (!_senderSpecificNotificationHandlers.ContainsKey(sender))
            {
                coll = new NotificationHandlerCollection();
                _senderSpecificNotificationHandlers[sender] = coll;
            }
            else
            {
                coll = _senderSpecificNotificationHandlers[sender];
            }
            coll.RegisterObserver<T>(handler, useWeakReference);
        }

        public void RemoveObserver<T>(object sender, NotificationHandler<T> handler) where T : Notification
        {
            _senderSpecificNotificationHandlers.Clean();

            if (!_senderSpecificNotificationHandlers.ContainsKey(sender)) return;

            var coll = _senderSpecificNotificationHandlers[sender];
            coll.RemoveObserver<T>(handler);
            if (coll.IsEmpty)
            {
                _senderSpecificNotificationHandlers.Remove(sender);
            }
        }

        public void PostNotification<T>(object sender, this T notification) where T : Notification
        {
            if (notification == null) throw new ArgumentNullException("notification");
            if (sender == null) throw new ArgumentNullException("sender");
            notification.SetSender(sender);

            //we first notify those registered directly with the sender
            if (_senderSpecificNotificationHandlers.ContainsKey(sender))
            {
                _senderSpecificNotificationHandlers[sender].PostNotification<T>(notification);
            }

            //if the sender was an Entity source, let anyone registered with the Entity hear about it
            var ent = notification.Entity;
            if (ent != null && ent != sender)
            {
                if (_senderSpecificNotificationHandlers.ContainsKey(ent))
                {
                    _senderSpecificNotificationHandlers[ent].PostNotification<T>(notification);
                }
            }

            //finally let anyone registered with the global hear about it
            _globalNotificationHandlers.PostNotification<T>(notification);
        }

        #endregion

        #region Overrides

        public override void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - _lastPurge > _purgeDelay)
            {
                _lastPurge = gameTime.TotalGameTime;

                //clean any dead references as this allows for all weak referencing objects
                _globalNotificationHandlers.Purge();

                _senderSpecificNotificationHandlers.Clean();
                foreach (var coll in _senderSpecificNotificationHandlers.Values)
                {
                    coll.Purge();
                }
            }
        }

        #endregion

        #region Special Types

        public delegate void NotificationHandler<T>(T notification) where T : Notification;

        private class NotificationHandlerCollection
        {

            #region Fields

            private Dictionary<Type, Delegate> _table = new Dictionary<Type, Delegate>();
            private ListDictionary<Type, Delegate> _weakTable = new ListDictionary<Type, Delegate>(() => new WeakList<Delegate>());

            #endregion

            #region CONSTRUCTOR

            public NotificationHandlerCollection()
            {

            }

            #endregion

            #region Properties

            public bool IsEmpty { get { return _table.Count == 0 && _weakTable.Count == 0; } }

            #endregion

            #region Methods

            public void Purge()
            {
                foreach (var lst in _weakTable.Lists)
                {
                    (lst as WeakList<Delegate>).Clean();
                }
                _weakTable.Purge();
            }

            public void RegisterObserver<T>(NotificationHandler<T> handler, bool useWeakReference = false) where T : Notification
            {
                var tp = typeof(T);
                if (useWeakReference)
                {
                    _weakTable.Add(tp, handler);
                }
                else
                {
                    System.Delegate d = (_table.ContainsKey(tp)) ? _table[tp] : null;
                    //this would only fail if someone modified this code to allow adding mismatched delegates with notification types
                    d = Delegate.Combine(d, handler);
                    _table[tp] = d;
                }
            }

            public void RemoveObserver<T>(NotificationHandler<T> handler) where T : Notification
            {
                var tp = typeof(T);
                if (_weakTable.ContainsKey(tp) && _weakTable.Lists[tp].Contains(handler))
                {
                    _weakTable.Lists[tp].Remove(handler);
                }
                else if (_table.ContainsKey(tp))
                {
                    var d = _table[tp];
                    if (d != null)
                    {
                        d = Delegate.Remove(d, handler);
                        if (d != null)
                        {
                            _table[tp] = d;
                        }
                        else
                        {
                            _table.Remove(tp);
                        }
                    }
                }
            }

            public void PostNotification<T>(T notification) where T : Notification
            {
                var tp = typeof(T);
                NotificationHandler<T> d = (_table.ContainsKey(tp)) ? _table[tp] as NotificationHandler<T> : null;
                if (_weakTable.ContainsKey(tp))
                {
                    if (_weakTable.Lists[tp].Count == 0)
                    {
                        _weakTable.Remove(tp);
                    }
                    else
                    {
                        var wd = Delegate.Combine(_weakTable.Lists[tp].ToArray()) as NotificationHandler<T>;
                        d += wd;
                    }
                }

                if (d != null) d(notification);
            }

            #endregion

        }

        #endregion

    }
}
