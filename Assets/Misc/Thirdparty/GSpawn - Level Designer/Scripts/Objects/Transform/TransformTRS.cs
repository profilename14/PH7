#if UNITY_EDITOR
using UnityEngine;
using System;

namespace GSPAWN
{
    [Serializable]
    public struct TransformTRS
    {
        [SerializeField]
        private Vector3     _position;
        [SerializeField]
        private Quaternion  _rotation;
        [SerializeField]
        private Vector3     _scale;

        public Vector3      position    { get { return _position; } }
        public Quaternion   rotation    { get { return _rotation; } }
        public Vector3      scale       { get { return _scale; } }

        public void extract(Transform transform)
        {
            _position   = transform.position;
            _rotation   = transform.rotation;
            _scale      = transform.lossyScale;
        }

        public void apply(Transform transform)
        {
            transform.position = _position;
            transform.rotation = _rotation;
            transform.setWorldScale(_scale);
        }

        public void applyRotationAndScale(Transform transform)
        {
            transform.rotation = _rotation;
            transform.setWorldScale(_scale);
        }

        public bool rotationOrScaleDiffers(Transform transform)
        {
            return transform.lossyScale != _scale ||
                   transform.rotation != _rotation;
        }

        public bool rotationOrScaleDiffers(TransformTRS transformTRS)
        {
            return transformTRS._scale != _scale ||
                   transformTRS._rotation != _rotation;
        }
    }
}
#endif