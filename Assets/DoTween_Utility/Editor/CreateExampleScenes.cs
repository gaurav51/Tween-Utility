using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DG.Tweening;

public class CreateExampleScenes : EditorWindow
{
    private static void SetupExampleEnvironment()
    {
        if (!AssetDatabase.IsValidFolder("Assets/DoTween_Utility/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets/DoTween_Utility", "Scenes");
        }

        // Create robust shader setup for both Built-in and URP compatibility
        Shader primaryShader = Shader.Find("Universal Render Pipeline/Lit");
        if (primaryShader == null) primaryShader = Shader.Find("Standard");

        // Create Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "StudioFloor";
        floor.transform.position = new Vector3(0, -1.5f, 0);
        floor.transform.localScale = new Vector3(10, 1, 10);
        Material floorMat = new Material(primaryShader) { color = new Color(0.1f, 0.1f, 0.12f) };
        if (floorMat.HasProperty("_Smoothness")) floorMat.SetFloat("_Smoothness", 0.8f);
        if (floorMat.HasProperty("_Metallic")) floorMat.SetFloat("_Metallic", 0.1f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // Adjust camera and lighting to view them beautifully
        Camera.main.transform.position = new Vector3(0, 3, -11);
        Camera.main.transform.localEulerAngles = new Vector3(15, 0, 0);
        Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
        Camera.main.clearFlags = CameraClearFlags.SolidColor; // Moody dark background

        // Ensure we have a nice main light
        Light dirLight = RenderSettings.sun;
        if (dirLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            RenderSettings.sun = dirLight;
        }
        dirLight.transform.localEulerAngles = new Vector3(45, 30, 0);
        dirLight.color = new Color(1f, 0.95f, 0.9f);
        dirLight.intensity = 1.2f;
        dirLight.shadows = LightShadows.Soft;

        // Add a rim light for extra pop
        GameObject rimLightObj = new GameObject("Rim Light");
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.transform.localEulerAngles = new Vector3(30, -150, 0);
        rimLight.color = new Color(0.4f, 0.6f, 1f);
        rimLight.intensity = 0.8f;
        rimLight.shadows = LightShadows.None;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.2f);
    }

    [MenuItem("DoTween Utility/Create Example Scene (Multiple Objects)")]
    public static void CreateMultipleExampleScene()
    {
        string scenePath = "Assets/DoTween_Utility/Scenes/DoTween_Multiple_Example.unity";

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        SetupExampleEnvironment();

        Shader primaryShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // 1. Cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "AnimatedCube";
        cube.transform.position = new Vector3(-4.5f, 0, 0);
        Material cubeMat = new Material(primaryShader) { color = new Color(0.2f, 0.7f, 1f) }; // Cyan
        cube.GetComponent<Renderer>().sharedMaterial = cubeMat;

        // 2. Sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AnimatedSphere";
        sphere.transform.position = new Vector3(-1.5f, 0, 0);
        Material sphereMat = new Material(primaryShader) { color = new Color(1f, 0.3f, 0.5f) }; // Pink
        sphere.GetComponent<Renderer>().sharedMaterial = sphereMat;

        // 3. Cylinder
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "AnimatedCylinder";
        cylinder.transform.position = new Vector3(1.5f, 0, 0);
        Material cylinderMat = new Material(primaryShader) { color = new Color(0.4f, 0.9f, 0.3f) }; // Lime
        cylinder.GetComponent<Renderer>().sharedMaterial = cylinderMat;
        
        // 4. Capsule (New)
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "AnimatedCapsule";
        capsule.transform.position = new Vector3(4.5f, 0, 0);
        Material capsuleMat = new Material(primaryShader) { color = new Color(0.9f, 0.7f, 0.1f) }; // Yellow-Orange
        capsule.GetComponent<Renderer>().sharedMaterial = capsuleMat;

        // 5. Quad/Torus replacement (We will use a scaled cube acting as a platform)
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "AnimatedPlatform";
        platform.transform.position = new Vector3(0, -1f, 3f);
        platform.transform.localScale = new Vector3(8f, 0.2f, 2f);
        Material platformMat = new Material(primaryShader) { color = new Color(0.6f, 0.2f, 0.9f) }; // Purple
        platform.GetComponent<Renderer>().sharedMaterial = platformMat;

        // Create the Sequence Manager
        GameObject sequenceManager = new GameObject("SequenceManager");
        var utility = sequenceManager.AddComponent<DoTween_Utility_Multiple>();

        // Configure Group 1: The Cube (Bouncing and Rotating)
        TweenObjectGroup group1 = new TweenObjectGroup { groupName = "Cube Sequence", targetObject = cube, sequenceLoops = -1, sequenceLoopType = LoopType.Yoyo };
        group1.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(-4.5f, 2, 0), duration = 1f, easeType = Ease.OutQuad });
        group1.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Rotation, targetVectorValue = new Vector3(0, 180, 0), duration = 1f, joinWithPrevious = true });

        // Configure Group 2: The Sphere (Scaling up and down with color/fade if it had sprite, but it's 3D so just PUNCH scaling)
        TweenObjectGroup group2 = new TweenObjectGroup { groupName = "Sphere Punch", targetObject = sphere, sequenceLoops = -1 };
        group2.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.PunchScale, targetVectorValue = new Vector3(1.5f, 1.5f, 1.5f), duration = 1f, shakeStrength = 0.5f, vibrato = 10 });
        group2.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.LocalPosition, targetVectorValue = new Vector3(0, 0, 0), duration = 1f }); // Dummy wait

        // Configure Group 3: The Cylinder (Moving relative in a square)
        TweenObjectGroup group3 = new TweenObjectGroup { groupName = "Cylinder Square Path", targetObject = cylinder, sequenceLoops = -1, sequenceLoopType = LoopType.Restart };
        group3.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(0, 2, 0), duration = 0.5f, isRelative = true, easeType = Ease.Linear });
        group3.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(2, 0, 0), duration = 0.5f, isRelative = true, easeType = Ease.Linear });
        group3.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(0, -2, 0), duration = 0.5f, isRelative = true, easeType = Ease.Linear });
        group3.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(-2, 0, 0), duration = 0.5f, isRelative = true, easeType = Ease.Linear });

        // Configure Group 4: The Capsule (Scale stretch and relax)
        TweenObjectGroup group4 = new TweenObjectGroup { groupName = "Capsule Stretch", targetObject = capsule, sequenceLoops = -1, sequenceLoopType = LoopType.Yoyo };
        group4.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Scale, targetVectorValue = new Vector3(0.5f, 2f, 0.5f), duration = 0.5f, easeType = Ease.OutElastic });
        group4.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Scale, targetVectorValue = new Vector3(1.5f, 0.5f, 1.5f), duration = 0.5f, easeType = Ease.InElastic });
        
        // Configure Group 5: The Platform (Hovering up and down)
        TweenObjectGroup group5 = new TweenObjectGroup { groupName = "Platform Hover (Slow)", targetObject = platform, sequenceLoops = -1, sequenceLoopType = LoopType.Yoyo };
        group5.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(0, 0.5f, 3f), duration = 3f, easeType = Ease.InOutSine });

        // Add groups to utility
        utility.tweenGroups.Add(group1);
        utility.tweenGroups.Add(group2);
        utility.tweenGroups.Add(group3);
        utility.tweenGroups.Add(group4);
        utility.tweenGroups.Add(group5);

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Successfully created and saved Mulitple Utility Example scene at: " + scenePath);
        
        Selection.activeGameObject = sequenceManager;
        EditorGUIUtility.PingObject(sequenceManager);
    }

    [MenuItem("DoTween Utility/Create Example Scene (Single Object)")]
    public static void CreateSingleExampleScene()
    {
        string scenePath = "Assets/DoTween_Utility/Scenes/DoTween_Single_Example.unity";

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        SetupExampleEnvironment();

        Shader primaryShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // Focus entirely on a single complex object
        GameObject starburst = GameObject.CreatePrimitive(PrimitiveType.Cube);
        starburst.name = "ComplexAnimatedCube";
        starburst.transform.position = new Vector3(0, 0, 0);
        Material starMat = new Material(primaryShader) { color = new Color(1f, 0.5f, 0f) }; // Orange
        if (starMat.HasProperty("_EmissionColor")) 
        {
            starMat.EnableKeyword("_EMISSION");
            starMat.SetColor("_EmissionColor", new Color(0.5f, 0.1f, 0f));
        }
        starburst.GetComponent<Renderer>().sharedMaterial = starMat;

        var utility = starburst.AddComponent<DoTween_Utility>();
        utility.sequenceLoops = -1;
        utility.sequenceLoopType = LoopType.Restart;
        
        // Let's create a crazy chained sequence of animations to show off what one object can do
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(0, 3, 0), duration = 1f, easeType = Ease.OutBack });
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Rotation, targetVectorValue = new Vector3(180, 360, 45), duration = 1f, joinWithPrevious = true });
        
        // Turns red after a short 0.5s delay (hanging in air)
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Color, targetColorValue = Color.red, duration = 0.5f, delay = 0.5f });
        
        // Shake violently mid-air
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.ShakeRotation, targetVectorValue = new Vector3(90, 90, 90), duration = 0.5f, vibrato = 20, shakeStrength = 1.5f });
        
        // Slam down into the ground
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(0, 0, 0), duration = 0.2f, easeType = Ease.InExpo });
        
        // On impact, squash/stretch and return to orange
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.PunchScale, targetVectorValue = new Vector3(2f, -0.5f, 2f), duration = 0.8f, vibrato = 5, easeType = Ease.OutElastic });
        utility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Color, targetColorValue = new Color(1f, 0.5f, 0f), duration = 0.5f, joinWithPrevious = true });

        // Add a second standalone object: A soothing, slowly bobbing Sphere
        GameObject bobSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bobSphere.name = "CalmBobbingSphere";
        bobSphere.transform.position = new Vector3(-3, 1, 0);
        Material bobMat = new Material(primaryShader) { color = new Color(0.2f, 0.8f, 1f) }; // Light Blue
        bobSphere.GetComponent<Renderer>().sharedMaterial = bobMat;
        
        var bobUtility = bobSphere.AddComponent<DoTween_Utility>();
        bobUtility.sequenceLoops = -1;
        bobUtility.sequenceLoopType = LoopType.Yoyo;
        bobUtility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Position, targetVectorValue = new Vector3(-3, 2, 0), duration = 2f, easeType = Ease.InOutSine, startTrigger = DoTween_Utility.StartTrigger.OnAwake });
        bobUtility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Scale, targetVectorValue = new Vector3(1.2f, 1.2f, 1.2f), duration = 2f, joinWithPrevious = true });
        
        // Add a third standalone object: A constant fast spinning Cylinder
        GameObject spinCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spinCyl.name = "SpinningCylinder";
        spinCyl.transform.position = new Vector3(3, 1, 0);
        Material spinMat = new Material(primaryShader) { color = new Color(0.8f, 0.2f, 1f) }; // Purple
        spinCyl.GetComponent<Renderer>().sharedMaterial = spinMat;
        
        var spinUtility = spinCyl.AddComponent<DoTween_Utility>();
        spinUtility.sequenceLoops = -1;
        spinUtility.sequenceLoopType = LoopType.Restart; // Restart to spin infinitely in same direction
        spinUtility.tweenSteps.Add(new TweenStep { tweenAttribute = DoTween_Utility.TweenAttribute.Rotation, targetVectorValue = new Vector3(0, 360, 0), duration = 1.5f, easeType = Ease.Linear, isRelative = true, startTrigger = DoTween_Utility.StartTrigger.OnAwake });

        Camera.main.transform.position = new Vector3(0, 2, -7);
        Camera.main.transform.localEulerAngles = new Vector3(10, 0, 0);

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Successfully created and saved Single Utility Example scene at: " + scenePath);
        
        Selection.activeGameObject = starburst;
        EditorGUIUtility.PingObject(starburst);
    }
}
