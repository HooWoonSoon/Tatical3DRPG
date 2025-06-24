using UnityEngine;

public abstract class CharacterBaseState
{
    protected Character character;
    protected StateMachine stateMachine;


    public CharacterBaseState(StateMachine stateMachine, Character character)
    {
        this.stateMachine = stateMachine;
        this.character = character;
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

    }
    public virtual void Exit() 
    {
        Debug.Log($"Exit {StateName()}");
    }
}