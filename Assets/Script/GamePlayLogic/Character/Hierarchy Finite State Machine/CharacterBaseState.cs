using UnityEngine;

public abstract class CharacterBaseState
{
    protected CharacterBase character;
    protected StateMachine stateMachine;

    public CharacterBaseState(StateMachine stateMachine, CharacterBase character)
    {
        this.stateMachine = stateMachine;
        this.character = character;
    }
    private string StateName()
    {
        return this.GetType().Name;
    }
    public virtual void Enter() 
    {
        Debug.Log($"Enter {StateName()}");
    }
    public virtual void Update() 
    {
        Debug.Log($"Update {StateName()}");
    }
    public virtual void Exit() 
    {
        Debug.Log($"Exit {StateName()}");
    }
}