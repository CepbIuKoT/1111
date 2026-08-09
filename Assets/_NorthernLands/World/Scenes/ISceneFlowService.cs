using NorthernLands.Core.Services;
using UnityEngine;

namespace NorthernLands.World.Scenes
{
    public interface ISceneFlowService : IGameService
    {
        bool IsLoading { get; }
        AsyncOperation LoadSingle(string sceneName);
    }
}
