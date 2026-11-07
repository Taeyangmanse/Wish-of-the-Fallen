using System;
using System.Collections.Generic;

namespace Project.Core.HexGrid
{
    public sealed class HexPathfinder
    {
        private readonly HexGridModel _gridModel;

        // 탐색 캐싱용 컬렉션 (GC 할당 원천 방지)
        // Unity 2022.2+ (.NET 6) 지원 PriorityQueue
        private readonly PriorityQueue<HexCoordinates, int> _frontier;
        private readonly Dictionary<HexCoordinates, int> _costSoFar;

        public HexPathfinder(HexGridModel gridModel, int initialCapacity = 256)
        {
            _gridModel = gridModel ?? throw new ArgumentNullException(nameof(gridModel));
            _frontier = new PriorityQueue<HexCoordinates, int>(initialCapacity);
            _costSoFar = new Dictionary<HexCoordinates, int>(initialCapacity);
        }

        /// <summary>
        /// A* 알고리즘을 사용해 최단 유효 경로 거리를 계산합니다. (경로 불가 시 -1 반환)
        /// 탐색 캐시를 재사용하여 런타임 GC 할당을 유발하지 않습니다.
        /// </summary>
        public int FindPathDistance(
            HexCoordinates start,
            HexCoordinates target,
            bool requireSupply = false,
            Faction targetFaction = Faction.DemonKing)
        {
            // 1. 방어적 가드
            if (!_gridModel.TryGetTile(target, out var targetTile) || !targetTile.IsTraversable)
                return -1;

            if (requireSupply && (!targetTile.HasSupply || targetTile.Owner != targetFaction))
                return -1;

            // 2. 캐시 초기화
            _frontier.Clear();
            _costSoFar.Clear();

            _frontier.Enqueue(start, 0);
            _costSoFar[start] = 0;

            // 3. A* 루프
            while (_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                // 도착
                if (current == target)
                {
                    return _costSoFar[current];
                }

                int newCost = _costSoFar[current] + 1; // 헥스 이동 비용

                for (int i = 0; i < 6; i++)
                {
                    var next = current.GetNeighbor(i);

                    // 1. 그리드 유효성 검사
                    if (!_gridModel.TryGetTile(next, out var nextTile))
                        continue;

                    // 2. 안개(Fog of War) 데이터 누수 방지 및 지형 통과 검사
                    bool isTraversableToPlayer = nextTile.IsExplored ? nextTile.IsTraversable : true;
                    if (!isTraversableToPlayer)
                        continue;

                    // 3. 수송대 보급망 검사
                    if (requireSupply && (!nextTile.HasSupply || nextTile.Owner != targetFaction))
                        continue;

                    // 4. 이중 룩업 방지 및 최단 거리 갱신
                    bool isNewOrBetter = !_costSoFar.TryGetValue(next, out int currentCost) || newCost < currentCost;

                    if (isNewOrBetter)
                    {
                        _costSoFar[next] = newCost;

                        // 휴리스틱 (F = G + H)
                        int priority = newCost + next.DistanceTo(target);
                        _frontier.Enqueue(next, priority);
                    }
                }
            }

            return -1;
        }
    }
}