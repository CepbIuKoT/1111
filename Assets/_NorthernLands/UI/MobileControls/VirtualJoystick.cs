using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NorthernLands.UI.MobileControls
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0.1f, 1f)] private float handleRange = 0.55f;

        public void Configure(PlayerInputRouter inputRouter, RectTransform backgroundRect, RectTransform handleRect)
        {
            input = inputRouter;
            background = backgroundRect;
            handle = handleRect;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (input == null || background == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
                return;

            var radius = background.rect.size * 0.5f;
            var normalized = new Vector2(
                radius.x <= 0f ? 0f : localPoint.x / radius.x,
                radius.y <= 0f ? 0f : localPoint.y / radius.y);
            normalized = Vector2.ClampMagnitude(normalized, 1f);
            input.SetVirtualMove(normalized);

            if (handle != null)
                handle.anchoredPosition = Vector2.Scale(normalized, radius) * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            input?.SetVirtualMove(Vector2.zero);
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }
    }
}
