using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NorthernLands.UI.MobileControls
{
    public sealed class VirtualActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Action
        {
            LightAttack,
            HeavyAttack,
            Block,
            Dash,
            Interact,
            RaceAbility
        }

        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private Action action;

        public void Configure(PlayerInputRouter inputRouter, Action buttonAction)
        {
            input = inputRouter;
            action = buttonAction;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (input == null)
                return;

            switch (action)
            {
                case Action.LightAttack: input.PressVirtualLightAttack(); break;
                case Action.HeavyAttack: input.PressVirtualHeavyAttack(); break;
                case Action.Block: input.SetVirtualBlock(true); break;
                case Action.Dash: input.PressVirtualDash(); break;
                case Action.Interact: input.PressVirtualInteract(); break;
                case Action.RaceAbility: input.PressVirtualRaceAbility(); break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (action == Action.Block)
                input?.SetVirtualBlock(false);
        }
    }
}
