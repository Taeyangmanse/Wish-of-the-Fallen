using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Project.Core.HexGrid
{
    public sealed class HexGridModel
    {
        private readonly Dictionary<HexCoordinates, HexTileData> _tiles;
        private readonly IReadOnlyDictionary<HexCoordinates, HexTileData> _readOnlyTiles;

        // 뷰 계층에서 구독할 타일 변경 이벤트
        public event Action<HexTileData> OnTileStateChanged;

        public HexGridModel(int initialCapacity = 1024)
        {
            _tiles = new Dictionary<HexCoordinates, HexTileData>(initialCapacity);
            _readOnlyTiles = _tiles;
        }

        // 외부 알고리즘 및 뷰 계층 노출용 (할당 0 byte)
        public IReadOnlyDictionary<HexCoordinates, HexTileData> Tiles => _readOnlyTiles;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTile(HexTileData tile)
        {
            _tiles[tile.Coordinates] = tile;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTile(HexCoordinates coords, out HexTileData tile)
        {
            return _tiles.TryGetValue(coords, out tile);
        }

        public bool CaptureTile(HexCoordinates coords, Faction newFaction)
        {
            if (_tiles.TryGetValue(coords, out var currentTile) && currentTile.Owner != newFaction)
            {
                var newTile = currentTile.WithFaction(newFaction);
                _tiles[coords] = newTile;
                OnTileStateChanged?.Invoke(newTile); 
                return true;
            }
            return false;
        }

        /// <summary>
        /// 보급선 상태를 일괄 갱신합니다. GC 할당 0 바이트 보장 및 상태 변경 노드만 선별 갱신.
        /// </summary>
        public void UpdateSupplyLines(HashSet<HexCoordinates> suppliedCoords)
        {
            if (suppliedCoords == null) throw new ArgumentNullException(nameof(suppliedCoords));

            // C# Dictionary 순회 중 값(Value) 변경은 에러를 발생시키지 않으므로 임시 List 복사 제거
            // struct Enumerator를 타므로 GC Alloc = 0 bytes
            foreach (var kvp in _tiles)
            {
                var coord = kvp.Key;
                var tile = kvp.Value;
                bool isSupplied = suppliedCoords.Contains(coord);

                // [더티 체킹] 상태가 실제로 바뀐 타일만 딕셔너리에 덮어쓰기
                if (tile.HasSupply != isSupplied)
                {
                    _tiles[coord] = tile.WithSupplyState(isSupplied);
                }
            }
        }
    }
}