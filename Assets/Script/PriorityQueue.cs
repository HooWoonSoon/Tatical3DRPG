using System;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> heap = new List<T>();

    public int Count => heap.Count;

    public void Enqueue(T item)
    {
        heap.Add(item);
        int i = heap.Count - 1;
        int parent = (i - 1) / 2;

        while (i > 0 && heap[parent].CompareTo(heap[i]) > 0)
        {
            Swap(i, parent);
            i = parent;
            parent = (i - 1) / 2;
        }
    }

    public T Dequeue()
    {
        if (heap.Count == 0) throw new InvalidOperationException("Queue is empty.");

        T ret = heap[0];
        int last = heap.Count - 1;

        if (heap.Count == 1)
        {
            heap.Clear();
            return ret;
        }

        heap[0] = heap[last];
        heap.RemoveAt(last);

        int i = 0;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left < heap.Count && heap[left].CompareTo(heap[smallest]) < 0)
                smallest = left;
            if (right < heap.Count && heap[right].CompareTo(heap[smallest]) < 0)
                smallest = right;
            if (smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
        return ret;
    }

    public bool Contains(T item)
    {
        return heap.Contains(item);
    }

    private void Swap(int i, int j)
    {
        T temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}