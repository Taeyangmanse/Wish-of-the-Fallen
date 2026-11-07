using System;

namespace Project.Core.HexGrid
{
    public class HexNode
    {
        public HexCoordinates Coordinates { get; }
        public Faction CurrentFaction { get; set; }
        public bool IsSupplyConnected { get; set; }

        public HexNode(HexCoordinates coordinates, Faction faction = Faction.Neutral)
        {
            Coordinates = coordinates;
            CurrentFaction = faction;
            IsSupplyConnected = false;
        }
    }
}