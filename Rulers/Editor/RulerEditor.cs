using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Loqheart.Utility
{
    [CustomEditor(typeof(RulerData))]
    public class RulerDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("RulerData for the Rulers editor to persist your settings per scene.  It won't be included in your build.", MessageType.Info);
        }
    }

    public class RulerEditor : EditorWindow
    {
        string settingsStr = "Settings";
        string showTooltipsStr = "Tooltips";
        string enableShortcutsStr = "Shortcuts";
        GUIContent resetDataGC;
        GUIContent clearFilterGC;
        GUIContent angleModeGC;
        GUIContent visibilityGC;
        GUIContent duplicateRulerGC;

        GUIContent exDataDistanceGC;
        GUIContent exDataAngleGC;

        GUIContent deleteRulerGC;
        GUIContent frameGC;
        GUIContent addRulerGC;


        // Editor Window UI
        GUIStyle headerStyle;
        GUIStyle miniButtonStyle;
        GUIStyle boldStyle;
        GUIStyle groupFoldoutStyle;

        GUIStyle toolbarStyle;
        string[] toolbarStrings;

        GUIStyle toggleStyle;

        GUIStyle labelStyle = new GUIStyle();


        GUILayoutOption colorFieldWidth = GUILayout.Width(50);
        GUILayoutOption exDataWidth = GUILayout.Width(75);

        bool showSettings = false;
        Vector2 scrollPos = Vector2.zero;
        Transform selectedTransform;
        Scene currentScene;

        // for ordered selection
        HashSet<GameObject> selectionSet = new HashSet<GameObject>();
        HashSet<GameObject> newSet = new HashSet<GameObject>();
        HashSet<GameObject> deleteSet = new HashSet<GameObject>();
        List<GameObject> selectionOrdered = new List<GameObject>();

        RulerData data;

        [MenuItem("Window/Rulers")]
        public static void DisplayWindow()
        {
            GetWindow<RulerEditor>();
        }

        private void OnHierarchyChange()
        {
            // manualy check to see if the scene has changed
            // if so reload the data
            if (currentScene != EditorSceneManager.GetActiveScene())
            {
                currentScene = EditorSceneManager.GetActiveScene();
                data = null;
                selectionSet = null;
                newSet = null;
                deleteSet = null;
                selectionOrdered = null;
                CheckInit();
            }
        }

        private void OnSelectionChange()
        {
            newSet.Clear();
            newSet.UnionWith(Selection.gameObjects);

            deleteSet.Clear();
            deleteSet.UnionWith(selectionSet);
            deleteSet.ExceptWith(newSet);
            foreach (var g in deleteSet)
            {
                selectionSet.Remove(g);
                selectionOrdered.Remove(g);
            }

            newSet.ExceptWith(selectionSet);
            foreach (var g in newSet)
            {
                selectionSet.Add(g);
                selectionOrdered.Add(g);
            }
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            data = null;

            selectionSet = null;
            newSet = null;
            deleteSet = null;
            selectionOrdered = null;
        }

        void OnEnable()
        {
            currentScene = EditorSceneManager.GetActiveScene();
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            titleContent = new GUIContent("Rulers");
            selectionSet = new HashSet<GameObject>();
            newSet = new HashSet<GameObject>();
            deleteSet = new HashSet<GameObject>();
            selectionOrdered = new List<GameObject>();
        }

        void ResetData()
        {
            if (EditorUtility.DisplayDialog("Rulers Reset Data", "Are you sure you want reset?", "Yes", "No"))
            {
                var rulerGameObject = GameObject.Find("RulerData");
                if (rulerGameObject != null)
                {
                    DestroyImmediate(rulerGameObject);
                    data = null;
                }

                CheckInit();
            }
        }

        void InstanceGUIContent()
        {
            if (data.showTooltips)
            {
                resetDataGC = new GUIContent("Reset Data", "Removes the rulers in the scene and resets settings.");
                clearFilterGC = new GUIContent("x", "clear filter");
                angleModeGC = new GUIContent("angle mode", "display angle information of ruler depending on mode");
                visibilityGC = new GUIContent("", "visibility");
                duplicateRulerGC = new GUIContent("★", "duplicate ruler");
                exDataDistanceGC = new GUIContent("Δ", "show component distances");
                exDataAngleGC = new GUIContent("∠", "show component angles");
                deleteRulerGC = new GUIContent("x", "delete ruler");
                frameGC = new GUIContent("/", "frame selected");
                addRulerGC = new GUIContent("+", "add empty ruler,\n or will create from 2 selected objects\n Shortcut: Ctrl + Shift + R");
            }
            else
            {
                resetDataGC = new GUIContent("Reset Data");
                clearFilterGC = new GUIContent("x");
                angleModeGC = new GUIContent("angle mode", "");
                visibilityGC = new GUIContent("");
                duplicateRulerGC = new GUIContent("★");
                exDataDistanceGC = new GUIContent("Δ");
                exDataAngleGC = new GUIContent("∠");
                deleteRulerGC = new GUIContent("x");
                frameGC = new GUIContent("/");
                addRulerGC = new GUIContent("+");
            }
        }

        void CheckInit()
        {
            if (data == null)
            {
                var rulerGameObject = GameObject.Find("RulerData");

                if (rulerGameObject == null)
                {
                    rulerGameObject = new GameObject("RulerData");
                    rulerGameObject.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSaveInBuild;
                    data = rulerGameObject.AddComponent<RulerData>();

                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                else
                {
                    data = rulerGameObject.GetComponent<RulerData>();
                }

                scrollPos = Vector2.zero;

                headerStyle = new GUIStyle(EditorStyles.label);
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.fontSize = 14;

                miniButtonStyle = new GUIStyle(EditorStyles.miniButton);

                boldStyle = new GUIStyle(EditorStyles.boldLabel);

                groupFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                groupFoldoutStyle.fontStyle = FontStyle.Bold;

                labelStyle.fontSize = 14;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.normal.textColor = new Color(.8f, .8f, .8f);
                var backtexture = new Texture2D(1, 1);
                backtexture.wrapMode = TextureWrapMode.Repeat;
                backtexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
                backtexture.Apply();
                labelStyle.normal.background = backtexture;

                toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
                toolbarStyle.fontSize = 11;
                toolbarStrings = new string[] { "Hide All", "Show All" };

                toggleStyle = new GUIStyle(EditorStyles.toggle);
                toggleStyle.fixedWidth = 20;

                InstanceGUIContent();
            }
        }

        #region Dirty
        void MarkDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        void CheckDirty(ref int oldVal, int newVal)
        {
            if (newVal != oldVal)
            {
                oldVal = newVal;
                MarkDirty();
            }
        }

        void CheckDirty(ref Color oldVal, Color newVal)
        {
            if (newVal != oldVal)
            {
                oldVal = newVal;
                MarkDirty();
            }
        }

        void CheckDirty(ref bool oldVal, bool newVal)
        {
            if (newVal != oldVal)
            {
                oldVal = newVal;
                MarkDirty();
            }
        }

        void CheckDirty(ref RulerAngleMode oldVal, RulerAngleMode newVal)
        {
            if (newVal != oldVal)
            {
                oldVal = newVal;
                MarkDirty();
            }
        }
        #endregion Dirty

        void OnGUI()
        {
            CheckInit();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            #region settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showSettings = EditorGUILayout.Foldout(showSettings, settingsStr, groupFoldoutStyle);

            if (showSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(showTooltipsStr, GUILayout.Width(50));
                var showTooltips = EditorGUILayout.Toggle(data.showTooltips, GUILayout.Width(50));
                if (showTooltips != data.showTooltips)
                {
                    data.showTooltips = showTooltips;
                    InstanceGUIContent();
                    MarkDirty();
                }

                EditorGUILayout.LabelField(enableShortcutsStr, GUILayout.Width(60));
                var enableShortcuts = EditorGUILayout.Toggle(data.enableShortcuts, GUILayout.Width(50));
                if (enableShortcuts != data.enableShortcuts)
                {
                    data.enableShortcuts = enableShortcuts;
                    MarkDirty();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(resetDataGC, GUILayout.Width(100)))
                {
                    ResetData();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("ruler thickness");
                CheckDirty(ref data.rulerThickness, EditorGUILayout.IntSlider(data.rulerThickness, 1, 30));

                EditorGUILayout.LabelField("ruler color");
                CheckDirty(ref data.rulerColor, EditorGUILayout.ColorField(data.rulerColor));

                EditorGUILayout.LabelField("text size");
                CheckDirty(ref data.fontSize, EditorGUILayout.IntSlider(data.fontSize, 4, 40));
                labelStyle.fontSize = data.fontSize;

                EditorGUILayout.LabelField("text color");
                CheckDirty(ref data.textColor, EditorGUILayout.ColorField(data.textColor));
                labelStyle.normal.textColor = data.textColor;

                EditorGUILayout.LabelField(angleModeGC);
                CheckDirty(ref data.angleMode, (RulerAngleMode)EditorGUILayout.EnumPopup(data.angleMode));
            }

            EditorGUILayout.EndVertical();
            #endregion settings

            #region filter
            var toolbarSelection = GUILayout.Toolbar(-1, toolbarStrings, toolbarStyle);
            switch (toolbarSelection)
            {
                case 0:
                    for (int i = 0; i < data.rulers.Length; ++i)
                    {
                        data.rulers[i].isVisible = false;
                    }
                    MarkDirty();
                    break;
                case 1:
                    for (int i = 0; i < data.rulers.Length; ++i)
                    {
                        data.rulers[i].isVisible = true;
                    }
                    MarkDirty();
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("filter", GUILayout.Width(50));
            var filterTransform = (Transform)EditorGUILayout.ObjectField(data.filterTransform, typeof(Transform), true, GUILayout.ExpandWidth(true));
            if (filterTransform != data.filterTransform)
            {
                data.filterTransform = filterTransform;
                MarkDirty();
            }

            if (GUILayout.Button(clearFilterGC, miniButtonStyle, GUILayout.Width(20)))
            {
                if (data.filterTransform != null)
                {
                    data.filterTransform = null;
                    MarkDirty();
                }
            }

            EditorGUILayout.EndHorizontal();
            #endregion filter

            EditorGUILayout.BeginVertical();
            int removeRulerIndex = -1;
            int duplicateRulerIndex = -1;
            for (int i = 0; i < data.rulers.Length; ++i)
            {
                var r = data.rulers[i];
                if (data.filterTransform != null && r.a != data.filterTransform && r.b != data.filterTransform)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();

                CheckDirty(ref r.isVisible, GUILayout.Toggle(r.isVisible, visibilityGC, toggleStyle));

                if (GUILayout.Button(duplicateRulerGC, miniButtonStyle, GUILayout.Width(20)))
                {
                    duplicateRulerIndex = i;
                }

                CheckDirty(ref r.color, EditorGUILayout.ColorField(r.color, colorFieldWidth));
                CheckDirty(ref r.textColor, EditorGUILayout.ColorField(r.textColor, colorFieldWidth));

                var isSetExDataDistance = GUILayout.Toggle(RulerExDataMode.Distance == r.exDataMode, exDataDistanceGC, "Button");
                if (isSetExDataDistance && r.exDataMode != RulerExDataMode.Distance)
                {
                    r.exDataMode = RulerExDataMode.Distance;
                    MarkDirty();
                }

                var isSetExDataAngle = GUILayout.Toggle(RulerExDataMode.Angle == r.exDataMode, exDataAngleGC, "Button");
                if (isSetExDataAngle && r.exDataMode != RulerExDataMode.Angle)
                {
                    r.exDataMode = RulerExDataMode.Angle;
                    MarkDirty();
                }

                if (!isSetExDataDistance && !isSetExDataAngle && r.exDataMode != RulerExDataMode.None)
                {
                    r.exDataMode = RulerExDataMode.None;
                    MarkDirty();
                }

                switch (r.exDataMode)
                {
                    case RulerExDataMode.Distance:
                        var delta = r.delta;
                        EditorGUILayout.LabelField("Δx " + delta.x.ToString("0.00"), boldStyle, exDataWidth);
                        EditorGUILayout.LabelField("Δy " + delta.y.ToString("0.00"), boldStyle, exDataWidth);
                        EditorGUILayout.LabelField("Δz" + delta.z.ToString("0.00"), boldStyle, exDataWidth);
                        break;
                    case RulerExDataMode.Angle:
                        var angles = r.GetAngles(data.angleMode);
                        switch (data.angleMode)
                        {
                            case RulerAngleMode.DirectionCosines:
                                EditorGUILayout.LabelField("∠α " + angles.x.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠β " + angles.y.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠γ " + angles.z.ToString("0.00"), boldStyle, exDataWidth);

                                break;
                            case RulerAngleMode.PlaneProjection:
                                EditorGUILayout.LabelField("∠xy " + angles.x.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠yz " + angles.y.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠xz " + angles.z.ToString("0.00"), boldStyle, exDataWidth);
                                break;
                            case RulerAngleMode.POV:
                                EditorGUILayout.LabelField("∠p " + angles.x.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠y " + angles.y.ToString("0.00"), boldStyle, exDataWidth);
                                EditorGUILayout.LabelField("∠r " + angles.z.ToString("0.00"), boldStyle, exDataWidth);
                                break;
                        }
                        break;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(deleteRulerGC, miniButtonStyle, GUILayout.Width(20)))
                {
                    removeRulerIndex = i;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(frameGC, miniButtonStyle, GUILayout.Width(20)))
                {
                    // search for objects
                    selectedTransform = r.a;
                }
                var aTransform =  (Transform)EditorGUILayout.ObjectField(r.a, typeof(Transform), true);
                if (aTransform != r.a)
                {
                    r.a = aTransform;
                    MarkDirty();
                }
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(r.delta.magnitude.ToString("0.00"), boldStyle, GUILayout.Width(40));

                //EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(frameGC, miniButtonStyle, GUILayout.Width(20)))
                {
                    // search for objects
                    selectedTransform = r.b;
                }

                var bTransform = (Transform)EditorGUILayout.ObjectField(r.b, typeof(Transform), true);
                if (bTransform != r.b)
                {
                    r.b = bTransform;
                    MarkDirty();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            if (duplicateRulerIndex != -1)
            {
                var ruler = data.rulers[duplicateRulerIndex];
                var r = new Ruler();
                r.color = ruler.color;
                r.textColor = ruler.textColor;
                r.a = ruler.a;
                r.b = ruler.b;
                data.Add(r, duplicateRulerIndex + 1);
                MarkDirty();
            }

            if (removeRulerIndex != -1)
            {
                data.RemoveAt(removeRulerIndex);
                MarkDirty();
            }

            if (GUILayout.Button(addRulerGC, miniButtonStyle))
            {
                AddRuler();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            CheckShortcuts();
        }

        void AddRuler()
        {
            var r = new Ruler();
            r.color = data.rulerColor;
            r.textColor = data.textColor;
            if (selectionOrdered.Count > 0)
            {
                r.a = selectionOrdered[0].transform;
            }

            if (selectionOrdered.Count > 1)
            {
                r.b = selectionOrdered[1].transform;
            }
            data.Add(r);
            MarkDirty();
        }

        void CheckShortcuts()
        {
            if (!data.enableShortcuts)
            {
                return;
            }

            Event e = Event.current;
            if (e.shift && e.control && e.keyCode == KeyCode.R && e.type == EventType.KeyUp)
            {
                AddRuler();
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            CheckInit();

            CheckShortcuts();

            Color oldColor = Handles.color;
            int controlId = 0;
            foreach (var r in data.rulers)
            {
                if (data.filterTransform != null && r.a != data.filterTransform && r.b != data.filterTransform)
                {
                    continue;
                }

                if (!r.isVisible)
                {
                    continue;
                }

                if (selectedTransform != null && (selectedTransform == r.a || selectedTransform == r.b))
                {
                    Selection.objects = new UnityEngine.Object[] { selectedTransform.gameObject };
                    SceneView.lastActiveSceneView.FrameSelected();
                    selectedTransform = null;
                }

                Handles.color = r.color;

                if (r.a != null && r.b != null)
                {
                    Handles.DrawAAPolyLine(data.rulerThickness, new Vector3[] { r.a.position, r.b.position });
                    Handles.SphereCap(controlId, r.a.position, r.a.rotation, 0.25f);

                    Vector3 delta = r.delta;
                    float mag = delta.magnitude;
                    var n = delta.normalized;
                    Handles.ArrowCap(controlId + 1, r.b.position - 1.14f * n, mag < 0.0001f ? Quaternion.identity : Quaternion.LookRotation(delta), 1f);

                    labelStyle.normal.textColor = r.textColor;
                    labelStyle.normal.background.SetPixel(0, 0, new Color(r.color.r, r.color.g, r.color.b, 0.5f));
                    labelStyle.normal.background.Apply();

                    var labelSB = new StringBuilder(64);
                    labelSB.Append(mag.ToString("0.00"));

                    switch (r.exDataMode)
                    {
                        case RulerExDataMode.Distance:
                            labelSB.Append("\nΔx ");
                            labelSB.AppendLine(delta.x.ToString("0.00"));
                            labelSB.Append("Δy ");
                            labelSB.AppendLine(delta.y.ToString("0.00"));
                            labelSB.Append("Δz ");
                            labelSB.Append(delta.z.ToString("0.00"));
                            break;
                        case RulerExDataMode.Angle:
                            var angles = r.GetAngles(data.angleMode);
                            switch (data.angleMode)
                            {
                                case RulerAngleMode.DirectionCosines:
                                    labelSB.Append("\n∠α ");
                                    labelSB.AppendLine(angles.x.ToString("0.00"));
                                    labelSB.Append("∠β ");
                                    labelSB.AppendLine(angles.y.ToString("0.00"));
                                    labelSB.Append("∠γ ");
                                    labelSB.Append(angles.z.ToString("0.00"));
                                    break;

                                case RulerAngleMode.PlaneProjection:
                                    labelSB.Append("\n∠xy ");
                                    labelSB.AppendLine(angles.x.ToString("0.00"));
                                    labelSB.Append("∠yz ");
                                    labelSB.AppendLine(angles.y.ToString("0.00"));
                                    labelSB.Append("∠xz ");
                                    labelSB.Append(angles.z.ToString("0.00"));
                                    break;

                                case RulerAngleMode.POV:
                                    labelSB.Append("\n∠p ");
                                    labelSB.AppendLine(angles.x.ToString("0.00"));
                                    labelSB.Append("∠y ");
                                    labelSB.AppendLine(angles.y.ToString("0.00"));
                                    labelSB.Append("∠r ");
                                    labelSB.Append(angles.z.ToString("0.00"));
                                    break;
                            }
                            break;
                    }
                    Handles.Label(mag * 0.5f * n + r.a.position, labelSB.ToString(), labelStyle);
                }
                else
                {
                    if (r.a == null)
                    {
                        Handles.SphereCap(controlId, Vector3.zero, Quaternion.identity, 0.25f);
                    }
                    else
                    {
                        Handles.SphereCap(controlId, r.a.position, Quaternion.identity, 0.25f);
                    }

                    if (r.b == null)
                    {
                        Handles.ArrowCap(controlId + 1, Vector3.zero, Quaternion.identity, 1f);
                    }
                    else
                    {
                        Handles.ArrowCap(controlId + 1, r.b.position, Quaternion.identity, 1f);
                    }
                }
                controlId += 2;
            }

            Handles.color = oldColor;

            SceneView.RepaintAll();

            Repaint();
        }
    }
}
