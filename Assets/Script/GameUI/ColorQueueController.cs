using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorQueueController : MonoBehaviour
{
    [Header("Queue")]
    [Range(1, 5)] public int capacity = 3;
    public bool strictSingleCart = false;

    readonly List<Route> slots = new List<Route>(5);

    public event Action<IReadOnlyList<Route>> OnQueueChanged;

    void Awake()
    {
        slots.Clear();
        for (int i = 0; i < capacity; i++) slots.Add(Route.None);
    }

    public void ClearAll()
    {
        for (int i = 0; i < capacity; i++) slots[i] = Route.None;
        OnQueueChanged?.Invoke(slots);
    }

    public int CountNonEmpty()
    {
        int c = 0;
        for (int i = 0; i < capacity; i++) if (slots[i] != Route.None) c++;
        return c;
    }

    public void Enqueue(Route color)
    {
        if (color == Route.None) return;

        for (int i = 0; i < capacity; i++)
        {
            if (slots[i] == Route.None)
            {
                slots[i] = color;
                OnQueueChanged?.Invoke(slots);
                return;
            }
        }

        // เต็มแล้ว → เขียนทับช่องท้าย
        slots[capacity - 1] = color;
        OnQueueChanged?.Invoke(slots);
    }

    public Route PeekHead()
    {
        for (int i = 0; i < capacity; i++)
            if (slots[i] != Route.None) return slots[i];
        return Route.None;
    }

    public bool ResolveForSingleCart(Route incoming, bool resetAfter = true)
    {
        if (strictSingleCart && CountNonEmpty() > 1)
        {
            if (resetAfter) ClearAll();
            return false;
        }

        var head = PeekHead();
        bool correct = (head != Route.None && head == incoming);

        if (resetAfter) ClearAll();
        return correct;
    }

    public bool ResolveForTrain(IReadOnlyList<Route> incomingSequence, bool resetAfter = true)
    {
        int want = Mathf.Min(incomingSequence.Count, capacity);
        var temp = new List<Route>(slots);

        int headIndex = 0;
        for (; headIndex < capacity; headIndex++)
            if (temp[headIndex] != Route.None) break;

        for (int i = 0; i < want; i++)
        {
            if (headIndex >= capacity) { if (resetAfter) ClearAll(); return false; }
            if (temp[headIndex] == Route.None) { if (resetAfter) ClearAll(); return false; }
            if (temp[headIndex] != incomingSequence[i]) { if (resetAfter) ClearAll(); return false; }
            headIndex++;
        }

        if (resetAfter) ClearAll();
        return true;
    }
}