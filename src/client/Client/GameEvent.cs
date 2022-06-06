namespace Vezel.Novadrop.Client;

[SuppressMessage("", "CA1008")]
public enum GameEvent
{
    EnteredIntroCinematic = 1001,
    EnteredServerList = 1002,
    EnteringLobby = 1003,
    EnteredLobby = 1004,
    EnteringCharacterCreation = 1005,
    LeftLobby = 1006,
    DeletedCharacter = 1007,
    CanceledCharacterCreation = 1008,
    EnteredCharacterCreation = 1009,
    CreatedCharacter = 1010,
    EnteredWorld = 1011,
    FinishedLoadingScreen = 1012,
    LeftWorld = 1013,
    MountedPegasus = 1014,
    DismountedPegasus = 1015,
    ChangedChannel = 1016,
}
