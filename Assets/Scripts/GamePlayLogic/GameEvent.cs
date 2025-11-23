using System;

public static class GameEvent
{
    //  Team Related Events
    public static Action onLeaderChangedRequest;
    public static Action<CharacterBase> onLeaderChanged;
    public static Action onTeamSortExchange;

    //  Skill UI Related Event
    public static Action onListOptionChanged;

    //  Battle Related Event
    public static Action onStartBattle;
    public static Action onEndBattle;

    //  Battle UI Related Event
    public static Action OnBattleUIFinish;

    //  Deployment Related Event
    public static Action onStartDeployment;
    public static Action onEndDeployment;

    public static Action<SkillData> onSkillCastStart;
    public static Action onSkillCastEnd;
}