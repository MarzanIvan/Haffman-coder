using System;
using System.Collections.Generic;

namespace source_project
{
    public class PriorityQueue<T>
    {
        int size;
        SortedDictionary<int, Queue<T>> Nodes;
        public int Size() => size;

        public PriorityQueue()
        {
            Nodes = new SortedDictionary<int, Queue<T>>();
            size = 0;
        }
        public void Push(int priority, T item)
        {
            if (!Nodes.ContainsKey(priority))
            {
                Nodes.Add(priority, new Queue<T>());
            }
            Nodes[priority].Enqueue(item);
            size++;
        }
        public T Pop()
        {
            if (size == 0)
            {
                throw new Exception();
            }
            --size;
            foreach(Queue<T> q in Nodes.Values) { 
                if (q.Count > 0)
                {
                    return q.Dequeue();
                }
            }
            throw new Exception();
        }

    }
}
