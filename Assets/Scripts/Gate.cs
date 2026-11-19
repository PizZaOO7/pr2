using UnityEngine;

namespace SimulationCourse
{
    /// <summary>
    /// Ворота (рамка): триггер в проёме. Засчитывается только полёт вперёд, по +Z рамки.
    /// Ничего не считает сам — сообщает о входе трассе GateCourse.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Gate : MonoBehaviour
    {
        [SerializeField] private Renderer[] _frame;

        private GateCourse _course;

        public int Index { get; private set; }

        public void Bind(GateCourse course, int index)
        {
            _course = course;
            Index = index;

            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5f, 4f, 0.5f);   // триггер заполняет проём 5×4
            trigger.center = Vector3.zero;
        }

        public void SetColor(Color color)
        {
            foreach (Renderer part in _frame)
            {
                if (part == null) continue;
                // URP Lit использует _BaseColor, стандартный шейдер — _Color
                if (part.material.HasProperty("_BaseColor")) part.material.SetColor("_BaseColor", color);
                if (part.material.HasProperty("_Color")) part.material.SetColor("_Color", color);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_course == null || other.attachedRigidbody == null) return;
            _course.OnGateEntered(this, other.attachedRigidbody);
        }
    }
}
