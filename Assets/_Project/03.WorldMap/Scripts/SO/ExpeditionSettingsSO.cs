using UnityEngine;
using Project.Core.HexGrid; // Core 어셈블리 참조

namespace Project.WorldMap.Data
{
    [CreateAssetMenu(fileName = "ExpeditionSettings", menuName = "WishOfTheFallen/Expedition Settings")]
    public class ExpeditionSettingsSO : ScriptableObject
    {
        [Header("이동 및 보급 규칙")]
        public int BaseCostAP = 2;
        public int MaxWildernessRange = 4;
        public int MaxSupplyRange = 16;

        [Header("정찰 및 기습")]
        [Range(0, 50)] public int AmbushChanceReduction = 0;

        /// <summary>
        /// Unity 엔진 데이터(SO)를 순수 C# Core 계층의 Context DTO로 변환하여 반환합니다.
        /// </summary>
        public ExpeditionMovementContext CreateContext(Faction currentFaction, int currentAP)
        {
            return new ExpeditionMovementContext(
                faction: currentFaction,
                currentAP: currentAP,
                costAP: BaseCostAP,
                maxWildernessRange: MaxWildernessRange,
                maxSupplyRange: MaxSupplyRange,
                ambushChanceReduction: AmbushChanceReduction
            );
        }
    }
}