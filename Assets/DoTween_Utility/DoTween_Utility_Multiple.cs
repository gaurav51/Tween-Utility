using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

[Serializable]
public class TweenObjectGroup
{
    public string groupName = "Target Group";
    public GameObject targetObject;
    
    public int sequenceLoops = 1;
    public LoopType sequenceLoopType = LoopType.Restart;
    public bool ignoreTimeScale = true;
    
    public DoTween_Utility.StartTrigger startTrigger = DoTween_Utility.StartTrigger.OnStart;
    
    public List<TweenStep> tweenSteps = new List<TweenStep>();
    
    [HideInInspector] public Sequence currentSequence;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Component graphicComponent;

    public void InitializeComponents()
    {
        if (targetObject == null) return;
        rectTransform = targetObject.GetComponent<RectTransform>();
        graphicComponent = targetObject.GetComponent<Graphic>();
        if (graphicComponent == null) graphicComponent = targetObject.GetComponent<SpriteRenderer>();
    }
}

[AddComponentMenu("DoTween Utility/DoTween Utility (Multiple)")]
[Icon("d_Animator Icon")]
public class DoTween_Utility_Multiple : MonoBehaviour
{
    public List<TweenObjectGroup> tweenGroups = new List<TweenObjectGroup>();

    private void Awake()
    {
        InitializeAllComponents();
        PlayGroupsByTrigger(DoTween_Utility.StartTrigger.OnAwake);
    }

    private void Start()
    {
        PlayGroupsByTrigger(DoTween_Utility.StartTrigger.OnStart);
    }

    private void OnEnable()
    {
        PlayGroupsByTrigger(DoTween_Utility.StartTrigger.OnEnable);
    }
    
    public void InitializeAllComponents()
    {
        foreach(var group in tweenGroups) group.InitializeComponents();
    }

    public void PlayGroupsByTrigger(DoTween_Utility.StartTrigger trigger)
    {
        for (int i = 0; i < tweenGroups.Count; i++)
        {
            if (tweenGroups[i].startTrigger == trigger)
                PlayGroupIndex(i);
        }
    }

    public void PlayGroupIndex(int index)
    {
        if (index < 0 || index >= tweenGroups.Count) return;
        var group = tweenGroups[index];
        if (group.targetObject == null || group.tweenSteps.Count == 0) return;
        
        group.InitializeComponents();
        ResetGroupState(group);

        group.currentSequence = DOTween.Sequence();
        group.currentSequence.SetUpdate(group.ignoreTimeScale);

        if (group.sequenceLoops != 0 && group.sequenceLoops != 1)
        {
            group.currentSequence.SetLoops(group.sequenceLoops, group.sequenceLoopType);
        }

        for (int i = 0; i < group.tweenSteps.Count; i++)
        {
            AppendStepToSequence(group, group.currentSequence, group.tweenSteps[i]);
        }
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DoTween_Utility.OnEditorTweenPlay?.Invoke(group.currentSequence);
        }
#endif
    }
    
    public void StopGroupIndex(int index)
    {
        if (index < 0 || index >= tweenGroups.Count) return;
        var group = tweenGroups[index];
        if (group.currentSequence != null && group.currentSequence.IsActive())
        {
            group.currentSequence.Kill();
        }
        ResetGroupState(group);
    }
    
    public void StopAllGroups()
    {
        for (int i = 0; i < tweenGroups.Count; i++) StopGroupIndex(i);
    }
    
    public void RewindGroupIndex(int index)
    {
        if (index < 0 || index >= tweenGroups.Count) return;
        var group = tweenGroups[index];
        if (group.currentSequence != null && group.currentSequence.IsActive())
        {
            group.currentSequence.Rewind();
            group.currentSequence.Kill();
        }
        ResetGroupState(group);
    }

    private void AppendStepToSequence(TweenObjectGroup group, Sequence seq, TweenStep step)
    {
        Tween tween = CreateTweenForStep(group, step);
        if (tween == null) return;

        tween.SetEase(step.easeType);
        
        int safeLoops = step.loops == 0 ? 1 : step.loops;
        if (safeLoops != 1) tween.SetLoops(safeLoops, step.loopType);

        tween.OnStart(() => {
            if (Application.isPlaying && step.onStartAudioClip != null)
                AudioSource.PlayClipAtPoint(step.onStartAudioClip, group.targetObject.transform.position);
            
            step.onStepStart?.Invoke();
        });
        
        tween.OnComplete(() => {
            step.onStepComplete?.Invoke();
        });

        if (step.joinWithPrevious) seq.Join(tween);
        else seq.Append(tween);
        
        if (step.delay > 0) tween.SetDelay(step.delay);
    }

    private Tween CreateTweenForStep(TweenObjectGroup group, TweenStep step)
    {
        if (group.targetObject == null) return null;
        return TweenBuilder.CreateTween(group.targetObject.transform, group.rectTransform, group.graphicComponent, step);
    }

    public void ResetGroupState(TweenObjectGroup group)
    {
        if (group.targetObject == null) return;
        DOTween.Kill(group.targetObject.transform);
        if (group.graphicComponent != null) DOTween.Kill(group.graphicComponent);
    }
}
