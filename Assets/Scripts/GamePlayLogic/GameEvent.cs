using System;

public static class GameEvent
{
    //  Team Related Events
    public static Action onLeaderChangedRequest;
    public static Action<CharacterBase> onLeaderChanged;
    public static Action onTeamSortExchange;

    //  Skill UI Related Event
    public static Action onListOptionChanged;
}