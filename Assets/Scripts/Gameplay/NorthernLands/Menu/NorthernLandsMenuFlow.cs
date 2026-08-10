namespace Unity.BossRoom.Gameplay.NorthernLands.Menu
{
    public enum NorthernLandsMenuAction
    {
        Continue,
        NewGame,
        Settings,
        Exit
    }

    public enum NorthernLandsMenuDestination
    {
        Disabled,
        RaceSelection,
        Campaign,
        Settings,
        Exit
    }

    /// <summary>
    /// Deterministic navigation rules for the startup menu. Presentation remains owned by the MainMenu scene.
    /// </summary>
    public static class NorthernLandsMenuFlow
    {
        public const string SceneName = "MainMenu";

        public static NorthernLandsMenuDestination Resolve(
            NorthernLandsMenuAction action,
            bool hasRunSave,
            bool hasPermanentRace)
        {
            return action switch
            {
                NorthernLandsMenuAction.Continue => hasRunSave
                    ? NorthernLandsMenuDestination.Campaign
                    : NorthernLandsMenuDestination.Disabled,
                NorthernLandsMenuAction.NewGame => hasPermanentRace
                    ? NorthernLandsMenuDestination.Campaign
                    : NorthernLandsMenuDestination.RaceSelection,
                NorthernLandsMenuAction.Settings => NorthernLandsMenuDestination.Settings,
                NorthernLandsMenuAction.Exit => NorthernLandsMenuDestination.Exit,
                _ => NorthernLandsMenuDestination.Disabled
            };
        }
    }
}
