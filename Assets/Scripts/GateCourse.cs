using UnityEngine;

namespace SimulationCourse
{
    /// <summary>Трасса из ворот: порядок прохождения, счётчик и время от первой до последней рамки.</summary>
    public sealed class GateCourse : MonoBehaviour
    {
        private static readonly Color PendingColor = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color NextColor = new Color(1f, 0.8f, 0.1f);
        private static readonly Color PassedColor = new Color(0.2f, 0.8f, 0.3f);

        [SerializeField] private Rigidbody _vehicle;
        [SerializeField] private Gate[] _gates;

        private bool _running;

        public int Passed { get; private set; }
        public int Total => _gates.Length;
        public bool Finished => Passed >= _gates.Length;
        public float Elapsed { get; private set; }

        private void Awake()
        {
            for (int i = 0; i < _gates.Length; i++) _gates[i].Bind(this, i);
            RefreshColors();
        }

        private void Update()
        {
            if (_running && !Finished) Elapsed += Time.deltaTime;
        }

        public void OnGateEntered(Gate gate, Rigidbody body)
        {
            if (body != _vehicle || Finished || gate.Index != Passed) return;
            if (Vector3.Dot(body.linearVelocity, gate.transform.forward) <= 0f) return;

            _running = true;
            Passed++;
            RefreshColors();
        }

        private void RefreshColors()
        {
            for (int i = 0; i < _gates.Length; i++)
            {
                _gates[i].SetColor(i < Passed ? PassedColor : i == Passed ? NextColor : PendingColor);
            }
        }
    }
}
