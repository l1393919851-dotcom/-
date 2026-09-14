using UnityEngine;

namespace Gameplay.Ocean
{
    /// <summary>
    /// 大块水面跟随目标 XZ，配合世界坐标 UV 形成无限海观感。
    /// </summary>
    public class InfiniteOceanSurface : MonoBehaviour
    {
        [SerializeField] Transform followTarget;
        [SerializeField] float seaLevelY;
        [SerializeField] float snapGrid = 4f;

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        public void SetSeaLevel(float y)
        {
            seaLevelY = y;
        }

        void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            Vector3 p = followTarget.position;
            float x = snapGrid > 0.001f ? Mathf.Round(p.x / snapGrid) * snapGrid : p.x;
            float z = snapGrid > 0.001f ? Mathf.Round(p.z / snapGrid) * snapGrid : p.z;
            transform.position = new Vector3(x, seaLevelY, z);
        }
    }
}
