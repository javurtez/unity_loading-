namespace LoadingGame
{
    /// <summary>
    /// Color family of a shape slot. Stored on each LoadSlot prefab and used as an index into
    /// <see cref="VisualTheme"/> families, so lookups never depend on object names or references.
    /// </summary>
    public enum ShapeColorFamily
    {
        Rose = 0,
        Amber = 1,
        Mint = 2,
        Sky = 3,
        Violet = 4,
        Pearl = 5
    }

    /// <summary>Entry animation played by a grade label.</summary>
    public enum GradeAnimation
    {
        PunchWobble,
        PopBack,
        PopQuad,
        Fade,
        FadeShake
    }
}
