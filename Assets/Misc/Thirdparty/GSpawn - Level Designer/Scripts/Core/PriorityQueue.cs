using UnityEngine;
using System.Collections.Generic;

namespace GSPAWN
{
    /// <summary>
    /// All items that can be stored in the priority queue must implement 
    /// this simple interface. This makes it possible for the priority queue 
    /// to quickly locate items based on their index in the internal array. 
    /// This is useful for example when changing an item's priority value.
    /// </summary>
    public interface IPriorityQueueLocatableItem
    {
        /// <summary>
        /// Returns the index of the item inside the queue's internal array.
        /// The implementing class only needs to implement this property in 
        /// the exact form and the priority queue will take care of the rest.
        /// </summary>
        int IndexInPriorityQueue { get; set; }
    }

    public class PriorityQueue<TData, TComparer>
        where TData : IPriorityQueueLocatableItem
        where TComparer : IComparer<TData>
    {
        private TComparer _comparer;
        private int _capacity = 50;
        private TData[] _items;
        private int _numItems = 0;

        public int NumItems { get { return _numItems; } }

        public PriorityQueue(int capacity, TComparer comparer)
        {
            _capacity = Mathf.Max(50, capacity);
            _comparer = comparer;
            _items = new TData[_capacity];
        }

        public void Clear()
        {
            _numItems = 0;
        }

        public void Enqueue(TData item)
        {
            if (_numItems + 1 > _capacity)
            {
                _capacity *= 2;
                System.Array.Resize<TData>(ref _items, _capacity);
            }

            _items[_numItems] = item;
            _items[_numItems].IndexInPriorityQueue = _numItems;

            ++_numItems;
            HeapifyUp(_numItems - 1);
        }

        public TData Dequeue()
        {
            TData item = _items[0];
            item.IndexInPriorityQueue = -1;

            _items[0] = _items[_numItems - 1];
            _items[0].IndexInPriorityQueue = 0;

            --_numItems;
            HeapifyDown(0);

            return item;
        }

        public void OnItemChangedPriority(TData item)
        {
            int itemIndex = item.IndexInPriorityQueue;
            int parentIndex = (itemIndex - 1) / 2;
            if (parentIndex >= 0)
            {
                if (_comparer.Compare(_items[itemIndex], _items[parentIndex]) == -1) HeapifyUp(itemIndex);
                else HeapifyDown(item.IndexInPriorityQueue);
            }
            else HeapifyDown(item.IndexInPriorityQueue);
        }

        private void HeapifyDown(int startIndex)
        {
            int index = startIndex;
            int leftChildIndex = index * 2 + 1;
            while (leftChildIndex < _numItems)
            {
                int smallestChildIndex = leftChildIndex;
                int rightChildIndex = index * 2 + 2;
                if (rightChildIndex < _numItems &&
                   (_comparer.Compare(_items[rightChildIndex], _items[smallestChildIndex]) == -1))
                {
                    smallestChildIndex = rightChildIndex;
                }

                if ((_comparer.Compare(_items[smallestChildIndex], _items[index]) == -1)) 
                    Swap(index, smallestChildIndex);
                else break;

                index = smallestChildIndex;
                leftChildIndex = index * 2 + 1;
            }
        }

        private void HeapifyUp(int index)
        {
            int parentIndex = (index - 1) / 2;
            while (parentIndex >= 0 && 
                  (_comparer.Compare(_items[index], _items[parentIndex]) == -1))
            {
                Swap(parentIndex, index);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }

        private void Swap(int index0, int index1)
        {
            TData temp = _items[index0];

            _items[index0] = _items[index1];
            _items[index0].IndexInPriorityQueue = index0;

            _items[index1] = temp;
            _items[index1].IndexInPriorityQueue = index1;
        }
    }
}
