using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class TweenBuilder
{
    // Adheres to Single Responsibility Principle - Sole job is constructing the specific Tween object.
    public static Tween CreateTween(Transform targetT, RectTransform rectTransform, Component graphicComponent, TweenStep step)
    {
        Tween newTween = null;

        switch (step.tweenAttribute)
        {
            case DoTween_Utility.TweenAttribute.Position: newTween = targetT.DOMove(step.targetVectorValue, step.duration); break;
            case DoTween_Utility.TweenAttribute.LocalPosition: newTween = targetT.DOLocalMove(step.targetVectorValue, step.duration); break;
            case DoTween_Utility.TweenAttribute.AnchoredPosition: 
                if (rectTransform != null) newTween = rectTransform.DOAnchorPos(step.targetVectorValue, step.duration); 
                break;
            case DoTween_Utility.TweenAttribute.Rotation: newTween = targetT.DORotate(step.targetVectorValue, step.duration); break;
            case DoTween_Utility.TweenAttribute.LocalRotation: newTween = targetT.DOLocalRotate(step.targetVectorValue, step.duration); break;
            case DoTween_Utility.TweenAttribute.Scale: newTween = targetT.DOScale(step.targetVectorValue, step.duration); break;
            case DoTween_Utility.TweenAttribute.Color:
                if (graphicComponent != null)
                {
                    if (graphicComponent is Graphic uiGraphic) newTween = uiGraphic.DOColor(step.targetColorValue, step.duration);
                    else if (graphicComponent is SpriteRenderer spriteRenderer) newTween = spriteRenderer.DOColor(step.targetColorValue, step.duration);
                    else if (graphicComponent is Renderer renderer) 
                    {
                        Material targetMat = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
                        if (targetMat != null)
                        {
                            if (targetMat.HasProperty("_BaseColor")) newTween = targetMat.DOColor(step.targetColorValue, "_BaseColor", step.duration);
                            else if (targetMat.HasProperty("_Color")) newTween = targetMat.DOColor(step.targetColorValue, "_Color", step.duration);
                        }
                    }
                }
                break;
            case DoTween_Utility.TweenAttribute.Fade:
                if (graphicComponent != null)
                {
                    if (graphicComponent is Graphic graphicFade) newTween = graphicFade.DOFade(step.targetFloatValue, step.duration);
                    else if (graphicComponent is SpriteRenderer spriteFade) newTween = spriteFade.DOFade(step.targetFloatValue, step.duration);
                    else if (graphicComponent is Renderer rendererFade) 
                    {
                        Material targetMat = Application.isPlaying ? rendererFade.material : rendererFade.sharedMaterial;
                        if (targetMat != null)
                        {
                            if (targetMat.HasProperty("_BaseColor")) newTween = targetMat.DOFade(step.targetFloatValue, "_BaseColor", step.duration);
                            else if (targetMat.HasProperty("_Color")) newTween = targetMat.DOFade(step.targetFloatValue, "_Color", step.duration);
                        }
                    }
                }
                break;
            case DoTween_Utility.TweenAttribute.ShakePosition: newTween = targetT.DOShakePosition(step.duration, step.targetVectorValue.magnitude * step.shakeStrength, step.vibrato, step.shakeRandomness); break;
            case DoTween_Utility.TweenAttribute.ShakeRotation: newTween = targetT.DOShakeRotation(step.duration, step.targetVectorValue.magnitude * step.shakeStrength, step.vibrato, step.shakeRandomness); break;
            case DoTween_Utility.TweenAttribute.ShakeScale: newTween = targetT.DOShakeScale(step.duration, step.targetVectorValue.magnitude * step.shakeStrength, step.vibrato, step.shakeRandomness); break;
            
            case DoTween_Utility.TweenAttribute.PunchPosition: newTween = targetT.DOPunchPosition(step.targetVectorValue * step.shakeStrength, step.duration, step.vibrato, step.punchElasticity); break;
            case DoTween_Utility.TweenAttribute.PunchRotation: newTween = targetT.DOPunchRotation(step.targetVectorValue * step.shakeStrength, step.duration, step.vibrato, step.punchElasticity); break;
            case DoTween_Utility.TweenAttribute.PunchScale: newTween = targetT.DOPunchScale(step.targetVectorValue * step.shakeStrength, step.duration, step.vibrato, step.punchElasticity); break;
            
            case DoTween_Utility.TweenAttribute.Text:
                if (graphicComponent != null && graphicComponent is Text textGraphic) 
                    newTween = textGraphic.DOText(step.targetStringValue, step.duration);
                break;
            case DoTween_Utility.TweenAttribute.ImageFill:
                if (graphicComponent != null && graphicComponent is Image imageGraphic) 
                    newTween = imageGraphic.DOFillAmount(step.targetFloatValue, step.duration);
                break;
        }
        
        if (newTween != null)
        {
            if (step.isRelative) newTween.SetRelative(true);
            if (step.isSpeedBased) newTween.SetSpeedBased(true);
        }
        
        return newTween;
    }
}
