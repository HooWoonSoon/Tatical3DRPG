using System;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public StateMachine stateMachine;
    public EventDriver eventDriver;

    private void Awake()
    {
        stateMachine = new StateMachine();
        eventDriver = new EventDriver();

        ExplorationState explorationState = new ExplorationState(stateMachine, this);
        BattleState battleState = new BattleState(stateMachine, this);

        stateMachine.Initialize(new ExplorationState(stateMachine, this));
        eventDriver.Subscribe("BattleMode", () => stateMachine.ChangeState(battleState));
        eventDriver.Subscribe("ExplorationMode", () => stateMachine.ChangeState(explorationState));
    }
}
