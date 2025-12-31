using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : Singleton<EventBus>
{
    private Dictionary<Type, List<Delegate>> eventDictionary = new Dictionary<Type, List<Delegate>>();

    //Bir evente abone ol
    public void Subscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = new List<Delegate>();
        }

        if (!eventDictionary[eventType].Contains(listener))
        {
            eventDictionary[eventType].Add(listener);
        }
    }

    //Bir eventten aboneligi kaldir
    public void Unsubscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType].Remove(listener);

            if (eventDictionary[eventType].Count == 0)
            {
                eventDictionary.Remove(eventType);
            }
        }
    }

    //Bir event yayinla
    public void Publish<T>(T gameEvent) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventDictionary.ContainsKey(eventType))
        {
            List<Delegate> listeners = new List<Delegate>(eventDictionary[eventType]);

            foreach (Delegate listener in listeners)
            {
                try
                {
                    (listener as Action<T>)?.Invoke(gameEvent);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking event {eventType.Name}: {e.Message}");
                }
            }
        }
    }

    public void ClearAll()
    {
        eventDictionary.Clear();
    }

    public void Clear<T>() where T : IGameEvent
    {
        Type eventType = typeof(T);
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary.Remove(eventType);
        }
    }

    private void OnDestroy()
    {
        ClearAll();
    }
}
