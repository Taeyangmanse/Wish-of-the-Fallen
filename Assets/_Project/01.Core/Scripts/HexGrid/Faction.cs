// Faction.cs
namespace Project.Core.HexGrid
{
    /// <summary>
    /// 타일 및 유닛의 소유 세력 (메모리 최적화를 위해 1바이트 할당)
    /// </summary>
    public enum Faction : byte
    {
        Neutral = 0,
        DemonKing = 1,
        Empire = 2
    }
}