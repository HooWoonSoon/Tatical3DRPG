using System;
using System.Collections.Generic;

public class EventDriver
{
    public Dictionary<string, Action> eventDriver = new Dictionary<string, Action>();
    public StateMachine stateMachine;

    public void Subscribe(string eventName, Action action)
    {
        if (!eventDriver.ContainsKey(eventName))
            eventDriver[eventName] = action;
        else
            eventDriver[eventName] += action;
    }

    public void Desubscribe(string eventName, Action action)
    {
        if (eventDriver.ContainsKey(eventName))
            eventDriver[eventName] -= action;
    }

    public void Trigger(string eventName)
    {
        if (eventDriver.ContainsKey(eventName))
            eventDriver[eventName]?.Invoke();
    }
}
