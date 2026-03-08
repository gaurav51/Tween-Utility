# DOTween Utility for Unity

A powerful, visual, and highly-extensively customizable wrapper for Demigiant's [DOTween](http://dotween.demigiant.com/) library. This utility allows developers and designers to build complex, multi-step animation sequences directly inside the Unity Inspector—without writing a single line of code!

## 📹 Demo

https://github.com/user-attachments/assets/762e1a00-8911-4217-9855-a3c647d7c147

https://github.com/user-attachments/assets/db6f155b-3711-4ef8-a005-aafb13ad7602

## 🌟 Key Features

*   **Visual Sequence Builder**: Create, reorder, and chain animation steps directly in the Unity Inspector.
*   **Two Specialized Handlers**:
    *   **Single Object Utility**: Perfect for attaching to a single GameObject (UI or 3D).
    *   **Multi-Object Utility**: A centralized manager (Tabbed Interface) to coordinate completely different objects and sequences from one central script.
*   **Editor Preview**: Test your animations instantly inside the Editor without entering Play Mode.
*   **Tons of Tween Attributes Supported**: Position, Rotation, Scale, UI AnchoredPosition, Color, Fade, Shakes, Punches, Text typing, Image Fill amounts.
    *   *Includes robust Color/Fade support for both 2D (UI/Sprites) and 3D (Standard/URP Materials)*
*   **Rapid Workflow Tools**:
    *   **Get Current**: Snap the object's current Scene View transform values directly into your target configuration fields instantly.
    *   **Copy & Paste**: Duplicate specific steps easily across sequences.
    *   **Reorder Controls**: Move steps Up/Down in the execution order.
*   **Rich Callbacks**: Trigger UnityEvents and even specific AudioClips exactly when a specific animation step starts or finishes.
*   **SOLID Architecture**: Tween assembly has been fully decoupled into a static `TweenBuilder` factory to adhere to the Single Responsibility Principle.

---

## 🎨 Example Scenes

Not sure where to start? The utility can instantly generate complete example scenes for you!

1. In the top Unity Toolbar, click **DoTween Utility**.
2. Select **Create Example Scene (Single Object)** or **Create Example Scene (Multiple Objects)**.
3. The tool will instantly generate a beautifully lit showcase scene filled with primitives executing complex animation chains so you can inspect how they are built!

---

## 🛠️ Installation

**Requirement**: Ensure you have [DOTween (Free or Pro)](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-hotween-v2-27676) installed and setup in your Unity project first.

### Option A: Unity Package Manager (Recommended)
You can install this utility directly via the Unity Package Manager (UPM) using the GitHub link:
1. Open the Unity Package Manager (`Window > Package Manager`).
2. Click the `+` button in the top-left corner and select **Add package from git URL...**
3. Paste the following URL and click **Add**:
   `https://github.com/gaurav51/Tween-Utility.git?path=/Assets/DoTween_Utility`

### Option B: Manual Installation
1. Clone or download this repository.
2. Copy the `Assets/DoTween_Utility` folder directly into your Unity project's `Assets/` directory.

---

## 📖 How to Use

### 1. Single Object Animation (`DoTween_Utility`)
Great for simple, self-contained UI popups, bouncy buttons, or moving platforms.

1. Select a GameObject (e.g., a UI Image or an Enemy prefab).
2. Click **Add Component** -> `DoTween Utility -> DoTween Utility (Single)`.
3. Choose your **Start Trigger** (OnAwake, OnStart, OnEnable, or Custom).
4. Click **＋ Add New Step** to begin building your timeline:
    * Set your target values.
    * Use "**Get Current**" to quickly capture scene values.
    * Check "**Play With Previous (Parallel)**" if you want the step to play simultaneously with the step immediately above it.

### 2. Multi-Object Animation (`DoTween_Utility_Multiple`)
Great for main menus, cutscenes, and complex sequencing where many different objects animate independently.

1. Create a central an empty GameObject (e.g., `SequenceManager`).
2. Click **Add Component** -> `DoTween Utility -> DoTween Utility (Multiple)`.
3. Click **＋ Add New Object Group** to create a new Tab.
4. Drag and drop the target GameObject you wish to animate into the "Target Object" field.
5. Add steps specifically for that object. Add more groups as needed!

---

## 🎮 Editor Controls
At the bottom of the inspector, you will see Master Editor Controls:
*   ▶ **Play Sequence**: Forces DOTween to preview your chain of animations directly in the Scene View!
*   ■ **Stop**: Attempts to kill all active editor-previews and reset the objects back to their pre-animating state.

---

## 🧩 Extending

If you need to add custom DOTween extensions in the future:
1. Add your new name to the `TweenAttribute` enum in `DoTween_Utility_Enums.cs`.
2. Map its translation in the `switch` statement located inside `TweenBuilder.cs`.
3. Add the UI rendering block for your new type inside the `OnInspectorGUI` switch statement in the Editor scripts.

## 📝 License
Released under the MIT License. Free to use in commercial and open-source projects!
