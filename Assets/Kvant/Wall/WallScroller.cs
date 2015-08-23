//
// Scroller script for Wall
//
using UnityEngine;

namespace Kvant
{
    [RequireComponent(typeof(Wall))]
    [AddComponentMenu("Kvant/Wall Scroller")]
    public class WallScroller : MonoBehaviour
    {
        public float yawAngle;
        public float speed;

        void Update()
        {
            var r = yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Wall>().offset += dir * speed * Time.deltaTime;
        }
    }
}
