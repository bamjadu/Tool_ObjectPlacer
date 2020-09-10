using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.EditorTools;



[EditorTool("Physics Object Placer")]
public class PhysicsObjectPlacerTool : EditorTool
{

    [SerializeField]
    public Texture2D m_ToolIcon;

    const string ToolName = "PhysicsObjectPlacer";

    GUIContent m_IconContent;

    GameObject targetTool;

    bool isInit = false;

    

    void OnActiveToolChanged()
    {
        if (EditorTools.IsActiveTool(this))
        {
            m_IconContent = new GUIContent()
            {
                image = m_ToolIcon,
                text = "Physics Object Placer",
                tooltip = "Physics Object Placer"
            };

            targetTool = GameObject.Find(ToolName);

            if (targetTool == null)
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Click a spawn position for the tool"), 2f);

                isInit = true;

            }
            else
            {
                Selection.activeGameObject = targetTool;
            }

            SceneView.lastActiveSceneView.FrameSelected();
        }
        else
        {

        }
    }


    private void OnEnable()
    {
        

        EditorTools.activeToolChanged += OnActiveToolChanged;
    }

    private void OnDisable()
    {
        EditorTools.activeToolChanged -= OnActiveToolChanged;
    }




    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }
    

    public override void OnToolGUI(EditorWindow window)
    {

        
        if (Event.current.keyCode == KeyCode.Escape)
        {
            isInit = false;
            //Event.current.Use();

            Selection.activeGameObject = null;
            Tools.current = Tool.None;

        }


        if (isInit)
        {
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(1);


            RaycastHit cHit;

            Ray cRay = SceneView.lastActiveSceneView.camera.ScreenPointToRay(HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition));

            Vector3 contactPoint = new Vector3(0f, 0f, 0f);


            



            if (Physics.Raycast(cRay, out cHit, Mathf.Infinity))
            {
                contactPoint = cHit.point;
            }



            //if (isInit && Event.current.type == EventType.MouseUp)
            if (Event.current.type == EventType.MouseUp)
            {

                targetTool = new GameObject(ToolName);

                PhysicsObjectPlacer comp = targetTool.AddComponent<PhysicsObjectPlacer>();

                comp.DragAndDropObjects = new List<GameObject>();

                targetTool.transform.position = cHit.point;

                targetTool.transform.Translate(0f, 3.5f, 0f);

                Selection.activeGameObject = targetTool;

                SceneView.lastActiveSceneView.FrameSelected();

                isInit = false;

                Event.current.Use();


            }
            //else if (isInit && Event.current.type == EventType.MouseMove)
            else if (Event.current.type == EventType.MouseMove)
            {
                Debug.DrawRay(contactPoint, Vector3.up * 3f, Color.green);

                Event.current.Use();
            }
            //else if (isInit && Event.current.type == EventType.Layout)
            else if (Event.current.type == EventType.Layout)
            {
                Debug.DrawRay(contactPoint, Vector3.up * 3f, Color.green);
            }

             
            //if (isInit)
            //{
                Handles.color = Color.green;
                Handles.DrawWireDisc(contactPoint, cHit.normal, 2f);

            //Handles.BeginGUI();

            //GUI.DrawTexture()

            //Handles.EndGUI();
            //}

        }

        if (!isInit)
        {

            //EditorTools.SetActiveTool<PhysicsObjectPlacerTool>();

            if (targetTool != null)
            {
                targetTool.transform.position = Handles.PositionHandle(targetTool.transform.position, targetTool.transform.rotation);
            }

            
            Handles.BeginGUI();

            GUILayout.BeginVertical();

            //GUI.contentColor = Color.cyan;
            GUILayout.Label("Physics Object Placer : On");
            GUILayout.Label("Press \"ESC\" to exit");

            GUILayout.EndVertical();


            Handles.EndGUI();
            
        }
    }
}
