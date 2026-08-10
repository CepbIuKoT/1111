using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.UserInput
{
    /// <summary>
    /// Runtime-created mobile joystick. It only becomes visible while the locally owned character exists.
    /// </summary>
    public class MobileMovementJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        const float k_JoystickSize = 210f;
        const float k_DeadZone = 0.12f;
        const float k_InputLookupInterval = 0.5f;

        RectTransform m_Background;
        RectTransform m_Knob;
        CanvasGroup m_CanvasGroup;
        ClientInputSender m_InputSender;
        Vector2 m_Direction;
        int m_ActivePointerId = int.MinValue;
        float m_NextInputLookup;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateForMobile()
        {
            if (!Application.isMobilePlatform || FindFirstObjectByType<MobileMovementJoystick>())
            {
                return;
            }

            var canvasObject = new GameObject("Mobile Controls", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasObject);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var joystickObject = new GameObject("Movement Joystick", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image), typeof(MobileMovementJoystick));
            joystickObject.transform.SetParent(canvasObject.transform, false);

            var background = joystickObject.GetComponent<RectTransform>();
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.zero;
            background.pivot = new Vector2(0.5f, 0.5f);
            background.anchoredPosition = new Vector2(155f, 155f);
            background.sizeDelta = Vector2.one * k_JoystickSize;

            var backgroundImage = joystickObject.GetComponent<Image>();
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            backgroundImage.color = new Color(0.05f, 0.08f, 0.12f, 0.58f);

            var knobObject = new GameObject("Knob", typeof(RectTransform), typeof(Image));
            knobObject.transform.SetParent(joystickObject.transform, false);
            var knob = knobObject.GetComponent<RectTransform>();
            knob.anchorMin = new Vector2(0.5f, 0.5f);
            knob.anchorMax = new Vector2(0.5f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.sizeDelta = Vector2.one * 92f;
            knob.anchoredPosition = Vector2.zero;

            var knobImage = knobObject.GetComponent<Image>();
            knobImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            knobImage.color = new Color(0.16f, 0.61f, 0.96f, 0.92f);
            knobImage.raycastTarget = false;

            joystickObject.GetComponent<MobileMovementJoystick>().Configure(background, knob);
        }

        void Configure(RectTransform background, RectTransform knob)
        {
            m_Background = background;
            m_Knob = knob;
            m_CanvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        void Update()
        {
            if (Time.unscaledTime >= m_NextInputLookup)
            {
                m_NextInputLookup = Time.unscaledTime + k_InputLookupInterval;
                FindOwnedInputSender();
            }

            if (m_InputSender && m_Direction.sqrMagnitude >= k_DeadZone * k_DeadZone)
            {
                m_InputSender.RequestMobileMove(m_Direction);
            }
        }

        void FindOwnedInputSender()
        {
            if (m_InputSender && m_InputSender.IsSpawned && m_InputSender.IsOwner)
            {
                SetVisible(true);
                return;
            }

            m_InputSender = null;
            var senders = FindObjectsByType<ClientInputSender>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var sender in senders)
            {
                if (sender.IsSpawned && sender.IsOwner)
                {
                    m_InputSender = sender;
                    break;
                }
            }

            SetVisible(m_InputSender);
        }

        void SetVisible(bool visible)
        {
            if (!m_CanvasGroup)
            {
                return;
            }

            m_CanvasGroup.alpha = visible ? 1f : 0f;
            m_CanvasGroup.interactable = visible;
            m_CanvasGroup.blocksRaycasts = visible;
            if (!visible)
            {
                ResetInput(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_ActivePointerId != int.MinValue)
            {
                return;
            }

            m_ActivePointerId = eventData.pointerId;
            UpdateDirection(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == m_ActivePointerId)
            {
                UpdateDirection(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == m_ActivePointerId)
            {
                ResetInput(true);
            }
        }

        void UpdateDirection(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_Background, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var radius = k_JoystickSize * 0.5f;
            m_Direction = Vector2.ClampMagnitude(localPoint / radius, 1f);
            if (m_Direction.magnitude < k_DeadZone)
            {
                m_Direction = Vector2.zero;
            }

            m_Knob.anchoredPosition = m_Direction * radius * 0.58f;
        }

        void ResetInput(bool stopCharacter)
        {
            m_ActivePointerId = int.MinValue;
            m_Direction = Vector2.zero;
            if (m_Knob)
            {
                m_Knob.anchoredPosition = Vector2.zero;
            }

            if (stopCharacter && m_InputSender)
            {
                m_InputSender.StopMobileMove();
            }
        }
    }
}
