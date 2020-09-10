using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.EditorTools;



[EditorTool("Projection Driven Object Placer")]
public class PlaceByProjectionTool : EditorTool
{

    [SerializeField]
    public Texture2D m_ToolIcon;

    const string ToolName = "ProjectionDrivenObjectPlacer";

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
                text = "Projection Driven Object Placer",
                tooltip = "Projection Driven Object Placer"
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

            Selection.activeGameObject = null;
            Tools.current = Tool.None;
        }


        if (isInit)
        {
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(2);


            RaycastHit cHit;

            Ray cRay = SceneView.lastActiveSceneView.camera.ScreenPointToRay(HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition));

            Vector3 contactPoint = new Vector3(0f, 0f, 0f);

            if (Physics.Raycast(cRay, out cHit, Mathf.Infinity))
            {
                contactPoint = cHit.point;
            }


            if (Event.current.type == EventType.MouseUp)
            {

                targetTool = new GameObject(ToolName);

                PlaceByProjection comp = targetTool.AddComponent<PlaceByProjection>();

                comp.Calculate = true;

                targetTool.transform.position = cHit.point;

                targetTool.transform.Translate(0f, 3.5f, 0f);

                targetTool.transform.up = cHit.normal;


                Selection.activeGameObject = targetTool;

                SceneView.lastActiveSceneView.FrameSelected();

                isInit = false;

                Event.current.Use();

            }
            else if (Event.current.type == EventType.MouseMove)
            {
                Debug.DrawRay(contactPoint, cHit.normal * 3f, Color.green);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.Layout)
            {
                Debug.DrawRay(contactPoint, cHit.normal * 3f, Color.green);
            }

            Handles.color = Color.green;
            Handles.DrawWireDisc(contactPoint, cHit.normal, 2f);

        }
        
    }
}
