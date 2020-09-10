using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicsObjectPlacer))]
public class PhysicsObjectPlacerEditor : Editor
{

    bool active;
    string ButtonName = "Start";
    int columnCount = 4;
    GameObject tilePrefab;
    int Index;
    List<Texture2D> gameObjTex = new List<Texture2D>();


    void SetAnimatedMaterial(bool isActive)
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
        {
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.sceneViewState.showMaterialUpdate = isActive;
        }
    }

    private void OnEnable()
    { 
        PhysicsObjectPlacer physicsObjectPlacer = (PhysicsObjectPlacer)target;

        for (int i = 0; i < physicsObjectPlacer.DragAndDropObjects.Count; i++)
        {
            Texture2D previewTex = AssetPreview.GetAssetPreview(physicsObjectPlacer.DragAndDropObjects[i]);

            if (!gameObjTex.Contains(previewTex))
                gameObjTex.Add(previewTex);
        }

        SetAnimatedMaterial(true);
    }

    private void OnDisable()
    {
        Tools.current = Tool.None;

        SetAnimatedMaterial(false);
    }


    void Awake()
    {
        PhysicsObjectPlacer physicsObjectPlacer = (PhysicsObjectPlacer)target;
        //physicsObjectPlacer.DragAndDropObjects.Clear();
        //gameObjTex.Clear();
    }

    void OnSceneGUI()
    {

        if (Tools.current != Tool.Custom)
            Tools.current = Tool.Custom;

        PhysicsObjectPlacer physicsObjectPlacer = (PhysicsObjectPlacer)target;
        Event e = Event.current;

        if (physicsObjectPlacer.DragAndDropObjects.Count !=0)
        {

            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.A))
                        {
                            physicsObjectPlacer.Activate = true;
                            ButtonName = "Stop";
                            active = true;
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (Event.current.keyCode == (KeyCode.A))
                        {
                            physicsObjectPlacer.Activate = false;
                            ButtonName = "Start";
                            active = false;
                        }
                        break;
                    }
            }
        }
    }

    //Color restoreColor = GUI.color;
    //Color UIColor;

    public override void OnInspectorGUI()
    {

        PhysicsObjectPlacer physicsObjectPlacer = (PhysicsObjectPlacer)target;   

        if (physicsObjectPlacer.Activate)
        {
            ButtonName = "Stop";
            active = true;
        }
        else
        {
            ButtonName = "Start";
            active = false;
        }

        EditorGUILayout.Separator();

        EditorGUI.BeginDisabledGroup(physicsObjectPlacer.DragAndDropObjects.Count == 0);

        GUIContent content = new GUIContent(ButtonName, "Start and Stop hotkey is 'A'");
        //GUI.backgroundColor = UIColor;
        if (GUILayout.Button(content))
        {
            if (physicsObjectPlacer.DragAndDropObjects.Count != 0)
            {
                if (!active)
                {
                    //UIColor = Color.green;
                    physicsObjectPlacer.Activate = true;
                    ButtonName = "Stop";
                    active = true;
                }
                else
                {
                    //UIColor = restoreColor;
                    physicsObjectPlacer.Activate = false;
                    ButtonName = "Start";
                    active = false;
                }
            }
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();


        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Radius", EditorStyles.boldLabel);
        physicsObjectPlacer.Radius = EditorGUILayout.Slider(physicsObjectPlacer.Radius, 0.1f, 3f);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Spawn Time", EditorStyles.boldLabel);
        physicsObjectPlacer.SpawnTime = EditorGUILayout.Slider(physicsObjectPlacer.SpawnTime, 0.01f, 1f);

        EditorGUILayout.Separator();
        
        EditorGUILayout.LabelField("Max Raycasting distance", EditorStyles.boldLabel);
        physicsObjectPlacer.MaxRaycastDistance = EditorGUILayout.Slider(physicsObjectPlacer.MaxRaycastDistance, 0.1f, 1000f);

        if (EditorGUI.EndChangeCheck())
        {
            
        }


        //EditorGUILayout.Separator();

        //GUILayout.Label("Group To Combine");
        //physicsObjectPlacer.Holder = (GameObject)EditorGUILayout.ObjectField("My Object", physicsObjectPlacer.Holder, typeof(GameObject), true);

        //EditorGUILayout.Separator();
        
        
        

        /*
        EditorGUILayout.Separator();
        
        GUILayout.Label("Object");
        physicsObjectPlacer.MyObject = (GameObject)EditorGUILayout.ObjectField("My Object", physicsObjectPlacer.MyObject, typeof(GameObject), true);
        */
        //GUIStyle GUIStyleBox = new GUIStyle();

        EditorGUILayout.Separator();

        Event evt = Event.current;


        GUIStyle BoxStyle = new GUIStyle(GUI.skin.box);
        BoxStyle.alignment = TextAnchor.MiddleCenter;
        BoxStyle.fontStyle = FontStyle.Italic;
        BoxStyle.fontSize = 12;
        GUI.skin.box = BoxStyle;

        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 75.0f, GUILayout.ExpandWidth(true));

        if (physicsObjectPlacer.DragAndDropObjects.Count == 0)
        {
            GUI.Box(drop_area, "Drag Prefabs here!", BoxStyle);
        }
        else
        {
            GUI.Box(drop_area, "Prefabs Listed!", BoxStyle);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        
        switch (evt.type)
        {
            
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;
                
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {

                        physicsObjectPlacer.DragAndDropObjects.Add(DragAndDrop.objectReferences[i] as GameObject);
                        Debug.Log(DragAndDrop.objectReferences[i] + " Added, ID:" + physicsObjectPlacer.DragAndDropObjects.Count);

                        gameObjTex.Add(AssetPreview.GetAssetPreview(DragAndDrop.objectReferences[i]));
                    }

                }
                
                physicsObjectPlacer.MaxRandomValue = physicsObjectPlacer.DragAndDropObjects.Count;

                break;
        }

        Rect boxRect = EditorGUILayout.BeginVertical(); //This draws a Line to separate the Controls
        GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

        
        Index = GUILayout.SelectionGrid(Index, gameObjTex.ToArray(), 6);
        
        EditorGUILayout.EndVertical();

        GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

        EditorGUILayout.Separator();

        if (GUILayout.Button("Bake Mesh"))
        {
            physicsObjectPlacer.BakeMeshes();
            //physicsObjectPlacer.CombineMeshes();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Clean List"))
        {
            physicsObjectPlacer.DragAndDropObjects.Clear();
            gameObjTex.Clear();
            Debug.Log("List of Prefabs cleared");
        }

        EditorGUILayout.Separator();

    }
}