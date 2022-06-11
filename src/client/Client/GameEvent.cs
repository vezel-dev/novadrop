namespace Vezel.Novadrop.Client;

[SuppressMessage("", "CA1008")]
public enum GameEvent
{
    EnteredIntroCinematic = 0x3e9,
    EnteredServerList = 0x3ea,
    EnteringLobby = 0x3eb,
    EnteredLobby = 0x3ec,
    EnteringCharacterCreation = 0x3ed,
    LeftLobby = 0x3ee,
    DeletedCharacter = 0x3ef,
    CanceledCharacterCreation = 0x3f0,
    EnteredCharacterCreation = 0x3f1,
    CreatedCharacter = 0x3f2,
    EnteredWorld = 0x3f3,
    FinishedLoadingScreen = 0x3f4,
    LeftWorld = 0x3f5,
    MountedPegasus = 0x3f6,
    DismountedPegasus = 0x3f7,
    ChangedChannel = 0x3f8,
}
