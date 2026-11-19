using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace SimulationCourse.EditorTools
{
    /// <summary>
    /// Однократно подключает MCP-мост при загрузке редактора, если локальный MCP-сервер уже
    /// запущен. Аналог нажатия Start Server в окне Window → MCP for Unity.
    /// </summary>
    internal static class McpAutoConnect
    {
        private const string SessionKey = "SimulationCourse.McpAutoConnect.Attempted";
        private const int MaxAttempts = 10;
        private const double RetryIntervalSeconds = 2.0;

        private static int _attempts;
        private static double _nextAttemptTime;
        private static bool _starting;

        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            // Play Mode в фоне: иначе Unity замирает на первом кадре, пока окно не в фокусе
            PlayerSettings.runInBackground = true;

            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                if (MCPServiceLocator.Bridge.IsRunning)
                {
                    EditorApplication.update -= Tick;
                    Debug.Log("[MCP] Мост уже подключён.");
                    return;
                }

                // Сервер не запущен — молча выходим, без попыток
                if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
                {
                    EditorApplication.update -= Tick;
                    return;
                }

                if (EditorApplication.timeSinceStartup < _nextAttemptTime) return;

                if (_attempts >= MaxAttempts)
                {
                    EditorApplication.update -= Tick;
                    Debug.LogWarning("[MCP] Не удалось подключить мост за " + MaxAttempts + " попыток.");
                    return;
                }

                _attempts++;
                _nextAttemptTime = EditorApplication.timeSinceStartup + RetryIntervalSeconds;
                _ = TryStartAsync();
            }
            catch (Exception exception)
            {
                EditorApplication.update -= Tick;
                Debug.LogWarning("[MCP] Ошибка подключения: " + exception.Message);
            }
        }

        private static async Task TryStartAsync()
        {
            if (_starting) return;
            _starting = true;
            try
            {
                bool started = await MCPServiceLocator.Bridge.StartAsync();
                if (started)
                {
                    EditorApplication.update -= Tick;
                    Debug.Log("[MCP] Мост подключён к серверу.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[MCP] StartAsync: " + exception.Message);
            }
            finally
            {
                _starting = false;
            }
        }
    }
}
