using UnityEngine;
using UnityEditor;
using DG.DOTweenEditor;
using DG.Tweening;

[CustomEditor(typeof(DoTween_Utility_Multiple))]
public class DoTween_Utility_MultipleEditor : Editor
{
    SerializedProperty tweenGroupsProp;

    private static TweenStep clipboardStep; // Static clipboard across instances
    private int selectedGroupIndex = 0;

    private void OnEnable()
    {
        tweenGroupsProp = serializedObject.FindProperty("tweenGroups");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DoTween_Utility_Multiple utility = (DoTween_Utility_Multiple)target;
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.25f, 0.85f, 0.45f) } // Bright green DOTween aesthetic
        };

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("DOTWEEN UTILITY: MULTI-OBJECT", titleStyle, GUILayout.Height(25));
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Assign multiple GameObjects below to control entirely different Dotween sequence groups from a single script.", MessageType.Info);
        EditorGUILayout.Space(2);

        if (GUILayout.Button("＋ Add New Object Group", GUILayout.Height(30)))
        {
            tweenGroupsProp.InsertArrayElementAtIndex(tweenGroupsProp.arraySize);
            var newGroup = tweenGroupsProp.GetArrayElementAtIndex(tweenGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("groupName").stringValue = "New Target Group";
            newGroup.isExpanded = true;
        }

        EditorGUILayout.Space();

        if (tweenGroupsProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No Object Groups exist yet. Click 'Add New Object Group' to begin.", MessageType.Warning);
            selectedGroupIndex = -1;
        }
        else
        {
            // Clamp selected index
            if (selectedGroupIndex >= tweenGroupsProp.arraySize) selectedGroupIndex = tweenGroupsProp.arraySize - 1;
            if (selectedGroupIndex < 0) selectedGroupIndex = 0;

            // Build Tab Names
            string[] tabs = new string[tweenGroupsProp.arraySize];
            for (int i = 0; i < tweenGroupsProp.arraySize; i++)
            {
                tabs[i] = tweenGroupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("groupName").stringValue;
            }

            // Draw Tabs
            selectedGroupIndex = GUILayout.SelectionGrid(selectedGroupIndex, tabs, Mathf.Min(tweenGroupsProp.arraySize, 3));
            EditorGUILayout.Space(5);

            int g = selectedGroupIndex;
            SerializedProperty groupProp = tweenGroupsProp.GetArrayElementAtIndex(g);
            
            EditorGUILayout.BeginVertical("window");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("EDITING GROUP: " + tabs[g], EditorStyles.boldLabel);
            if (GUILayout.Button("Delete Group", GUILayout.Width(100)))
            {
                tweenGroupsProp.DeleteArrayElementAtIndex(g);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("groupName"));
            EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("targetObject"));
            EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("startTrigger"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Group Sequence Settings", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("ignoreTimeScale"));
            EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("sequenceLoops"));
            SerializedProperty loopsPropGrp = groupProp.FindPropertyRelative("sequenceLoops");
            if (loopsPropGrp.intValue != 0 && loopsPropGrp.intValue != 1)
            {
                EditorGUILayout.PropertyField(groupProp.FindPropertyRelative("sequenceLoopType"));
            }
            
            SerializedProperty stepsProp = groupProp.FindPropertyRelative("tweenSteps");
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Steps On Object", EditorStyles.boldLabel);
            
            for (int i = 0; i < stepsProp.arraySize; i++)
            {
                SerializedProperty stepProp = stepsProp.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                stepProp.isExpanded = EditorGUILayout.Foldout(stepProp.isExpanded, "Step " + (i + 1), true);
                
                if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
                {
                    stepsProp.MoveArrayElement(i, i - 1);
                    break;
                }
                if (GUILayout.Button("↓", GUILayout.Width(25)) && i < stepsProp.arraySize - 1)
                {
                    stepsProp.MoveArrayElement(i, i + 1);
                    break;
                }
                if (GUILayout.Button(new GUIContent("C", "Copy Step"), GUILayout.Width(25)))
                {
                    clipboardStep = utility.tweenGroups[g].tweenSteps[i].Clone();
                }
                if (GUILayout.Button(new GUIContent("P", "Paste Step"), GUILayout.Width(25)))
                {
                    if (clipboardStep != null)
                    {
                        Undo.RecordObject(utility, "Paste Tween Step");
                        utility.tweenGroups[g].tweenSteps[i] = clipboardStep.Clone();
                        EditorUtility.SetDirty(utility);
                    }
                }
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    stepsProp.DeleteArrayElementAtIndex(i);
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
                    EditorGUILayout.LabelField("Values", EditorStyles.miniBoldLabel);
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
                                if (utility.tweenGroups[g].targetObject != null)
                                {
                                    Undo.RecordObject(utility, "Capture Current Transform");
                                    Transform t = utility.tweenGroups[g].targetObject.transform;
                                    if (currentAttr == DoTween_Utility.TweenAttribute.Position) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = t.position;
                                    else if (currentAttr == DoTween_Utility.TweenAttribute.LocalPosition) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = t.localPosition;
                                    else if (currentAttr == DoTween_Utility.TweenAttribute.AnchoredPosition) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = utility.tweenGroups[g].targetObject.GetComponent<RectTransform>() != null ? utility.tweenGroups[g].targetObject.GetComponent<RectTransform>().anchoredPosition : Vector2.zero;
                                    else if (currentAttr == DoTween_Utility.TweenAttribute.Rotation) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = t.eulerAngles;
                                    else if (currentAttr == DoTween_Utility.TweenAttribute.LocalRotation) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = t.localEulerAngles;
                                    else if (currentAttr == DoTween_Utility.TweenAttribute.Scale) utility.tweenGroups[g].tweenSteps[i].targetVectorValue = t.localScale;
                                    EditorUtility.SetDirty(utility);
                                }
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
                    EditorGUILayout.LabelField("Callbacks", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStartAudioClip"));
                    EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStepStart"));
                    EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("onStepComplete"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("＋ Add New Step To " + groupProp.FindPropertyRelative("groupName").stringValue, GUILayout.Height(25)))
            {
                int index = stepsProp.arraySize;
                stepsProp.InsertArrayElementAtIndex(index);
                stepsProp.GetArrayElementAtIndex(index).isExpanded = true;
            }
            
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶ Play Group Preview", GUILayout.Height(25)))
            {
                if (!Application.isPlaying) DOTweenEditorPreview.Stop(true);
                utility.PlayGroupIndex(g);
            }
            if (GUILayout.Button("■ Stop Group", GUILayout.Height(25)))
            {
                if (!Application.isPlaying) DOTweenEditorPreview.Stop(true);
                utility.StopGroupIndex(g);
                if (!Application.isPlaying) SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8);
        }

        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Master Controls", EditorStyles.boldLabel);
        if (GUILayout.Button("Stop All Groups", GUILayout.Height(25)))
        {
            if (!Application.isPlaying) DOTweenEditorPreview.Stop(true);
            utility.StopAllGroups();
            if (!Application.isPlaying) SceneView.RepaintAll();
        }
    }
}
