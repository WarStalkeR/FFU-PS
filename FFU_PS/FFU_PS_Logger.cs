using System;

namespace FFU_Phase_Shift {
    public static class ModLog {
        private const string LogPrefix = "[FFU: Phase Shift]";
        public static void Info(string message) {
            UnityEngine.Debug.Log($"{LogPrefix} {message}");
        }
        public static void Warning(string message) {
            UnityEngine.Debug.LogWarning($"{LogPrefix} {message}");
        }
        public static void Error(string message) {
            UnityEngine.Debug.LogError($"{LogPrefix} {message}");
        }
        public static void Exception(Exception e) {
            UnityEngine.Debug.LogException(e);
        }
    }
}
