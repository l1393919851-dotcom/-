using UnityEngine;

namespace Gameplay.Ocean
{
    /// <summary>
    /// 简单船体控制：W/S 前进后退，A/D 转向。
    /// </summary>
    public class SimpleBoatController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 14f;
        [SerializeField] float turnSpeed = 70f;
        [SerializeField] float seaLevelY = 0.15f;
        [SerializeField] float bobAmplitude = 0.08f;
        [SerializeField] float bobFrequency = 1.4f;

        float _bobPhase;
        float _yaw;

        public void Configure(float seaY, float bobAmp = -1f)
        {
            seaLevelY = seaY;
            if (bobAmp >= 0f)
            {
                bobAmplitude = bobAmp;
            }
        }

        void Awake()
        {
            _bobPhase = Random.Range(0f, Mathf.PI * 2f);
            _yaw = transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        void Update()
        {
            float throttle = Input.GetAxisRaw("Vertical");
            float steer = Input.GetAxisRaw("Horizontal");

            _yaw += steer * turnSpeed * Time.deltaTime;
            Quaternion rot = Quaternion.Euler(0f, _yaw, 0f);
            transform.rotation = rot;

            if (Mathf.Abs(throttle) > 0.01f)
            {
                transform.position += rot * Vector3.forward * (throttle * moveSpeed * Time.deltaTime);
            }

            Vector3 pos = transform.position;
            pos.y = seaLevelY + Mathf.Sin(Time.time * bobFrequency + _bobPhase) * bobAmplitude;
            transform.position = pos;
        }
    }
}
