using UnityEngine;

namespace Gameplay.Ocean
{
    /// <summary>
    /// 第三人称跟随船体。
    /// </summary>
    public class OceanCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 12f, -18f);
        [SerializeField] float followSmooth = 6f;
        [SerializeField] float lookAtHeight = 1f;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float t = 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
            Vector3 desired = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, desired, t);

            Vector3 toTarget = (target.position + Vector3.up * lookAtHeight) - transform.position;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion lookRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            Quaternion current = transform.rotation;
            if (current.x * current.x + current.y * current.y + current.z * current.z + current.w * current.w < 0.0001f)
            {
                current = Quaternion.identity;
            }
            else
            {
                current = Quaternion.Normalize(current);
            }

            transform.rotation = Quaternion.Normalize(Quaternion.Slerp(current, lookRot, t));
        }
    }
}
