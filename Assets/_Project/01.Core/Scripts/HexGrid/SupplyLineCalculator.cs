using System;
using System.Collections.Generic;

namespace Project.Core.HexGrid
{
    public sealed class SupplyLineCalculator
    {
        private readonly Queue<HexCoordinates> _bfsQueue;
        private readonly HashSet<HexCoordinates> _connectedSet;

        public SupplyLineCalculator(int initialCapacity = 256)
        {
            _bfsQueue = new Queue<HexCoordinates>(initialCapacity);
            _connectedSet = new HashSet<HexCoordinates>(initialCapacity);
        }

        /// <summary>
        /// 지정한 루트 노드로부터 대상 팩션의 연결된 보급 좌표 집합을 고속 계산합니다.
        /// 반환된 HashSet은 인스턴스 캐시이므로 다음 계산 전까지만 유효합니다.
        /// </summary>
        public HashSet<HexCoordinates> CalculateSupplyLines(
            HexCoordinates rootCoordinate,
            Faction targetFaction,
            IReadOnlyDictionary<HexCoordinates, HexNode> grid)
        {
            // 1. 방어적 가드 클로저
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            _bfsQueue.Clear();
            _connectedSet.Clear();

            // 2. 루트 노드 유효성 및 팩션 점령 검사
            if (!grid.TryGetValue(rootCoordinate, out var rootNode) || rootNode.CurrentFaction != targetFaction)
            {
                return _connectedSet;
            }

            // 3. 루트 노드 초기화
            _bfsQueue.Enqueue(rootCoordinate);
            _connectedSet.Add(rootCoordinate);

            // 4. BFS 순회
            while (_bfsQueue.Count > 0)
            {
                var currentCoords = _bfsQueue.Dequeue();

                for (int i = 0; i < 6; i++)
                {
                    var neighborCoords = currentCoords.GetNeighbor(i);

                    // grid에 존재하며, 팩션이 일치하고, HashSet.Add 성공 시에만 큐 삽입 (해시 룩업 1회 단축)
                    if (grid.TryGetValue(neighborCoords, out var neighborNode) &&
                        neighborNode.CurrentFaction == targetFaction &&
                        _connectedSet.Add(neighborCoords))
                    {
                        _bfsQueue.Enqueue(neighborCoords);
                    }
                }
            }

            return _connectedSet;
        }

        /// <summary>
        /// 계산된 보급망 결과를 실제 노드 데이터 모델에 일괄 동기화합니다.
        /// </summary>
        public void SynchronizeSupplyState(
            HashSet<HexCoordinates> connectedCoordinates,
            IEnumerable<HexNode> allNodes)
        {
            if (connectedCoordinates == null || allNodes == null) return;

            foreach (var node in allNodes)
            {
                node.IsSupplyConnected = connectedCoordinates.Contains(node.Coordinates);
            }
        }
    }
}