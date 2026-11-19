using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SimulationCourse.EditorTools
{
    /// <summary>
    /// Собирает сцену задания №2 «Дрон сквозь рамки»: земля, дрон на одном Rigidbody
    /// (тяга + ручное управление + стабилизатор), коридор из 5 рамок разной высоты,
    /// трасса GateCourse со счётчиком, HUD, свет и камера.
    /// </summary>
    public static class Assignment02SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/02-DroneGates.unity";

        private static readonly Vector3[] GateHeights =
        {
            new Vector3(0f, 2.5f, 20f),
            new Vector3(1.5f, 3.5f, 35f),
            new Vector3(-1.5f, 2.5f, 50f),
            new Vector3(1f, 4f, 65f),
            new Vector3(-1f, 3f, 80f),
        };

        [MenuItem("Course/Build Assignment 02 (Дрон сквозь рамки)")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayerSettings.runInBackground = true;

            BuildLight();
            BuildGround();

            Rigidbody droneBody = BuildDrone(out DroneAutoLevel stabilizer);
            Gate[] gates = BuildGates();
            BuildCourse(droneBody, gates);
            BuildHud(stabilizer);
            BuildCamera();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log($"[Course] Сцена задания №2 собрана: {ScenePath}");
        }

        private static void BuildLight()
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 40f);
            ground.transform.localScale = new Vector3(200f, 1f, 200f);
            SetColor(ground.GetComponent<Renderer>(), new Color(0.35f, 0.45f, 0.3f));
        }

        private static Rigidbody BuildDrone(out DroneAutoLevel stabilizer)
        {
            var drone = new GameObject("Drone");
            drone.transform.position = new Vector3(0f, 2.5f, 5f);   // перед первой рамкой, носом по +Z

            var body = drone.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.angularDamping = 1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.useGravity = true;
            body.isKinematic = false;

            var box = drone.AddComponent<BoxCollider>();
            box.size = new Vector3(0.6f, 0.15f, 0.6f);

            drone.AddComponent<DroneLift>();
            drone.AddComponent<DroneManualAttitude>();
            stabilizer = drone.AddComponent<DroneAutoLevel>();

            // --- Визуальная модель: дочерние объекты без коллайдеров ---
            Transform root = drone.transform;

            var bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyCollider(bodyVisual);
            bodyVisual.name = "Body";
            bodyVisual.transform.SetParent(root, false);
            bodyVisual.transform.localScale = new Vector3(0.45f, 0.12f, 0.45f);
            SetColor(bodyVisual.GetComponent<Renderer>(), new Color(0.2f, 0.2f, 0.25f));

            Vector3[] rotorOffsets =
            {
                new Vector3(0.3f, 0.08f, 0.3f), new Vector3(-0.3f, 0.08f, 0.3f),
                new Vector3(0.3f, 0.08f, -0.3f), new Vector3(-0.3f, 0.08f, -0.3f),
            };
            for (int i = 0; i < rotorOffsets.Length; i++)
            {
                var rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyCollider(rotor);
                rotor.name = "Rotor" + (i + 1);
                rotor.transform.SetParent(root, false);
                rotor.transform.localPosition = rotorOffsets[i];
                rotor.transform.localScale = new Vector3(0.3f, 0.01f, 0.3f);
                SetColor(rotor.GetComponent<Renderer>(), new Color(0.7f, 0.7f, 0.75f));
            }

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyCollider(nose);
            nose.name = "Nose";
            nose.transform.SetParent(root, false);
            nose.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            nose.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);
            SetColor(nose.GetComponent<Renderer>(), Color.red);   // показывает «вперёд» (+Z)

            return body;
        }

        /// <summary>Рамка: проём 5×4 м, толщина перекладины 0,25 м, +Z — направление пролёта.</summary>
        private static Gate BuildGate(string name, Vector3 position)
        {
            var frame = new GameObject(name);
            frame.transform.position = position;   // без поворота: +Z смотрит вдоль коридора

            Renderer[] bars = new Renderer[4];

            // Боковые стойки: полная высота 4.5, внутренний зазор по ширине ровно 5
            bars[0] = CreateBar(frame.transform, "Left", new Vector3(-2.625f, 0f, 0f), new Vector3(0.25f, 4.5f, 0.25f));
            bars[1] = CreateBar(frame.transform, "Right", new Vector3(2.625f, 0f, 0f), new Vector3(0.25f, 4.5f, 0.25f));
            // Верх и низ: перекрывают проём между стойками, внутренний зазор по высоте ровно 4
            bars[2] = CreateBar(frame.transform, "Top", new Vector3(0f, 2.125f, 0f), new Vector3(5.5f, 0.25f, 0.25f));
            bars[3] = CreateBar(frame.transform, "Bottom", new Vector3(0f, -2.125f, 0f), new Vector3(5.5f, 0.25f, 0.25f));

            // Триггер в проёме: (5, 4, 0.5) — задаст Bind()
            frame.AddComponent<BoxCollider>();

            var gate = frame.AddComponent<Gate>();
            SetReference(gate, "_frame", bars);

            return gate;
        }

        private static Renderer CreateBar(Transform parent, string name, Vector3 localPosition, Vector3 scale)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = name;
            bar.transform.SetParent(parent, false);
            bar.transform.localPosition = localPosition;
            bar.transform.localScale = scale;
            SetColor(bar.GetComponent<Renderer>(), new Color(0.75f, 0.75f, 0.75f));
            return bar.GetComponent<Renderer>();
        }

        private static Gate[] BuildGates()
        {
            var gates = new Gate[GateHeights.Length];
            for (int i = 0; i < GateHeights.Length; i++)
                gates[i] = BuildGate("Frame" + (i + 1), GateHeights[i]);
            return gates;
        }

        private static void BuildCourse(Rigidbody droneBody, Gate[] gates)
        {
            var courseGo = new GameObject("GateCourse");
            var course = courseGo.AddComponent<GateCourse>();
            SetReference(course, "_vehicle", droneBody);
            SetReference(course, "_gates", gates);
        }

        private static void BuildHud(DroneAutoLevel stabilizer)
        {
            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<DroneHud>();
            SetReference(hud, "_course", Object.FindAnyObjectByType<GateCourse>());
            SetReference(hud, "_stabilizer", stabilizer);
        }

        private static void BuildCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(-7f, 7f, -6f);
            cameraGo.transform.LookAt(new Vector3(0f, 3f, 45f));
        }

        private static void DestroyCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static void SetColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            // В edit mode: отдельный материал на каждый объект — иначе renderer.material
            // создаёт утечку, а sharedMaterial перекрасил бы общий дефолтный материал
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            renderer.sharedMaterial = material;
        }

        private static void SetReference(Component component, string propertyName, Object value)
        {
            var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Course] Поле {propertyName} не найдено у {component.GetType().Name}");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(Component component, string propertyName, Object[] values)
        {
            var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[Course] Поле {propertyName} не найдено у {component.GetType().Name}");
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
