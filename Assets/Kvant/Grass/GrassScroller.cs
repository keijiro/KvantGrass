//
// Scroller script for Grass
//
using UnityEngine;

namespace Kvant
{
    [RequireComponent(typeof(Grass))]
    [AddComponentMenu("Kvant/Grass Scroller")]
    public class GrassScroller : MonoBehaviour
    {
        public float yawAngle;
        public float speed;

        void Update()
        {
            var r = yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Grass>().offset += dir * speed * Time.deltaTime;
        }
    }
}
