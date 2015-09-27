//
// Scroller script for Grass
//
using UnityEngine;
using UnityEngine.Serialization;

namespace Kvant
{
    [RequireComponent(typeof(Grass))]
    [AddComponentMenu("Kvant/Grass Scroller")]
    public class GrassScroller : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("yawAngle")]
        float _yawAngle;

        public float yawAngle {
            get { return _yawAngle; }
            set { _yawAngle = value; }
        }

        [SerializeField, FormerlySerializedAs("speed")]
        float _speed;

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        void Update()
        {
            var r = _yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Grass>().offset += dir * _speed * Time.deltaTime;
        }
    }
}
