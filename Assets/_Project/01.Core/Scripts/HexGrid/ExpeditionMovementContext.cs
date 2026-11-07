namespace Project.Core.HexGrid
{
    /// <summary>
    /// 원정대의 동적 스탯 및 버프를 주입하기 위한 컨텍스트
    /// </summary>
    public readonly struct ExpeditionMovementContext
    {
        public readonly Faction Faction;
        public readonly int CurrentAP;
        public readonly int CostAP;
        public readonly int MaxWildernessRange;
        public readonly int MaxSupplyRange;
        public readonly int AmbushChanceReduction; // 정찰병 특성 등 기습 확률 감소치 (%)

        public ExpeditionMovementContext(
            Faction faction,
            int currentAP,
            int costAP = 2,
            int maxWildernessRange = 4,
            int maxSupplyRange = 16,
            int ambushChanceReduction = 0)
        {
            Faction = faction;
            CurrentAP = currentAP;
            CostAP = costAP;
            MaxWildernessRange = maxWildernessRange;
            MaxSupplyRange = maxSupplyRange;
            AmbushChanceReduction = ambushChanceReduction;
        }
    }
}