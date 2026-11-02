public readonly struct ObstacleReading
{
    public bool HasObstacle { get; }
    public float Distance { get; }
    public float AngleDegrees { get; }

    public ObstacleReading(bool hasObstacle, float distance, float angleDegrees)
    {
        HasObstacle = hasObstacle;
        Distance = distance;
        AngleDegrees = angleDegrees;
    }

    public static ObstacleReading None => new ObstacleReading(false, float.PositiveInfinity, 0f);
}