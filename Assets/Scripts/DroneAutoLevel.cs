using UnityEngine;

namespace SimulationCourse
{
    /// <summary>
    /// Стабилизатор (занятие 5): возвращает корпус к горизонту.
    /// Ошибка ориентации — Quaternion.FromToRotation(transform.up, Vector3.up);
    /// тормозится только угловая скорость наклона (ProjectOnPlane), поэтому Q/E остаются ручными.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DroneAutoLevel : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float returnGain = 8f;
        [SerializeField, Min(0f)] private float dampingGain = 3f;

        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Граница учебной модели: дрон, у которого up смотрит вниз, не восстанавливаем
            if (Vector3.Dot(transform.up, Vector3.up) <= 0f) return;

            Quaternion error = Quaternion.FromToRotation(transform.up, Vector3.up);
            error = Quaternion.Normalize(error);
            error.ToAngleAxis(out float angleDegrees, out Vector3 axis);
            if (angleDegrees > 180f) angleDegrees -= 360f;

            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            // Тормозим только наклон: вдоль transform.up — это рыскание, его оставляем пилоту
            Vector3 tiltVelocity = Vector3.ProjectOnPlane(
                _body.angularVelocity, transform.up);

            Vector3 correction = axis * (angleRadians * returnGain)
                - tiltVelocity * dampingGain;

            _body.AddTorque(correction, ForceMode.Acceleration);
        }
    }
}
