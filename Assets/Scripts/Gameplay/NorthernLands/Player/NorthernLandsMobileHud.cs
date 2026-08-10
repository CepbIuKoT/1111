using TMPro;
using Unity.BossRoom.Gameplay.NorthernLands.Combat;
using Unity.BossRoom.Gameplay.NorthernLands.Campaign;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.NorthernLands.Player
{
    /// <summary>
    /// Creates a landscape Android control layout without scene-specific serialized UI.
    /// </summary>
    public sealed class NorthernLandsMobileHud : MonoBehaviour
    {
        static readonly Color k_Dark = new(0.025f, 0.04f, 0.065f, 0.66f);
        static readonly Color k_Gold = new(0.82f, 0.64f, 0.25f, 0.88f);

        NorthernLandsPlayerInput m_Input;
        NorthernLandsCombatant m_Player;
        NorthernLandsCampaignDirector m_Director;
        TMP_FontAsset m_Font;
        TMP_Text m_HealthLabel;
        TMP_Text m_SilverLabel;
        TMP_Text m_ObjectiveLabel;
        TMP_Text m_StatusLabel;
        TMP_Text m_InteractionLabel;
        TMP_Text m_LocationLabel;
        GameObject m_InteractionButton;

        void Start()
        {
            m_Input = FindFirstObjectByType<NorthernLandsPlayerInput>();
            m_Director = GetComponent<NorthernLandsCampaignDirector>();
            if (!m_Input || (!Application.isMobilePlatform && !Application.isEditor))
            {
                return;
            }

            var sourceFont = Resources.Load<Font>("NorthernLands/Fonts/LiberationSans");
            m_Font = sourceFont ? TMP_FontAsset.CreateFontAsset(sourceFont) : TMP_Settings.defaultFontAsset;
            BuildCanvas();
            if (m_Director)
            {
                m_Director.UiChanged += RefreshCampaignUi;
                RefreshCampaignUi();
            }
        }

        void OnDestroy()
        {
            if (m_Director)
            {
                m_Director.UiChanged -= RefreshCampaignUi;
            }
        }

        void Update()
        {
            if (!m_HealthLabel)
            {
                return;
            }

            if (!m_Player)
            {
                var combatants = FindObjectsByType<NorthernLandsCombatant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var combatant in combatants)
                {
                    if (combatant.IsPlayer)
                    {
                        m_Player = combatant;
                        break;
                    }
                }
            }

            if (m_Player)
            {
                m_HealthLabel.text = $"ЗДОРОВЬЕ  {Mathf.CeilToInt(m_Player.Health)} / {Mathf.CeilToInt(m_Player.MaxHealth)}";
            }
            var silver = m_Director ? m_Director.NorthernSilver : 0;
            var silverText = $"СЕВЕРНОЕ СЕРЕБРО  {silver}";
            if (m_SilverLabel.text != silverText)
            {
                m_SilverLabel.text = silverText;
            }

            if (m_Director)
            {
                var interaction = m_Director.InteractionText;
                m_InteractionButton.SetActive(!string.IsNullOrEmpty(interaction));
                if (m_InteractionLabel.text != interaction)
                {
                    m_InteractionLabel.text = interaction;
                }
            }
        }

        void BuildCanvas()
        {
            var canvasObject = new GameObject("Northern Lands HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var joystickObject = CreateCircle("Движение", canvasObject.transform, new Vector2(180f, 180f), 250f, k_Dark);
            var joystickRect = joystickObject.GetComponent<RectTransform>();
            joystickRect.anchorMin = joystickRect.anchorMax = Vector2.zero;
            var knob = CreateCircle("Ручка", joystickObject.transform, Vector2.zero, 98f, new Color(0.72f, 0.82f, 0.92f, 0.9f));
            var stick = joystickObject.AddComponent<NorthernLandsVirtualJoystick>();
            stick.Configure(m_Input, joystickRect, knob.GetComponent<RectTransform>());

            CreateActionButton(canvasObject.transform, "УДАР", new Vector2(-170f, 170f), 190f, k_Gold, m_Input.QueueAttack);
            CreateActionButton(canvasObject.transform, "СИЛА", new Vector2(-360f, 275f), 145f, new Color(0.38f, 0.49f, 0.64f, 0.9f), m_Input.QueueHeavyAttack);
            CreateActionButton(canvasObject.transform, "РЫВОК", new Vector2(-375f, 105f), 145f, new Color(0.28f, 0.37f, 0.5f, 0.9f), m_Input.QueueDodge);

            var sprint = CreateCircle("Бег", canvasObject.transform, new Vector2(-555f, 120f), 125f, new Color(0.2f, 0.29f, 0.4f, 0.82f));
            var sprintRect = sprint.GetComponent<RectTransform>();
            sprintRect.anchorMin = sprintRect.anchorMax = new Vector2(1f, 0f);
            AddLabel(sprint.transform, "БЕГ", 25f);
            sprint.AddComponent<NorthernLandsSprintButton>().Configure(m_Input);

            var location = new GameObject("Location", typeof(RectTransform), typeof(TextMeshProUGUI));
            location.transform.SetParent(canvasObject.transform, false);
            var locationRect = location.GetComponent<RectTransform>();
            locationRect.anchorMin = locationRect.anchorMax = new Vector2(0.5f, 1f);
            locationRect.pivot = new Vector2(0.5f, 1f);
            locationRect.sizeDelta = new Vector2(700f, 90f);
            locationRect.anchoredPosition = new Vector2(0f, -35f);
            m_LocationLabel = location.GetComponent<TextMeshProUGUI>();
            m_LocationLabel.font = m_Font;
            m_LocationLabel.text = "СЕВЕРНЫЕ ЗЕМЛИ  •  РИВЕРХОЛЬМ";
            m_LocationLabel.fontSize = 28f;
            m_LocationLabel.color = new Color(0.92f, 0.94f, 0.98f, 0.92f);
            m_LocationLabel.alignment = TextAlignmentOptions.Center;

            m_HealthLabel = CreateHudLabel(canvasObject.transform, "Health", new Vector2(45f, -38f), new Vector2(500f, 55f));
            m_SilverLabel = CreateHudLabel(canvasObject.transform, "Silver", new Vector2(45f, -92f), new Vector2(500f, 48f));
            m_ObjectiveLabel = CreateHudLabel(canvasObject.transform, "Objective", new Vector2(45f, -145f), new Vector2(760f, 60f));
            m_ObjectiveLabel.fontSize = 23f;
            m_ObjectiveLabel.color = new Color(0.96f, 0.82f, 0.42f);

            m_StatusLabel = CreateCenteredLabel(canvasObject.transform, "Status", new Vector2(0f, -105f), new Vector2(980f, 105f), 23f);
            m_InteractionButton = CreateCenteredInteractionButton(canvasObject.transform);
            m_InteractionLabel = m_InteractionButton.GetComponentInChildren<TextMeshProUGUI>();
            m_InteractionButton.SetActive(false);
        }

        void CreateActionButton(Transform parent, string caption, Vector2 position, float size, Color color, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = CreateCircle(caption, parent, position, size, color);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            AddLabel(buttonObject.transform, caption, size > 160f ? 30f : 23f);
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            button.onClick.AddListener(action);
        }

        GameObject CreateCircle(string name, Transform parent, Vector2 position, float size, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * size;
            rect.anchoredPosition = position;
            var image = item.GetComponent<Image>();
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            image.color = color;
            return item;
        }

        void AddLabel(Transform parent, string caption, float size)
        {
            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = m_Font;
            label.text = caption;
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        TMP_Text CreateHudLabel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = m_Font;
            label.fontSize = name == "Health" ? 27f : 22f;
            label.color = name == "Health" ? new Color(0.93f, 0.35f, 0.31f) : new Color(0.55f, 0.82f, 1f);
            label.alignment = TextAlignmentOptions.Left;
            label.raycastTarget = false;
            return label;
        }

        TMP_Text CreateCenteredLabel(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = m_Font;
            label.fontSize = fontSize;
            label.color = new Color(0.94f, 0.95f, 0.98f);
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }

        GameObject CreateCenteredInteractionButton(Transform parent)
        {
            var item = new GameObject("Interaction", typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 40f);
            rect.sizeDelta = new Vector2(340f, 82f);
            var image = item.GetComponent<Image>();
            image.color = new Color(0.12f, 0.18f, 0.26f, 0.94f);
            var button = item.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => m_Director?.TryInteract());
            AddLabel(item.transform, "ДЕЙСТВИЕ", 27f);
            return item;
        }

        void RefreshCampaignUi()
        {
            if (!m_Director || !m_ObjectiveLabel || !m_StatusLabel)
            {
                return;
            }

            m_ObjectiveLabel.text = m_Director.ObjectiveText;
            m_StatusLabel.text = m_Director.StatusText;
            m_LocationLabel.text = m_Director.LocationText;
        }
    }

    sealed class NorthernLandsVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        NorthernLandsPlayerInput m_Input;
        RectTransform m_Background;
        RectTransform m_Knob;
        int m_PointerId = int.MinValue;

        public void Configure(NorthernLandsPlayerInput input, RectTransform background, RectTransform knob)
        {
            m_Input = input;
            m_Background = background;
            m_Knob = knob;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_PointerId == int.MinValue)
            {
                m_PointerId = eventData.pointerId;
                UpdateStick(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == m_PointerId)
            {
                UpdateStick(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != m_PointerId)
            {
                return;
            }

            m_PointerId = int.MinValue;
            m_Knob.anchoredPosition = Vector2.zero;
            m_Input.SetMobileMove(Vector2.zero);
        }

        void UpdateStick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Background, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }

            var direction = Vector2.ClampMagnitude(local / (m_Background.sizeDelta.x * 0.5f), 1f);
            if (direction.magnitude < 0.1f)
            {
                direction = Vector2.zero;
            }

            m_Knob.anchoredPosition = direction * m_Background.sizeDelta.x * 0.3f;
            m_Input.SetMobileMove(direction);
        }
    }

    sealed class NorthernLandsSprintButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        NorthernLandsPlayerInput m_Input;

        public void Configure(NorthernLandsPlayerInput input)
        {
            m_Input = input;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_Input.SetMobileSprint(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_Input.SetMobileSprint(false);
        }
    }
}
