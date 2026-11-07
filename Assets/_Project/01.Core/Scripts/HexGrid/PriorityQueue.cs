using System;
using System.Collections.Generic;

namespace Project.Core.HexGrid
{
    /// <summary>
    /// .NET Standard 2.1 (Unity 환경)을 위한 무할당(Zero-GC) 최소 힙 우선순위 큐
    /// </summary>
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _elements;

        public PriorityQueue(int initialCapacity = 256)
        {
            _elements = new List<(TElement, TPriority)>(initialCapacity);
        }

        public int Count => _elements.Count;

        public void Clear() => _elements.Clear();

        public void Enqueue(TElement element, TPriority priority)
        {
            _elements.Add((element, priority));
            int currentIndex = _elements.Count - 1;

            // Up-Heap (부모 노드와 비교하여 더 작으면 위로 올림)
            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;
                if (_elements[currentIndex].Priority.CompareTo(_elements[parentIndex].Priority) >= 0)
                    break;

                var tmp = _elements[currentIndex];
                _elements[currentIndex] = _elements[parentIndex];
                _elements[parentIndex] = tmp;

                currentIndex = parentIndex;
            }
        }

        public TElement Dequeue()
        {
            if (_elements.Count == 0) throw new InvalidOperationException("Queue is empty.");

            var bestItem = _elements[0].Element;
            int lastIndex = _elements.Count - 1;

            // 마지막 노드를 루트로 옮김
            _elements[0] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);
            lastIndex--;

            // Down-Heap (자식 노드들과 비교하여 더 작은 쪽으로 내림)
            int parentIndex = 0;
            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1;
                if (leftChildIndex > lastIndex) break; // 자식이 없음

                int rightChildIndex = leftChildIndex + 1;
                int minChildIndex = leftChildIndex;

                // 오른쪽 자식이 존재하고 왼쪽 자식보다 작으면 오른쪽 자식 선택
                if (rightChildIndex <= lastIndex &&
                    _elements[rightChildIndex].Priority.CompareTo(_elements[leftChildIndex].Priority) < 0)
                {
                    minChildIndex = rightChildIndex;
                }

                // 부모가 가장 작은 자식보다 작거나 같으면 정렬 완료
                if (_elements[parentIndex].Priority.CompareTo(_elements[minChildIndex].Priority) <= 0)
                    break;

                // 스왑
                var tmp = _elements[parentIndex];
                _elements[parentIndex] = _elements[minChildIndex];
                _elements[minChildIndex] = tmp;

                parentIndex = minChildIndex;
            }

            return bestItem;
        }
    }
}