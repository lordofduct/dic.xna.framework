using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Dic.Xna.Framework
{

    /// <summary>
    /// Represents a Transform of an entity, and facilitates nesting Entities.
    /// </summary>
    /// <remarks>
    /// I personally don't like this implementation, was playing around with ideas to storing both local and global positions within the transform 
    /// to speed up retrieving the values for various stuff. It's not the best, and needs some work. As long as the public interface doesn't change 
    /// though this can be tweaked as we move forward with the code base.
    /// </remarks>
    public sealed class Transform : EntityComponent
    {

        #region Fields
        
        private Transform _parent;
        private Transform _root;
        private ChildCollection _children;

        private Matrix _localTrans = Matrix.Identity;
        private Matrix _localInverseTrans = Matrix.Identity;
        private Vector3 _localScale = Vector3.One;
        private Quaternion _localRot = Quaternion.Identity;
        private Vector3 _localPos = Vector3.Zero;

        private Matrix _worldTrans = Matrix.Identity;
        private Vector3 _worldScale = Vector3.One;
        private Quaternion _worldRot = Quaternion.Identity;
        private Vector3 _worldPos = Vector3.Zero;

        private bool _bDirty;

        #endregion

        #region CONSTRUCTOR

        public Transform()
            : base()
        {
            _children = new ChildCollection(this);
            this.SetParent(null);
        }

        #endregion

        #region Properties

        public Transform Parent
        {
            get { return _parent; }
            set
            {
                //parent is NOT set here, it is all dealt with in side ChildCollection
                if(_parent == value) return;
                if (value == null)
                {
                    _parent.Children.Remove(this);
                }
                else
                {
                    _parent.Children.Add(this);
                }
            }
        }

        public Transform Root
        {
            get { return _root; }
        }

        public ICollection<Transform> Children
        {
            get { return _children; }
        }

        public Matrix LocalMatrix
        {
            get
            {
                if (_bDirty) this.CleanTransform();
                return _localTrans;
            }
        }

        public Matrix LocalInverseMatrix
        {
            get
            {
                if (_bDirty) this.CleanTransform();
                return _localInverseTrans;
            }
        }

        public Vector3 LocalPosition
        {
            get { return _localPos; }
            set
            {
                _localPos = value;
                _bDirty = true;
            }
        }

        public Vector3 LocalScale
        {
            get { return _localScale; }
            set
            {
                _localScale = value;
                _bDirty = true;
            }
        }

        public Quaternion LocalRotation
        {
            get { return _localRot; }
            set
            {
                _localRot = value;
                _bDirty = true;
            }
        }

        public Matrix WorldMatrix
        {
            get
            {
                if (this.CompletelyDirty) this.CleanTransform();
                return _worldTrans;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (this.CompletelyDirty) this.CleanTransform();
                return _worldPos;
            }
            set
            {
                if (_parent == null)
                {
                    _worldPos = value;
                    _localPos = _worldPos;
                    _bDirty = true;
                }
                else
                {
                    var ipm = _parent.GetGlobalToLocalMatrix();
                    _worldPos = value;
                    _localPos = Vector3.Transform(_worldPos, ipm);
                    _bDirty = true;
                }
            }
        }

        public Quaternion Rotation
        {
            get
            {
                if (this.CompletelyDirty) this.CleanTransform();
                return _worldRot;
            }
            set
            {
                if (_parent == null)
                {
                    _worldRot = value;
                    _localRot = _worldRot;
                    _bDirty = true;
                }
                else
                {
                    var iq = Quaternion.Inverse(_parent.Rotation);
                    _worldRot = value;
                    _localRot = iq * _worldRot;
                    _bDirty = true;
                }
            }
        }

        public Vector3 LossyScale
        {
            get
            {
                if (this.CompletelyDirty) this.CleanTransform();
                return _worldScale;
            }
        }

        #endregion

        #region Methods

        public bool SetLocalMatrix(Matrix m)
        {
            Vector3 s;
            Quaternion r;
            Vector3 t;
            if (m.Decompose(out s, out r, out t))
            {
                _localTrans = m;
                _localScale = s;
                _localPos = t;
                _bDirty = false;
                return true;
            }
            else
                return false;
        }

        public Matrix GetLocalToGlobalMatrix()
        {
            //Matrix mat = Matrix.Identity;
            //foreach (var m in (from p in GetParentChain().Reverse() select p.LocalMatrix))
            //{
            //    mat *= m;
            //}
            //return mat * this.LocalMatrix;
            if (_parent == null)
            {
                return this.LocalMatrix;
            }
            else
            {
                return _parent.GetLocalToGlobalMatrix() * this.LocalMatrix;
            }
        }

        public Matrix GetGlobalToLocalMatrix()
        {
            //Matrix mat = this.LocalInverseMatrix;
            //foreach (var m in (from p in GetParentChain() select p.LocalInverseMatrix))
            //{
            //    mat *= m;
            //}
            //return mat;
            if (_parent == null)
            {
                return this.LocalInverseMatrix;
            }
            else
            {
                return this.LocalInverseMatrix * _parent.GetGlobalToLocalMatrix();
            }
        }

        public IEnumerable<Transform> GetParentChain()
        {
            var p = _parent;
            while (p != null)
            {
                yield return p;
                p = _parent.Parent;
            }
        }




        private bool CompletelyDirty
        {
            get
            {
                if (_bDirty) return true;
                if (_parent != null) return _parent.CompletelyDirty;
                return false;
            }
        }

        private void CleanTransform()
        {
            _localTrans = Matrix.CreateScale(_localScale) * Matrix.CreateFromQuaternion(_localRot) * Matrix.CreateTranslation(_localPos);
            _localInverseTrans = Matrix.CreateTranslation(-_localPos) * Matrix.Transpose(Matrix.CreateFromQuaternion(_localRot)) * Matrix.CreateScale(new Vector3(1f / _localScale.X, 1f / _localScale.Y, 1f / _localScale.Z));
            if (_parent == null)
            {
                _worldTrans = _localTrans;
                _worldScale = _localScale;
                _worldRot = _localRot;
                _worldPos = _localPos;
            }
            else
            {
                _worldTrans = _parent.WorldMatrix * _localTrans;
                _worldTrans.Decompose(out _worldScale, out _worldRot, out _worldPos);
            }

            _bDirty = false;
        }

        private void SetParent(Transform par)
        {
            _parent = par;
            _root = (_parent != null) ? _parent._root : this;
            this.CleanTransform();
        }

        #endregion

        #region Special Types

        public class ChildCollection : ICollection<Transform>
        {

            #region Fields

            private List<Transform> _lst = new List<Transform>();
            private Transform _owner;

            #endregion

            #region CONSTRUCTOR

            internal ChildCollection(Transform owner)
            {
                _owner = owner;
            }

            #endregion

            #region ICollection Interface

            public int Count
            {
                get { return _lst.Count; }
            }

            bool ICollection<Transform>.IsReadOnly
            {
                get { return false; }
            }

            public void Add(Transform item)
            {
                if (_lst.Contains(item))
                {
                    //move to top of list
                    _lst.Remove(item);
                    _lst.Add(item);
                }
                else
                {
                    if (item._parent != null)
                    {
                        item._parent.Children.Remove(item);
                    }

                    _lst.Add(item);
                    item.SetParent(_owner);
                }
            }

            public bool Contains(Transform item)
            {
                return _lst.Contains(item);
            }

            public bool Remove(Transform item)
            {
                if (item._parent == _owner)
                {
                    item.SetParent(null);
                    _lst.Remove(item);
                    return true;
                }
                else
                    return false;
            }

            public void Clear()
            {
                var arr = _lst.ToArray();
                _lst.Clear();
                foreach (var item in arr)
                {
                    item.SetParent(null);
                } 
            }

            public void CopyTo(Transform[] array, int arrayIndex)
            {
                _lst.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Transform> GetEnumerator()
            {
                return _lst.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _lst.GetEnumerator();
            }

            #endregion

        }

        #endregion

        #region IDisposable Interface

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _children.Clear();
            }

            base.Dispose(disposing);
        }

        #endregion

    }
}
