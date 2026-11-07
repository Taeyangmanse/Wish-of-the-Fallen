using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Project.Core.HexGrid
{
    /// <summary>
    /// 입방 좌표계(Cube Coordinates) 기반 불변 육각 타일 좌표.
    /// 제약 조건: X + Y + Z = 0
    /// </summary>
    public readonly struct HexCoordinates : IEquatable<HexCoordinates>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        // 축 좌표계(Axial) 접근자 별칭 제공 (Q = X, R = Z)
        public int Q => X;
        public int R => Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexCoordinates(int x, int z)
        {
            X = x;
            Z = z;
            Y = -x - z;
        }

        // 내부 고속 생성용 (Y 계산 생략)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HexCoordinates(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // 6방향 정적 방향 벡터 테이블
        private static readonly HexCoordinates[] Directions = new HexCoordinates[]
        {
            new HexCoordinates(1, -1, 0),   // 0: 동 (E)
            new HexCoordinates(1, 0, -1),   // 1: 북동 (NE)
            new HexCoordinates(0, 1, -1),   // 2: 북서 (NW)
            new HexCoordinates(-1, 1, 0),  // 3: 서 (W)
            new HexCoordinates(-1, 0, 1),  // 4: 남서 (SW)
            new HexCoordinates(0, -1, 1)   // 5: 남동 (SE)
        };

        /// <summary>
        /// 인접한 6방향 단위 벡터를 반환합니다. (0~5)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HexCoordinates GetDirection(int directionIndex)
        {
            Debug.Assert(directionIndex >= 0 && directionIndex < 6, "방향 인덱스는 0 이상 5 이하이어야 합니다.");
            return Directions[directionIndex & 7]; // 테이블 범위 내 고속 참조
        }

        /// <summary>
        /// 지정한 방향의 인접 이웃 노드 좌표를 반환합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HexCoordinates GetNeighbor(int directionIndex)
        {
            return this + GetDirection(directionIndex);
        }

        /// <summary>
        /// 두 헥스 좌표 간의 맨해튼 타일 거리 (O(1))
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistanceTo(HexCoordinates target)
        {
            return (Math.Abs(X - target.X) + Math.Abs(Y - target.Y) + Math.Abs(Z - target.Z)) >> 1; // / 2 비트 시프트
        }

        #region 연산자 오버로딩 (핫패스 인라이닝 적용)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
            => new HexCoordinates(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b)
            => new HexCoordinates(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HexCoordinates operator *(HexCoordinates a, int scalar)
            => new HexCoordinates(a.X * scalar, a.Y * scalar, a.Z * scalar);

        #endregion

        #region 동등성 및 해시

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HexCoordinates other) => X == other.X && Z == other.Z;

        public override bool Equals(object obj) => obj is HexCoordinates other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(X, Z);

        public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.Equals(b);
        public static bool operator !=(HexCoordinates a, HexCoordinates b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y}, {Z})";

        #endregion
    }
}