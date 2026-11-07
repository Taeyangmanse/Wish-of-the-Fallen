namespace Project.Core.HexGrid
{
    public enum MovementFailureReason : byte
    {
        None = 0,
        NotEnoughAP,
        InvalidStartTile,
        TargetNotTraversable,
        OutOfRange
    }
}