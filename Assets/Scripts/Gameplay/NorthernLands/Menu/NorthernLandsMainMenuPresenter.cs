using System;
using System.Linq;
using TMPro;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Unity.BossRoom.Gameplay.NorthernLands.Menu
{
    /// <summary>
    /// Builds the mobile-first Northern Lands menu inside the existing MainMenu scene.
    /// </summary>
    public sealed class NorthernLandsMainMenuPresenter : IStartable, IDisposable
    {
        const string k_VolumeKey = "northern-lands-master-volume";

        static readonly Color k_Background = new(0.025f, 0.035f, 0.055f, 0.96f);
        static readonly Color k_Panel = new(0.06f, 0.075f, 0.105f, 0.97f);
        static readonly Color k_Gold = new(0.82f, 0.64f, 0.25f, 1f);
        static readonly Color k_Button = new(0.12f, 0.16f, 0.22f, 1f);
        static readonly Color k_ButtonPressed = new(0.22f, 0.29f, 0.38f, 1f);
        static readonly Color k_Text = new(0.93f, 0.95f, 0.98f, 1f);

        static TMP_FontAsset s_MenuFont;

        readonly NorthernLandsContentCatalog m_Content;
        readonly NorthernLandsProgressState m_Progress;
        readonly NorthernLandsCampaignLauncher m_Launcher;

        GameObject m_Root;
        GameObject m_MainPanel;
        GameObject m_RacePanel;
        GameObject m_SettingsPanel;
        GameObject m_ConfirmNewGamePanel;
        TMP_Text m_Status;
        TMP_Text m_RaceName;
        TMP_Text m_RaceDescription;
        TMP_Text m_VolumeLabel;
        RaceDefinition[] m_Races = Array.Empty<RaceDefinition>();
        int m_RaceIndex;

        public NorthernLandsMainMenuPresenter(
            NorthernLandsContentCatalog content,
            NorthernLandsProgressState progress,
            NorthernLandsCampaignLauncher launcher)
        {
            m_Content = content;
            m_Progress = progress;
            m_Launcher = launcher;
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == NorthernLandsMenuFlow.SceneName)
            {
                Build(activeScene);
            }
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (m_Root)
            {
                Object.Destroy(m_Root);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == NorthernLandsMenuFlow.SceneName)
            {
                Build(scene);
            }
            else
            {
                m_Root = null;
            }
        }

        void Build(Scene scene)
        {
            if (m_Root)
            {
                Object.Destroy(m_Root);
            }

            foreach (var sceneRoot in scene.GetRootGameObjects())
            {
                if (sceneRoot.name == "MainMenuState")
                {
                    sceneRoot.SetActive(false);
                }
            }

            m_Races = m_Content.Races
                .OrderBy(race => race.displayName, StringComparer.CurrentCulture)
                .ToArray();

            m_Root = new GameObject("NorthernLandsMainMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(m_Root, scene);
            var canvas = m_Root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = m_Root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var backdrop = CreateImage("Backdrop", m_Root.transform, k_Background);
            Stretch(backdrop.rectTransform);

            m_MainPanel = CreatePanel("Main", new Vector2(700f, 850f));
            CreateLabel(m_MainPanel.transform, "СЕВЕРНЫЕ ЗЕМЛИ XIV", 52f, new Vector2(0f, 330f), new Vector2(610f, 90f), k_Gold);
            CreateLabel(m_MainPanel.transform, "Мрачная северная action-RPG", 25f, new Vector2(0f, 270f), new Vector2(580f, 50f), k_Text);

            var continueButton = CreateButton(m_MainPanel.transform, "Продолжить", new Vector2(0f, 145f), OnContinue);
            continueButton.interactable = m_Launcher.CanContinue;
            CreateButton(m_MainPanel.transform, "Новая игра", new Vector2(0f, 45f), OnNewGame);
            CreateButton(m_MainPanel.transform, "Настройки", new Vector2(0f, -55f), ShowSettings);
            CreateButton(m_MainPanel.transform, "Выход", new Vector2(0f, -155f), Application.Quit);

            m_Status = CreateLabel(m_MainPanel.transform,
                m_Launcher.CanContinue ? "Найдено сохранение приключения" : "Сохранение ещё не создано",
                22f,
                new Vector2(0f, -280f),
                new Vector2(600f, 100f),
                new Color(0.72f, 0.78f, 0.86f, 1f));

            BuildRacePanel();
            BuildSettingsPanel();
            BuildNewGameConfirmation();
            ShowOnly(m_MainPanel);
        }

        void BuildRacePanel()
        {
            m_RacePanel = CreatePanel("RaceSelection", new Vector2(940f, 850f));
            CreateLabel(m_RacePanel.transform, "ВЫБОР ПОСТОЯННОЙ РАСЫ", 42f, new Vector2(0f, 330f), new Vector2(850f, 80f), k_Gold);
            CreateLabel(m_RacePanel.transform, "Выбор сохранится даже после полного сброса прохождения", 22f, new Vector2(0f, 275f), new Vector2(820f, 55f), k_Text);

            m_RaceName = CreateLabel(m_RacePanel.transform, string.Empty, 38f, new Vector2(0f, 170f), new Vector2(650f, 70f), k_Text);
            m_RaceDescription = CreateLabel(m_RacePanel.transform, string.Empty, 23f, new Vector2(0f, 15f), new Vector2(780f, 220f), new Color(0.82f, 0.86f, 0.92f, 1f));

            CreateButton(m_RacePanel.transform, "◀", new Vector2(-350f, 170f), PreviousRace, new Vector2(90f, 74f));
            CreateButton(m_RacePanel.transform, "▶", new Vector2(350f, 170f), NextRace, new Vector2(90f, 74f));
            CreateButton(m_RacePanel.transform, "Принять навсегда", new Vector2(0f, -195f), AcceptRace, new Vector2(440f, 78f));
            CreateButton(m_RacePanel.transform, "Назад", new Vector2(0f, -300f), () => ShowOnly(m_MainPanel), new Vector2(340f, 70f));
            RefreshRace();
        }

        void BuildSettingsPanel()
        {
            m_SettingsPanel = CreatePanel("Settings", new Vector2(700f, 700f));
            CreateLabel(m_SettingsPanel.transform, "НАСТРОЙКИ", 46f, new Vector2(0f, 245f), new Vector2(600f, 80f), k_Gold);
            CreateLabel(m_SettingsPanel.transform, "Общая громкость", 25f, new Vector2(0f, 120f), new Vector2(500f, 50f), k_Text);
            var volume = Mathf.Clamp01(PlayerPrefs.GetFloat(k_VolumeKey, 0.8f));
            AudioListener.volume = volume;
            m_VolumeLabel = CreateLabel(m_SettingsPanel.transform, VolumeText(volume), 31f, new Vector2(0f, 45f), new Vector2(500f, 55f), k_Text);
            CreateButton(m_SettingsPanel.transform, "Тише", new Vector2(-145f, -45f), () => ChangeVolume(-0.1f), new Vector2(240f, 70f));
            CreateButton(m_SettingsPanel.transform, "Громче", new Vector2(145f, -45f), () => ChangeVolume(0.1f), new Vector2(240f, 70f));
            CreateButton(m_SettingsPanel.transform, "Назад", new Vector2(0f, -210f), () => ShowOnly(m_MainPanel), new Vector2(340f, 70f));
        }

        void BuildNewGameConfirmation()
        {
            m_ConfirmNewGamePanel = CreatePanel("ConfirmNewGame", new Vector2(720f, 620f));
            CreateLabel(m_ConfirmNewGamePanel.transform, "НАЧАТЬ НОВУЮ ИГРУ?", 42f, new Vector2(0f, 205f), new Vector2(620f, 80f), k_Gold);
            CreateLabel(m_ConfirmNewGamePanel.transform,
                "Текущее приключение будет сброшено. Постоянная раса сохранится.",
                25f,
                new Vector2(0f, 70f),
                new Vector2(580f, 130f),
                k_Text);
            CreateButton(m_ConfirmNewGamePanel.transform, "Начать заново", new Vector2(0f, -75f), StartNewGameNow, new Vector2(420f, 76f));
            CreateButton(m_ConfirmNewGamePanel.transform, "Отмена", new Vector2(0f, -175f), () => ShowOnly(m_MainPanel), new Vector2(340f, 70f));
        }

        void OnContinue()
        {
            if (!m_Launcher.TryContinue(out var error))
            {
                SetStatus(error);
            }
        }

        void OnNewGame()
        {
            if (!m_Progress.HasPermanentRace)
            {
                ShowOnly(m_RacePanel);
                return;
            }

            if (m_Launcher.CanContinue)
            {
                ShowOnly(m_ConfirmNewGamePanel);
                return;
            }

            StartNewGameNow();
        }

        void StartNewGameNow()
        {
            if (!m_Launcher.TryStartNewGame(out var error))
            {
                SetStatus(error);
            }
        }

        void AcceptRace()
        {
            if (m_Races.Length == 0)
            {
                return;
            }

            if (!m_Launcher.TryChoosePermanentRace(m_Races[m_RaceIndex].id, out var error))
            {
                SetStatus(error);
                ShowOnly(m_MainPanel);
                return;
            }

            ShowOnly(m_MainPanel);
            SetStatus($"Постоянная раса: {m_Races[m_RaceIndex].displayName}");
            if (!m_Launcher.TryStartNewGame(out error))
            {
                SetStatus(error);
            }
        }

        void PreviousRace()
        {
            if (m_Races.Length == 0)
            {
                return;
            }

            m_RaceIndex = (m_RaceIndex - 1 + m_Races.Length) % m_Races.Length;
            RefreshRace();
        }

        void NextRace()
        {
            if (m_Races.Length == 0)
            {
                return;
            }

            m_RaceIndex = (m_RaceIndex + 1) % m_Races.Length;
            RefreshRace();
        }

        void RefreshRace()
        {
            if (m_Races.Length == 0 || !m_RaceName || !m_RaceDescription)
            {
                return;
            }

            var race = m_Races[m_RaceIndex];
            m_RaceName.text = $"{m_RaceIndex + 1}/{m_Races.Length}  •  {race.displayName}";
            m_RaceDescription.text =
                $"{race.description}\n\n" +
                $"Здоровье ×{race.healthMultiplier:0.00}   Мана ×{race.manaMultiplier:0.00}   " +
                $"Урон ×{race.damageMultiplier:0.00}   Скорость ×{race.speedMultiplier:0.00}\n" +
                $"Расовая способность: {race.ability}   Перезарядка: {race.cooldownSeconds:0} сек.";
        }

        void ShowSettings()
        {
            ShowOnly(m_SettingsPanel);
        }

        void ChangeVolume(float delta)
        {
            var volume = Mathf.Clamp01(AudioListener.volume + delta);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(k_VolumeKey, volume);
            PlayerPrefs.Save();
            m_VolumeLabel.text = VolumeText(volume);
        }

        static string VolumeText(float volume)
        {
            return $"{Mathf.RoundToInt(volume * 100f)}%";
        }

        void SetStatus(string message)
        {
            ShowOnly(m_MainPanel);
            m_Status.text = message;
        }

        void ShowOnly(GameObject panel)
        {
            m_MainPanel.SetActive(panel == m_MainPanel);
            m_RacePanel.SetActive(panel == m_RacePanel);
            m_SettingsPanel.SetActive(panel == m_SettingsPanel);
            m_ConfirmNewGamePanel.SetActive(panel == m_ConfirmNewGamePanel);
        }

        GameObject CreatePanel(string name, Vector2 size)
        {
            var panel = CreateImage(name, m_Root.transform, k_Panel).gameObject;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return panel;
        }

        static Button CreateButton(Transform parent, string caption, Vector2 position, UnityAction action, Vector2? size = null)
        {
            var buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(480f, 78f);
            rect.anchoredPosition = position;

            var image = buttonObject.GetComponent<Image>();
            image.color = k_Button;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = k_Button;
            colors.highlightedColor = new Color(0.18f, 0.23f, 0.31f, 1f);
            colors.pressedColor = k_ButtonPressed;
            colors.disabledColor = new Color(0.08f, 0.09f, 0.11f, 0.75f);
            button.colors = colors;
            button.onClick.AddListener(action);

            var label = CreateLabel(buttonObject.transform, caption, 28f, Vector2.zero, rect.sizeDelta, k_Text);
            label.raycastTarget = false;
            return button;
        }

        static TMP_Text CreateLabel(Transform parent, string text, float fontSize, Vector2 position, Vector2 size, Color color)
        {
            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = ResolveMenuFont();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        static TMP_FontAsset ResolveMenuFont()
        {
            if (s_MenuFont)
            {
                return s_MenuFont;
            }

            var sourceFont = Resources.Load<Font>("NorthernLands/Fonts/LiberationSans");
            if (!sourceFont)
            {
                return TMP_Settings.defaultFontAsset;
            }

            s_MenuFont = TMP_FontAsset.CreateFontAsset(sourceFont);
            s_MenuFont.name = "Northern Lands Dynamic Cyrillic";
            return s_MenuFont;
        }

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}