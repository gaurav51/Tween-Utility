using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System;
using System.Collections.Generic;

[AddComponentMenu("DoTween Utility/DoTween Utility (Single)")]
[Icon("d_PlayableDirector Icon")]
public partial class DoTween_Utility : MonoBehaviour
{
#if UNITY_EDITOR
    // Safely bridge editor preview logic from runtime events
    public static Action<Tween> OnEditorTweenPlay;
#endif


    // Global Sequence Settings
    public int sequenceLoops = 1; // -1 for infinite
    public LoopType sequenceLoopType = LoopType.Restart;
    public bool ignoreTimeScale = true;

    // Array of tween steps
    public List<TweenStep> tweenSteps = new List<TweenStep>();
    
    // Component References
    private RectTransform rectTransform;
    private Component graphicComponent;

    public bool isUI { get; private set; }

    public Sequence CurrentSequence { get; private set; }

    // Cache original state for Reset
    private bool hasSavedState = false;
    private Vector3 origLocalPos, origScale;
    private Vector2 origAnchoredPos;
    private Quaternion origLocalRot;
    private Color origColor;

    private void Awake()
    {
        InitializeComponents();
        if (Application.isPlaying) SaveState();

        if (Application.isPlaying) HandleTrigger(StartTrigger.OnAwake);
    }

    private void Start()
    {
        if (Application.isPlaying) HandleTrigger(StartTrigger.OnStart);
    }

    private void OnEnable()
    {
        if (Application.isPlaying) HandleTrigger(StartTrigger.OnEnable);
    }

    private void HandleTrigger(StartTrigger trigger)
    {
        if (tweenSteps == null) return;
        
        for (int i = 0; i < tweenSteps.Count; i++)
        {
            if (tweenSteps[i].startTrigger == trigger && !tweenSteps[i].joinWithPrevious)
            {
                // Play this step (it will also build sequences for any subsequent joined steps)
                PlayStepSequence(i);
            }
        }
    }

    public void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        isUI = rectTransform != null;

        if (isUI)
        {
            graphicComponent = GetComponent<Graphic>();
        }
        else
        {
            graphicComponent = GetComponent<SpriteRenderer>();
            if (graphicComponent == null) graphicComponent = GetComponent<Renderer>();
        }
    }

    public void SaveState()
    {
        InitializeComponents();
        origLocalPos = transform.localPosition;
        origScale = transform.localScale;
        origLocalRot = transform.localRotation;
        
        if (isUI && rectTransform != null)
        {
            origAnchoredPos = rectTransform.anchoredPosition;
        }

        if (graphicComponent != null)
        {
            if (graphicComponent is Graphic uiGraphic) origColor = uiGraphic.color;
            else if (graphicComponent is SpriteRenderer spriteRenderer) origColor = spriteRenderer.color;
            else if (graphicComponent is Renderer renderer && renderer.sharedMaterial != null)
            {
                if (renderer.sharedMaterial.HasProperty("_BaseColor")) origColor = renderer.sharedMaterial.GetColor("_BaseColor");
                else if (renderer.sharedMaterial.HasProperty("_Color")) origColor = renderer.sharedMaterial.color;
            }
        }

        hasSavedState = true;
    }

    public void ResetState()
    {
        if (!hasSavedState) return;

        StopTween();

        transform.localPosition = origLocalPos;
        transform.localScale = origScale;
        transform.localRotation = origLocalRot;

        if (isUI && rectTransform != null)
        {
            rectTransform.anchoredPosition = origAnchoredPos;
        }

        if (graphicComponent != null)
        {
            if (graphicComponent is Graphic uiGraphic) uiGraphic.color = origColor;
            else if (graphicComponent is SpriteRenderer spriteRenderer) spriteRenderer.color = origColor;
            else if (graphicComponent is Renderer renderer && renderer.sharedMaterial != null)
            {
                Material targetMat = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
                if (targetMat != null)
                {
                    if (targetMat.HasProperty("_BaseColor")) targetMat.SetColor("_BaseColor", origColor);
                    else if (targetMat.HasProperty("_Color")) targetMat.color = origColor;
                }
            }
        }
        
        if (graphicComponent != null && graphicComponent is Text textGraphic)
        {
            textGraphic.text = ""; // Avoid messy overlapping string resets
        }
    }

    private Tween CreateTweenForStep(TweenStep step)
    {
        return TweenBuilder.CreateTween(transform, rectTransform, graphicComponent, step);
    }

    // Play all steps regardless of trigger (useful for editor preview or bulk execution)
    public void PlayTween()
    {
        if (!hasSavedState) SaveState();

        InitializeComponents();
        StopTween();
        ResetState(); // Always start clean from initial state

        CurrentSequence = DOTween.Sequence();
        CurrentSequence.SetUpdate(ignoreTimeScale); // Allows to pause with TimeScale or ignore it

        if (sequenceLoops != 0 && sequenceLoops != 1)
        {
            CurrentSequence.SetLoops(sequenceLoops, sequenceLoopType);
        }

        if (tweenSteps == null || tweenSteps.Count == 0) return;

        for (int i = 0; i < tweenSteps.Count; i++)
        {
            AppendStepToSequence(CurrentSequence, tweenSteps[i]);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            OnEditorTweenPlay?.Invoke(CurrentSequence);
        }
#endif
    }

    // Play a specific step index. If subsequent steps have joinWithPrevious, they are played alongside it.
    public void PlayStepSequence(int startIndex)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) 
            Debug.Log($"[DoTween] PlayStepSequence triggered for Step Index {startIndex}!");
#endif
        if (!hasSavedState) SaveState();
        InitializeComponents();
        
        if (startIndex < 0 || startIndex >= tweenSteps.Count) return;

        Sequence stepSequence = DOTween.Sequence();
        stepSequence.SetUpdate(ignoreTimeScale);

        AppendStepToSequence(stepSequence, tweenSteps[startIndex]);

        // Automatically append any steps that are chained to this one using joinWithPrevious
        for (int i = startIndex + 1; i < tweenSteps.Count; i++)
        {
            if (tweenSteps[i].joinWithPrevious)
            {
                AppendStepToSequence(stepSequence, tweenSteps[i]);
            }
            else
            {
                break; // Stop when we hit a step that doesn't join
            }
        }
        
        tweenSteps[startIndex].activeTween = stepSequence;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            OnEditorTweenPlay?.Invoke(stepSequence);
        }
#endif
    }

    public void PlayStepIndex(int index)
    {
        PlayStepSequence(index);
    }

    private void AppendStepToSequence(Sequence sequence, TweenStep step)
    {
        Tween newTween = CreateTweenForStep(step);

        if (newTween != null)
        {
            int safeLoops = step.loops == -1 ? int.MaxValue / 2 : step.loops;
            if (safeLoops != 1)
            {
                newTween.SetEase(step.easeType).SetLoops(safeLoops, step.loopType);
            }
            else
            {
                newTween.SetEase(step.easeType);
            }
            
            newTween.SetDelay(step.delay);
            newTween.OnStart(() => {
#if UNITY_EDITOR
                if (!Application.isPlaying && step.onStepStart?.GetPersistentEventCount() > 0)
                    Debug.Log("[DoTween] Step Start fired! (If your event didn't trigger, click its dropdown and change 'Runtime Only' to 'Editor And Runtime')");
#endif
                if (Application.isPlaying && step.onStartAudioClip != null)
                {
                    AudioSource.PlayClipAtPoint(step.onStartAudioClip, transform.position);
                }
                
                step.onStepStart?.Invoke();
            });
            newTween.OnComplete(() => {
#if UNITY_EDITOR
                if (!Application.isPlaying && step.onStepComplete?.GetPersistentEventCount() > 0)
                    //Debug.Log("[DoTween] Step Complete fired! (If your event didn't trigger, click its dropdown and change 'Runtime Only' to 'Editor And Runtime')");
#endif
                step.onStepComplete?.Invoke();
            });

            if (step.joinWithPrevious)
            {
                sequence.Join(newTween);
            }
            else
            {
                sequence.Append(newTween);
            }
        }
    }

    public void StopTween()
    {
        if (CurrentSequence != null && CurrentSequence.IsActive())
        {
            CurrentSequence.Kill();
        }
    }

    public void StopStepIndex(int index)
    {
        if (tweenSteps == null || index < 0 || index >= tweenSteps.Count) return;
        
        if (tweenSteps[index].activeTween != null && tweenSteps[index].activeTween.IsActive())
        {
            tweenSteps[index].activeTween.Kill();
        }
    }

    public void StopAllTweens()
    {
        StopTween();
        
        if (tweenSteps != null)
        {
            foreach (var step in tweenSteps)
            {
                if (step.activeTween != null && step.activeTween.IsActive())
                {
                    step.activeTween.Kill();
                }
            }
        }
    }

    public void RewindTween()
    {
        if (CurrentSequence != null && CurrentSequence.IsActive())
        {
            CurrentSequence.Rewind();
        }
    }
}
