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
        string precisionStr = "0.00";

        // settings strings
        string rulerThicknessStr = "ruler thickness";
        string pointSizeStr = "point size";
        string arrowSizeStr = "arrow size";
        string rulerColorStr = "ruler color";
        string textSizeStr = "text size";
        string textColorStr = "text color";
        string displayPrecisionStr = "display precision";

        string ButtonStr = "Button";
        string filterStr = "filter";

        // exdata strings
        string deltaXStr = "Δx ";
        string deltaYStr = "Δy ";
        string deltaZStr = "Δz ";

        string deltaRStr = "Δr ";
        string deltaUStr = "Δu ";
        string deltaFStr = "Δf ";

        string angleEPStr = "∠p ";
        string angleEYStr = "∠y ";
        string angleERStr = "∠r ";

        string nlStr = "\n";

        Color distXColor = new Color(1f, 0f, 1f, 1f);
        Color distYColor = new Color(1f, 1f, 0f, 1f);
        Color distZColor = new Color(0f, 1f, 1f, 1f);

        Color angleXColor = new Color(1f, 0f, 1f, 0.05f);
        Color angleYColor = new Color(1f, 1f, 0f, 0.05f);
        Color angleZColor = new Color(0f, 1f, 1f, 0.05f);

        GUIContent resetDataGC;
        GUIContent clearFilterGC;
        GUIContent visibilityGC;
        GUIContent duplicateRulerGC;

        GUIContent exDataIsLocalGC;
        GUIContent exDataIsGlobalGC;
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


        GUILayoutOption Width20 = GUILayout.Width(20);
        GUILayoutOption Width50 = GUILayout.Width(50);

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
                selectionSet.Clear();
                newSet.Clear();
                deleteSet.Clear();
                selectionOrdered.Clear();
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
                visibilityGC = new GUIContent("", "visibility");
                duplicateRulerGC = new GUIContent("★", "duplicate ruler");
                exDataIsLocalGC = new GUIContent("L", "local coordinates");
                exDataIsGlobalGC = new GUIContent("G", "global coordinates");
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
                visibilityGC = new GUIContent("");
                duplicateRulerGC = new GUIContent("★");
                exDataIsLocalGC = new GUIContent("L", "");
                exDataIsGlobalGC = new GUIContent("G", "");
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

                    if (!Application.isPlaying)
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

                precisionStr = "0." + new string('0', data.precision);

                InstanceGUIContent();
            }
        }

        #region Dirty
        void MarkDirty()
        {
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        void CheckDirty<T>(ref T oldVal, T newVal) where T:struct
        {
            if (!newVal.Equals(oldVal))
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
                EditorGUILayout.LabelField(showTooltipsStr, Width50);
                var showTooltips = EditorGUILayout.Toggle(data.showTooltips, Width50);
                if (showTooltips != data.showTooltips)
                {
                    data.showTooltips = showTooltips;
                    InstanceGUIContent();
                    MarkDirty();
                }

                EditorGUILayout.LabelField(enableShortcutsStr, GUILayout.Width(60));
                var enableShortcuts = EditorGUILayout.Toggle(data.enableShortcuts, Width50);
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

                EditorGUILayout.LabelField(rulerThicknessStr);
                CheckDirty(ref data.rulerThickness, EditorGUILayout.IntSlider(data.rulerThickness, 1, 30));

                EditorGUILayout.LabelField(pointSizeStr);
                CheckDirty(ref data.pointSize, EditorGUILayout.IntSlider(data.pointSize, 0, 100));

                EditorGUILayout.LabelField(arrowSizeStr);
                CheckDirty(ref data.arrowSize, EditorGUILayout.IntSlider(data.arrowSize, 0, 50));

                EditorGUILayout.LabelField(rulerColorStr);
                CheckDirty(ref data.rulerColor, EditorGUILayout.ColorField(data.rulerColor));

                EditorGUILayout.LabelField(textSizeStr);
                CheckDirty(ref data.fontSize, EditorGUILayout.IntSlider(data.fontSize, 4, 40));
                labelStyle.fontSize = data.fontSize;

                EditorGUILayout.LabelField(textColorStr);
                CheckDirty(ref data.textColor, EditorGUILayout.ColorField(data.textColor));
                labelStyle.normal.textColor = data.textColor;

                EditorGUILayout.LabelField(displayPrecisionStr);
                var oldPrecision = data.precision;
                CheckDirty(ref data.precision, EditorGUILayout.IntSlider(data.precision, 1, 5));
                if (data.precision != oldPrecision)
                {
                    precisionStr = "0." + new string('0', data.precision);
                }
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
            EditorGUILayout.LabelField(filterStr, Width50);
            var filterTransform = (Transform)EditorGUILayout.ObjectField(data.filterTransform, typeof(Transform), true, GUILayout.ExpandWidth(true));
            if (filterTransform != data.filterTransform)
            {
                data.filterTransform = filterTransform;
                MarkDirty();
            }

            if (GUILayout.Button(clearFilterGC, miniButtonStyle, Width20))
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

                if (GUILayout.Button(duplicateRulerGC, miniButtonStyle, Width20))
                {
                    duplicateRulerIndex = i;
                }

                CheckDirty(ref r.color, EditorGUILayout.ColorField(r.color, Width50));
                CheckDirty(ref r.textColor, EditorGUILayout.ColorField(r.textColor, Width50));

                if (GUILayout.Button(r.isLocal ? exDataIsLocalGC : exDataIsGlobalGC, Width20))
                {
                    r.isLocal = !r.isLocal;
                    MarkDirty();
                }

                var showExDist = GUILayout.Toggle(r.showExDist, exDataDistanceGC, ButtonStr);
                if (showExDist != r.showExDist)
                {
                    r.showExDist = showExDist;
                    MarkDirty();
                }

                var showExAngle = GUILayout.Toggle(r.showExAngle, exDataAngleGC, ButtonStr);
                if (showExAngle != r.showExAngle)
                {
                    r.showExAngle = showExAngle;
                    MarkDirty();
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(deleteRulerGC, miniButtonStyle, Width20))
                {
                    removeRulerIndex = i;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(frameGC, miniButtonStyle, Width20))
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

                EditorGUILayout.LabelField(r.delta.magnitude.ToString(precisionStr), boldStyle, GUILayout.Width(40));

                if (GUILayout.Button(frameGC, miniButtonStyle, Width20))
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
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

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
                    Handles.SphereHandleCap(controlId, r.a.position, r.a.rotation, 
                        HandleUtility.GetHandleSize(r.a.position) * data.pointSize / 100f, 
                        EventType.Repaint);


                    Vector3 delta = r.delta;
                    float mag = delta.magnitude;
                    var n = delta.normalized;

                    float arrowSize = HandleUtility.GetHandleSize(r.b.position) * data.arrowSize / 25f;
                    Handles.ConeHandleCap(controlId + 1, r.b.position - n * arrowSize/1.43f, 
                        mag < 0.0001f ? Quaternion.identity : Quaternion.LookRotation(delta), 
                        arrowSize, EventType.Repaint);

                    labelStyle.normal.textColor = r.textColor;
                    labelStyle.normal.background.SetPixel(0, 0, new Color(r.color.r, r.color.g, r.color.b, 0.5f));
                    labelStyle.normal.background.Apply();

                    var labelSB = new StringBuilder(64);
                    labelSB.Append(mag.ToString(precisionStr));

                    DrawExDist(r, labelSB);
                    DrawExAngle(r, labelSB);

                    Handles.Label(r.a.position + delta * 0.5f, labelSB.ToString(), labelStyle);
                }
                controlId += 2;
            }

            Handles.color = oldColor;

            SceneView.RepaintAll();

            Repaint();
        }

        void DrawExDist(Ruler r, StringBuilder sb)
        {
            if (!r.showExDist) return;

            var oldColor = Handles.color;
            var delta = r.delta;

            sb.Append(nlStr);

            if (r.isLocal)
            {
                var rdot = Vector3.Dot(delta, r.a.right);
                var udot = Vector3.Dot(delta, r.a.up);
                var fdot = Vector3.Dot(delta, r.a.forward);
                var rproj = rdot * r.a.right;
                var uproj = udot * r.a.up;
                var fproj = fdot * r.a.forward;

                //right
                Handles.color = distXColor;
                Handles.DrawLine(r.a.position, r.a.position + rproj);

                // up
                Handles.color = distYColor;
                Handles.DrawLine(r.a.position + rproj + fproj, r.a.position + rproj + fproj + uproj);

                // forward
                Handles.color = distZColor;
                Handles.DrawLine(r.a.position + rproj, r.a.position + rproj + fproj);

                sb.Append(deltaRStr);
                sb.AppendLine((rdot < 0 ? "-" : "") + rproj.magnitude.ToString(precisionStr));
                sb.Append(deltaUStr);
                sb.AppendLine((udot < 0 ? "-" : "") + uproj.magnitude.ToString(precisionStr));
                sb.Append(deltaFStr);
                sb.Append((fdot < 0 ? "-" : "") + fproj.magnitude.ToString(precisionStr));

            }
            else
            {
                var rproj = new Vector3(delta.x, 0f, 0f);
                var uproj = new Vector3(0f, delta.y, 0f);
                var fproj = new Vector3(0f, 0f, delta.z);

                //right
                Handles.color = distXColor;
                Handles.DrawLine(r.a.position, r.a.position + rproj);

                // up
                Handles.color = distYColor;
                Handles.DrawLine(r.a.position + rproj + fproj, r.a.position + rproj + fproj + uproj);

                // forward
                Handles.color = distZColor;
                Handles.DrawLine(r.a.position + rproj, r.a.position + rproj + fproj);

                sb.Append(deltaXStr);
                sb.AppendLine(delta.x.ToString(precisionStr));
                sb.Append(deltaYStr);
                sb.AppendLine(delta.y.ToString(precisionStr));
                sb.Append(deltaZStr);
                sb.Append(delta.z.ToString(precisionStr));
            }
            Handles.color = oldColor;
        }

        void DrawExAngle(Ruler r, StringBuilder sb)
        {
            if (!r.showExAngle) return;
            var oldColor = Handles.color;
            var delta = r.delta;
            var angles = r.GetAngles();
            var mag = delta.magnitude;

            sb.Append(nlStr);

            if (r.isLocal)
            {
                // pitch
                Handles.color = angleXColor;
                Handles.DrawSolidArc(r.a.position, r.a.right, r.a.forward, angles.x, mag);

                // yaw
                Handles.color = angleYColor;
                Handles.DrawSolidArc(r.a.position, r.a.up, r.a.forward, angles.y, mag);

                // roll
                Handles.color = angleZColor;
                Handles.DrawSolidArc(r.a.position, r.a.forward, r.a.right, angles.z, mag);

                sb.Append(angleEPStr);
                sb.AppendLine(angles.x.ToString(precisionStr));
                sb.Append(angleEYStr);
                sb.AppendLine(angles.y.ToString(precisionStr));
                sb.Append(angleERStr);
                sb.Append(angles.z.ToString(precisionStr));
            }
            else
            {
                // pitch
                Handles.color = angleXColor;
                Handles.DrawSolidArc(r.a.position, Vector3.right, Vector3.forward, angles.x, mag);

                // yaw
                Handles.color = angleYColor;
                Handles.DrawSolidArc(r.a.position, Vector3.up, Vector3.forward, angles.y, mag);

                // roll
                Handles.color = angleZColor;
                Handles.DrawSolidArc(r.a.position, Vector3.forward, Vector3.right, angles.z, mag);

                sb.Append(angleEPStr);
                sb.AppendLine(angles.x.ToString(precisionStr));
                sb.Append(angleEYStr);
                sb.AppendLine(angles.y.ToString(precisionStr));
                sb.Append(angleERStr);
                sb.Append(angles.z.ToString(precisionStr));
            }
            Handles.color = oldColor;
        }
    }
}
