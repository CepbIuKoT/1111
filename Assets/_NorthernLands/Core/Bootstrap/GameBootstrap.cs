using NorthernLands.Core.Services;
using NorthernLands.Core.StateMachine;
using NorthernLands.SaveSystem;
using NorthernLands.World.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthernLands.Core.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public ServiceRegistry Services { get; private set; }

        [SerializeField, Range(30, 120)] private int targetFrameRate = 60;
        [SerializeField] private string firstSceneName = "01_MainMenu";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = targetFrameRate;

            Services = new ServiceRegistry();
            RegisterServices(Services);
            Services.InitializeAll();

            EnterFirstScene();
        }

        private static void RegisterServices(ServiceRegistry services)
        {
            services.Register(new GameStateMachine());
            services.Register<ISaveGameService>(new JsonSaveGameService());
            services.Register<IPermanentProfileStore>(new PermanentProfileStore());
            services.Register<ISceneFlowService>(new SceneFlowService());
        }

        private void EnterFirstScene()
        {
            var stateMachine = Services.Get<GameStateMachine>();
            if (SceneManager.GetActiveScene().name == firstSceneName)
            {
                stateMachine.ChangeTo(GameState.MainMenu);
                return;
            }

            stateMachine.ChangeTo(GameState.Loading);
            var operation = Services.Get<ISceneFlowService>().LoadSingle(firstSceneName);
            operation.completed += _ => stateMachine.ChangeTo(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            Services?.ShutdownAll();
            Instance = null;
        }
    }
}
