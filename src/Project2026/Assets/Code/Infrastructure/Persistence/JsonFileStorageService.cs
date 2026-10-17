using Code.Infrastructure.Serialization;
using System;
using System.IO;
using UnityEngine;
using VContainer;

namespace Code.Infrastructure.Persistence
{
    public class JsonFileStorageService : IStorageService
    {
        private readonly string _rootPath;

        [Inject]
        public JsonFileStorageService() : this(Path.Combine(Application.persistentDataPath, "Saves")) { }

        public JsonFileStorageService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public bool TryLoad<T>(string key, out T data) where T : class
        {
            data = null;
            var path = GetPath(key);

            if (!File.Exists(path))
                return false;

            string json;

            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read save '{key}': {exception.Message}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                Quarantine(path, key, "Save file is empty.");
                return false;
            }

            try
            {
                data = json.FromJson<T>();
            }
            catch (Exception exception)
            {
                Quarantine(path, key, exception.Message);
                return false;
            }

            if (data != null)
                return true;

            Quarantine(path, key, "Save file deserialized to null.");
            return false;
        }

        public void Save<T>(string key, T data) where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var path = GetPath(key);
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, data.ToJson());

            if (!File.Exists(path))
            {
                File.Move(tempPath, path);
                return;
            }

            File.Replace(tempPath, path, null);
        }

        private string GetPath(string key) => Path.Combine(_rootPath, key + ".json");

        private static void Quarantine(string path, string key, string reason)
        {
            var corruptPath = path + ".corrupt";

            try
            {
                if (File.Exists(corruptPath))
                    File.Delete(corruptPath);

                if (File.Exists(path))
                    File.Move(path, corruptPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to quarantine save '{key}': {exception.Message}");
                return;
            }

            Debug.LogWarning($"Discarding damaged save '{key}': {reason}");
        }
    }
}
