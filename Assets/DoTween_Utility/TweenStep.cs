using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

[Serializable]
public class TweenStep
{
    public DoTween_Utility.TweenAttribute tweenAttribute = DoTween_Utility.TweenAttribute.Position;
    public Ease easeType = Ease.OutQuad;
    public float duration = 1f;
    public float delay = 0f;
    
    public int loops = 1;
    public LoopType loopType = LoopType.Restart;
    
    public DoTween_Utility.StartTrigger startTrigger = DoTween_Utility.StartTrigger.OnStart;
    
    public bool joinWithPrevious = false; // Join with previous step instead of appending
    
    public bool isRelative = false; 
    public bool isSpeedBased = false;

    // Values
    public Vector3 targetVectorValue;
    public Color targetColorValue = Color.white;
    public float targetFloatValue = 1f;
    public string targetStringValue = "";
    
    // Shake & Punch settings
    public float shakeStrength = 1f;
    public int vibrato = 10;
    public float shakeRandomness = 90f;
    [Range(0f, 1f)] public float punchElasticity = 1f;

    // Audio
    public AudioClip onStartAudioClip;

    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;

    // Runtime reference
    [HideInInspector] public Tween activeTween;

    public TweenStep Clone()
    {
        return (TweenStep)this.MemberwiseClone();
    }
}
