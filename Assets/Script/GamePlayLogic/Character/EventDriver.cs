using System;
using System.Collections.Generic;

public class EventDriver
{
    private static EventDriver Instance;
    public static EventDriver instance
    {
        get
        {
            if (Instance == null)
                Instance = new EventDriver();
            return Instance;
        }
    }
    public Dictionary<string, Action> eventDriver = new Dictionary<string, Action>();

    private EventDriver() { }

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
