using UnityEngine;
using UnityEditor;
using DG.DOTweenEditor;

[CustomEditor(typeof(DoTween_Utility))]
public class DoTween_UtilityEditor : Editor
{
    SerializedProperty sequenceLoopsProp;
    SerializedProperty sequenceLoopTypeProp;
    SerializedProperty sequenceIgnoreTimeScaleProp;
    SerializedProperty tweenStepsProp;

    private static TweenStep clipboardStep; // Static clipboard across instances

    private void OnEnable()
    {
        sequenceLoopsProp = serializedObject.FindProperty("sequenceLoops");
        sequenceLoopTypeProp = serializedObject.FindProperty("sequenceLoopType");
        sequenceIgnoreTimeScaleProp = serializedObject.FindProperty("ignoreTimeScale");
        tweenStepsProp = serializedObject.FindProperty("tweenSteps");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DoTween_Utility utility = (DoTween_Utility)target;

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.25f, 0.85f, 0.45f) } // Bright green DOTween aesthetic
        };

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("DOTWEEN UTILITY: SINGLE OBJECT", titleStyle, GUILayout.Height(25));
        EditorGUILayout.Space(5);

        // Auto initialization if needed
        utility.InitializeComponents();

        EditorGUILayout.LabelField("Tween Target (Auto-Detected)", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("Obj Type:", utility.isUI ? "UI (RectTransform)" : "Transform (Normal Object)", EditorStyles.helpBox);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Global Sequence Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(sequenceIgnoreTimeScaleProp, new GUIContent("Ignore Time Scale (Unscaled Time)"));
        EditorGUILayout.PropertyField(sequenceLoopsProp, new GUIContent("Global Loops (-1 for Inf)"));
        if (sequenceLoopsProp.intValue != 0 && sequenceLoopsProp.intValue != 1)
        {
            EditorGUILayout.PropertyField(sequenceLoopTypeProp, new GUIContent("Global Loop Type"));
        }

        EditorGUILayout.Space();

        // Draw Tween Steps manually for tailored dynamic properties
        EditorGUILayout.LabelField("Tween Steps Sequence", EditorStyles.boldLabel);
        
        for (int i = 0; i < tweenStepsProp.arraySize; i++)
        {
            SerializedProperty stepProp = tweenStepsProp.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            stepProp.isExpanded = EditorGUILayout.Foldout(stepProp.isExpanded, "Step " + (i + 1), true);
            
            if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
            {
                tweenStepsProp.MoveArrayElement(i, i - 1);
                break; // Break and repaint
            }
            if (GUILayout.Button("↓", GUILayout.Width(25)) && i < tweenStepsProp.arraySize - 1)
            {
                tweenStepsProp.MoveArrayElement(i, i + 1);
                break;
            }
            if (GUILayout.Button(new GUIContent("C", "Copy Step"), GUILayout.Width(25)))
            {
                clipboardStep = utility.tweenSteps[i].Clone();
            }
            if (GUILayout.Button(new GUIContent("P", "Paste Step"), GUILayout.Width(25)))
            {
                if (clipboardStep != null)
                {
                    Undo.RecordObject(utility, "Paste Tween Step");
                    utility.tweenSteps[i] = clipboardStep.Clone();
                    EditorUtility.SetDirty(utility);
                }
            }
            
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                tweenStepsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (stepProp.isExpanded)
            {
                SerializedProperty attrProp = stepProp.FindPropertyRelative("tweenAttribute");
                EditorGUILayout.PropertyField(attrProp);
                
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("startTrigger"));
                
                if (i > 0)
                {
                    EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("joinWithPrevious"), new GUIContent("Play With Previous (Parallel)"));
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Settings", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("easeType"));
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("delay"));
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("isRelative"));
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("isSpeedBased"));
                EditorGUILayout.EndHorizontal();
                
                SerializedProperty loopsProp = stepProp.FindPropertyRelative("loops");
                EditorGUILayout.PropertyField(loopsProp, new GUIContent("Step Loops"));
                if (loopsProp.intValue != 0 && loopsProp.intValue != 1)
                {
                    EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("loopType"));
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Target Values", EditorStyles.miniBoldLabel);
                DoTween_Utility.TweenAttribute currentAttr = (DoTween_Utility.TweenAttribute)attrProp.enumValueIndex;
                switch (currentAttr)
                {
                    case DoTween_Utility.TweenAttribute.Position:
                    case DoTween_Utility.TweenAttribute.LocalPosition:
                    case DoTween_Utility.TweenAttribute.AnchoredPosition:
                    case DoTween_Utility.TweenAttribute.Rotation:
                    case DoTween_Utility.TweenAttribute.LocalRotation:
                    case DoTween_Utility.TweenAttribute.Scale:
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetVectorValue"), new GUIContent("Target Vector"));
                        if (GUILayout.Button("Get Current", GUILayout.Width(85)))
                        {
                            Undo.RecordObject(utility, "Capture Current Transform");
                            if (currentAttr == DoTween_Utility.TweenAttribute.Position) utility.tweenSteps[i].targetVectorValue = utility.transform.position;
                            else if (currentAttr == DoTween_Utility.TweenAttribute.LocalPosition) utility.tweenSteps[i].targetVectorValue = utility.transform.localPosition;
                            else if (currentAttr == DoTween_Utility.TweenAttribute.AnchoredPosition) utility.tweenSteps[i].targetVectorValue = utility.GetComponent<RectTransform>() != null ? utility.GetComponent<RectTransform>().anchoredPosition : Vector2.zero;
                            else if (currentAttr == DoTween_Utility.TweenAttribute.Rotation) utility.tweenSteps[i].targetVectorValue = utility.transform.eulerAngles;
                            else if (currentAttr == DoTween_Utility.TweenAttribute.LocalRotation) utility.tweenSteps[i].targetVectorValue = utility.transform.localEulerAngles;
                            else if (currentAttr == DoTween_Utility.TweenAttribute.Scale) utility.tweenSteps[i].targetVectorValue = utility.transform.localScale;
                            EditorUtility.SetDirty(utility);
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    case DoTween_Utility.TweenAttribute.Color:
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetColorValue"), new GUIContent("Target Color"));
                        break;
                    case DoTween_Utility.TweenAttribute.Fade:
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetFloatValue"), new GUIContent("Target Alpha (Float)"));
                        break;
                    case DoTween_Utility.TweenAttribute.ShakePosition:
                    case DoTween_Utility.TweenAttribute.ShakeRotation:
                    case DoTween_Utility.TweenAttribute.ShakeScale:
                    case DoTween_Utility.TweenAttribute.PunchPosition:
                    case DoTween_Utility.TweenAttribute.PunchRotation:
                    case DoTween_Utility.TweenAttribute.PunchScale:
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetVectorValue"), new GUIContent("Effect Direction/Force"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("shakeStrength"), new GUIContent("Strength Multiplier"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("vibrato"));
                        if (currentAttr.ToString().Contains("Punch"))
                            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("punchElasticity"), new GUIContent("Elasticity"));
                        else
                            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("shakeRandomness"), new GUIContent("Randomness"));
                        break;
                    case DoTween_Utility.TweenAttribute.Text:
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetStringValue"), new GUIContent("Target Text"));
                        break;
                    case DoTween_Utility.TweenAttribute.ImageFill:
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("targetFloatValue"), new GUIContent("Fill Amount [0-1]"));
                        break;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Action & Audio Callbacks", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStartAudioClip"));
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStepStart"));
                EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStepComplete"));
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Test Play This Step", GUILayout.Height(20)))
                {
                    if (!Application.isPlaying)
                    {
                        DOTweenEditorPreview.Stop(true);
                        utility.SaveState();
                    }
                    
                    // This securely builds the tween AND notifies the Editor delegate to play it.
                    utility.PlayStepIndex(i);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        if (GUILayout.Button("＋ Add New Step", GUILayout.Height(30)))
        {
            int index = tweenStepsProp.arraySize;
            tweenStepsProp.InsertArrayElementAtIndex(index);
            // Default expansion
            var newProp = tweenStepsProp.GetArrayElementAtIndex(index);
            newProp.isExpanded = true;
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Master Editor Controls", EditorStyles.boldLabel);

        // Buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("▶ Play Sequence", GUILayout.Height(30)))
        {
            if (!Application.isPlaying)
            {
                // Force stop any existing preview to cleanly setup the next run
                DOTweenEditorPreview.Stop(true); 
                // Capture the current inspector values as the fresh start state
                utility.SaveState(); 
            }
            
            utility.PlayTween();
            
            if (!Application.isPlaying && utility.CurrentSequence != null)
            {
                DOTweenEditorPreview.PrepareTweenForPreview(utility.CurrentSequence);
                DOTweenEditorPreview.Start();
            }
        }
        if (GUILayout.Button("■ Stop", GUILayout.Height(30)))
        {
            if (!Application.isPlaying)
            {
                DOTweenEditorPreview.Stop(true);
            }
            utility.StopTween();
            utility.ResetState();
        }
        if (GUILayout.Button("Rewind"))
        {
            if (!Application.isPlaying)
            {
                DOTweenEditorPreview.Stop(true);
            }
            utility.RewindTween();
            utility.ResetState();
            if (!Application.isPlaying) SceneView.RepaintAll();
        }
        if (GUILayout.Button("Reset State"))
        {
            utility.ResetState();
            if (!Application.isPlaying) SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
    }
}

[InitializeOnLoad]
public static class DoTweenEditorPreviewBridge
{
    static DoTweenEditorPreviewBridge()
    {
        DoTween_Utility.OnEditorTweenPlay -= HandleEditorTweenPlay;
        DoTween_Utility.OnEditorTweenPlay += HandleEditorTweenPlay;
    }

    private static System.Collections.Generic.List<DG.Tweening.Tween> pendingTweens = new System.Collections.Generic.List<DG.Tweening.Tween>();

    private static void HandleEditorTweenPlay(DG.Tweening.Tween tween)
    {
        if (!Application.isPlaying && tween != null)
        {
            if (pendingTweens.Count == 0)
            {
                EditorApplication.update += StartPendingTweens;
            }
            pendingTweens.Add(tween);
        }
    }

    private static void StartPendingTweens()
    {
        EditorApplication.update -= StartPendingTweens;
        
        var toStart = new System.Collections.Generic.List<DG.Tweening.Tween>(pendingTweens);
        pendingTweens.Clear();
        
        foreach (var t in toStart)
        {
            DG.DOTweenEditor.DOTweenEditorPreview.PrepareTweenForPreview(t);
        }
        DG.DOTweenEditor.DOTweenEditorPreview.Start();
    }
}
