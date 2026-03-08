using UnityEngine;

public partial class DoTween_Utility 
{
    public enum TweenAttribute
    {
        Position,
        LocalPosition,
        AnchoredPosition,
        Rotation,
        LocalRotation,
        Scale,
        Color,
        Fade,
        ShakePosition,
        ShakeRotation,
        ShakeScale,
        PunchPosition,
        PunchRotation,
        PunchScale,
        Text,
        ImageFill
    }

    public enum StartTrigger
    {
        OnAwake,
        OnStart,
        OnEnable,
        Custom
    }
}
