using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthernLands.World.Scenes
{
    public sealed class SceneFlowService : ISceneFlowService
    {
        public bool IsLoading { get; private set; }

        public void Initialize()
        {
        }

        public AsyncOperation LoadSingle(string sceneName)
        {
            if (IsLoading)
                throw new InvalidOperationException("A scene transition is already running.");
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is required.", nameof(sceneName));

            IsLoading = true;
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                IsLoading = false;
                throw new InvalidOperationException($"Scene '{sceneName}' could not be loaded.");
            }

            operation.completed += _ => IsLoading = false;
            return operation;
        }

        public void Shutdown()
        {
            IsLoading = false;
        }
    }
}
