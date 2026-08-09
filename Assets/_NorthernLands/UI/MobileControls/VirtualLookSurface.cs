using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NorthernLands.UI.MobileControls
{
    public sealed class VirtualLookSurface : MonoBehaviour, IDragHandler
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField, Min(0.01f)] private float sensitivity = 0.75f;

        public void Configure(PlayerInputRouter inputRouter) => input = inputRouter;

        public void OnDrag(PointerEventData eventData)
        {
            input?.AddVirtualLook(eventData.delta * sensitivity);
        }
    }
}
