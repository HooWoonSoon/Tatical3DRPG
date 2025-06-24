using UnityEngine;

public abstract class CharacterBaseState
{
    protected Character player;
    protected StateMachine stateMachine;
    protected CharacterBaseState currentSubState;
    public CharacterBaseState(StateMachine stateMachine, Character player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }
    public virtual string StateName()
    {
        return this.GetType().Name;
    }
    public virtual void Enter() 
    {
        Debug.Log($"Enter {StateName()}");
    }
    public virtual void Update() 
    {
        currentSubState?.Update();
    }
    public virtual void Exit() 
    {
        Debug.Log($"Exit {StateName()}");
    }
    public virtual void SetSubState(CharacterBaseState newSubState) 
    {
        currentSubState = newSubState;
        currentSubState.Enter();
    }
}