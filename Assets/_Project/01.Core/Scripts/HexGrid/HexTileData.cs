using System;
using System.Runtime.CompilerServices;

namespace Project.Core.HexGrid
{

    [Flags]
    public enum TileFlags : byte
    {
        None = 0,
        IsExplored = 1 << 0, // 시야 확보 여부
        IsTraversable = 1 << 1, // 이동 가능 지형
        HasSupplyLine = 1 << 2, // 보급선 연결 여부
        IsCorrupted = 1 << 3  // 오염/독기 상태
    }

    public readonly struct HexTileData : IEquatable<HexTileData>
    {
        public readonly HexCoordinates Coordinates;
        public readonly Faction Owner;
        public readonly TileFlags Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexTileData(HexCoordinates coords, Faction owner, TileFlags flags)
        {
            Coordinates = coords;
            Owner = owner;
            Flags = flags;
        }

        // 플래그 조회 편의 프로퍼티 (비트 연산 마스킹)
        public bool HasSupply => (Flags & TileFlags.HasSupplyLine) != 0;
        public bool IsTraversable => (Flags & TileFlags.IsTraversable) != 0;
        public bool IsExplored => (Flags & TileFlags.IsExplored) != 0;
        public bool IsCorrupted => (Flags & TileFlags.IsCorrupted) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexTileData WithFaction(Faction newOwner)
            => new HexTileData(Coordinates, newOwner, Flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexTileData WithFlags(TileFlags newFlags)
            => new HexTileData(Coordinates, Owner, newFlags);

        /// <summary>
        /// 보급선 플래그만 안전하게 토글하는 불변 헬퍼
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexTileData WithSupplyState(bool isSupplied)
        {
            var nextFlags = isSupplied
                ? (Flags | TileFlags.HasSupplyLine)
                : (Flags & ~TileFlags.HasSupplyLine);

            return new HexTileData(Coordinates, Owner, nextFlags);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HexTileData other) => Coordinates.Equals(other.Coordinates);
        public override bool Equals(object obj) => obj is HexTileData other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Coordinates.GetHashCode();
    }
}