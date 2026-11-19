using UnityEngine;
using UnityEngine.InputSystem;

namespace SimulationCourse
{
    /// <summary>
    /// Ручное управление тремя осями моментами (занятие 4):
    /// W/S — тангаж, A/D — крен, Q/E — рыскание. Моменты прикладываются в осях корпуса.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DroneManualAttitude : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float pitchAcceleration = 4f;
        [SerializeField, Min(0f)] private float rollAcceleration = 4f;
        [SerializeField, Min(0f)] private float yawAcceleration = 2f;

        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            float pitch = (keyboard.wKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed ? 1f : 0f);
            float roll = (keyboard.aKey.isPressed ? 1f : 0f)
                - (keyboard.dKey.isPressed ? 1f : 0f);
            float yaw = (keyboard.eKey.isPressed ? 1f : 0f)
                - (keyboard.qKey.isPressed ? 1f : 0f);

            // Порядок компонент Vector3: X, Y, Z = pitch, yaw, roll
            Vector3 angularAcceleration = new Vector3(
                pitch * pitchAcceleration,
                yaw * yawAcceleration,
                roll * rollAcceleration);
            _body.AddRelativeTorque(angularAcceleration, ForceMode.Acceleration);
        }
    }
}
