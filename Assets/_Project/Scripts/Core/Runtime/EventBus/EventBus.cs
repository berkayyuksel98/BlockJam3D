using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Merkezi event yönetim sistemi
/// </summary>
public class EventBus : Singleton<EventBus>
{
    private Dictionary<Type, List<Delegate>> eventDictionary = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// Bir event'e abone ol
    /// </summary>
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

    /// <summary>
    /// Bir event'ten aboneliği kaldır
    /// </summary>
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

    /// <summary>
    /// Bir event yayınla
    /// </summary>
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

    /// <summary>
    /// Tüm event'leri temizle
    /// </summary>
    public void ClearAll()
    {
        eventDictionary.Clear();
    }

    /// <summary>
    /// Belirli bir event tipinin tüm dinleyicilerini temizle
    /// </summary>
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
