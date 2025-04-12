using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerStateMachine player;
    protected StateMachine stateMachine;
    protected PlayerBaseState currentSubState;
    public PlayerBaseState(StateMachine stateMachine, PlayerStateMachine player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
    public virtual void SetSubState(PlayerBaseState newSubState) 
    {
        currentSubState = newSubState;
        currentSubState.Enter();    
    }
}