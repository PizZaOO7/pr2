using UnityEngine;
using UnityEngine.InputSystem;

namespace SimulationCourse
{
    /// <summary>
    /// Тяга вдоль корпуса (занятие 4). Space — газ вверх, Left Ctrl — снижение.
    /// Тяга задаётся ускорением (ForceMode.Acceleration), поэтому не зависит от массы.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DroneLift : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float climbAcceleration = 7f;

        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Keyboard keyboard = Keyboard.current;
            float climb = 0f;

            if (keyboard != null)
            {
                if (keyboard.spaceKey.isPressed) climb += 1f;
                if (keyboard.leftCtrlKey.isPressed) climb -= 1f;
            }

            // Тяга не бывает отрицательной: моторы не тянут вниз.
            // Без ввода acceleration == -gravity.y → дрон висит.
            float acceleration = Mathf.Max(0f, -Physics.gravity.y + climb * climbAcceleration);
            _body.AddRelativeForce(Vector3.up * acceleration, ForceMode.Acceleration);
        }
    }
}
