using System;
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
        GUIContent resetDataGC;
        GUIContent clearFilterGC;
        GUIContent visibilityGC;
        GUIContent duplicateRulerGC;
        GUIContent rulerColorGC;
        GUIContent deleteRulerGC;
        GUIContent frameGC;


        // Editor Window UI
        GUIStyle headerStyle;
        GUIStyle miniButtonStyle;
        GUIStyle boldStyle;
        GUIStyle groupFoldoutStyle;

        GUIStyle toolbarStyle;
        string[] toolbarStrings;

        GUIStyle toggleStyle;

        // Scene UI
        GUIStyle labelStyle = new GUIStyle();

        bool showSettings = false;
        Vector2 scrollPos = Vector2.zero;
        Transform selectedTransform;
        Scene currentScene;

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
                CheckInit();
            }
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            data = null;
        }

        void OnEnable()
        {
            currentScene = EditorSceneManager.GetActiveScene();
            CheckInit();

            SceneView.onSceneGUIDelegate += OnSceneGUI;
            titleContent = new GUIContent("Rulers");
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
                visibilityGC = new GUIContent("", "visibility");
                duplicateRulerGC = new GUIContent("*", "duplicate ruler");
                rulerColorGC = new GUIContent("", "ruler color");
                deleteRulerGC = new GUIContent("x", "delete ruler");
                frameGC = new GUIContent("/", "frame");
            }
            else
            {
                resetDataGC = new GUIContent("Reset Data");
                clearFilterGC = new GUIContent("x");
                visibilityGC = new GUIContent("");
                duplicateRulerGC = new GUIContent("*");
                rulerColorGC = new GUIContent(" ");
                deleteRulerGC = new GUIContent("x");
                frameGC = new GUIContent("/");
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
                groupFoldoutStyle.fontStyle = FontStyle.BoldAndItalic;

                labelStyle.fontSize = 20;
                labelStyle.normal.textColor = new Color(.8f, .8f, .8f);

                toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
                toolbarStyle.fontSize = 11;
                toolbarStrings = new string[] { "Hide All", "Show All" };

                toggleStyle = new GUIStyle(EditorStyles.toggle);
                toggleStyle.fixedWidth = 20;

                InstanceGUIContent();
            }
        }

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

        void OnGUI()
        {
            CheckInit();
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

                if (GUILayout.Button(resetDataGC, GUILayout.Width(150)))
                {
                    ResetData();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.LabelField("ruler thickness");
                CheckDirty(ref data.rulerThickness, EditorGUILayout.IntSlider(data.rulerThickness, 1, 30));

                EditorGUILayout.LabelField("ruler color");
                CheckDirty(ref data.rulerColor, EditorGUILayout.ColorField(data.rulerColor));

                EditorGUILayout.LabelField("text size");
                CheckDirty(ref data.fontSize, EditorGUILayout.IntSlider(data.fontSize, 4, 40));
                labelStyle.fontSize = data.fontSize;

                EditorGUILayout.LabelField("text color");
                CheckDirty(ref data.fontColor, EditorGUILayout.ColorField(data.fontColor));
                labelStyle.normal.textColor = data.fontColor;
                EditorGUILayout.EndVertical();
            }

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

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

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

                CheckDirty(ref r.color, EditorGUILayout.ColorField(rulerColorGC, r.color, GUILayout.Width(50)));
                EditorGUILayout.LabelField(r.delta.magnitude.ToString("0.00"), boldStyle);

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
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
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

            if (GUILayout.Button(new GUIContent("+", "add ruler"), miniButtonStyle))
            {
                var r = new Ruler();
                r.color = data.rulerColor;
                data.Add(r);
                MarkDirty();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            CheckInit();
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
                    Handles.Label(mag * 0.5f * n + r.a.position, mag.ToString("0.00"), labelStyle);
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
