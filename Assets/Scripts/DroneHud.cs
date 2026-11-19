using UnityEngine;
using UnityEngine.InputSystem;

namespace SimulationCourse
{
    /// <summary>
    /// HUD задания: счётчик рамок, таймер, состояние стабилизатора.
    /// T — включить/выключить стабилизатор (сравнение поведения, опциональный пункт задания).
    /// </summary>
    public sealed class DroneHud : MonoBehaviour
    {
        [SerializeField] private GateCourse _course;
        [SerializeField] private DroneAutoLevel _stabilizer;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tKey.wasPressedThisFrame && _stabilizer != null)
                _stabilizer.enabled = !_stabilizer.enabled;
        }

        private void OnGUI()
        {
            const float width = 300f;
            GUI.Box(new Rect(10f, 10f, width, 88f), string.Empty);

            if (_course != null)
            {
                GUI.Label(new Rect(22f, 16f, width - 24f, 22f),
                    $"Рамки: {_course.Passed} / {_course.Total}" + (_course.Finished ? "  (готово)" : ""));
                GUI.Label(new Rect(22f, 40f, width - 24f, 22f),
                    $"Время: {_course.Elapsed:0.0} с");
            }

            bool stabilized = _stabilizer == null || _stabilizer.enabled;
            GUI.Label(new Rect(22f, 64f, width - 24f, 22f),
                $"Стабилизатор: {(stabilized ? "ВКЛ" : "ВЫКЛ")}  (T)");

            GUI.Label(new Rect(12f, Screen.height - 30f, 700f, 22f),
                "Space — газ, Left Ctrl — снижение, W/S — тангаж, A/D — крен, Q/E — рыскание, T — стабилизатор");
        }
    }
}
