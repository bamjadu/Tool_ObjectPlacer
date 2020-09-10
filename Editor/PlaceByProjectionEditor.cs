using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PlaceByProjection))]

public class PlaceByProjectionEditor : Editor
{

    /*
    private void OnSceneGUI()
    {

    }
    */


    public override void OnInspectorGUI()
    {
        PlaceByProjection PlaceByProjection = (PlaceByProjection)target;

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        //EditorGUILayout.LabelField("Projection Size", EditorStyles.boldLabel);
        //PlaceByProjection.ProjectionSize = EditorGUILayout.Vector2Field("",PlaceByProjection.ProjectionSize);

        EditorGUILayout.LabelField("Layer Mask", EditorStyles.boldLabel);
        PlaceByProjection.layerMask = EditorGUILayout.LayerField(PlaceByProjection.layerMask);

        EditorGUILayout.LabelField("Position Max Offset", EditorStyles.boldLabel);
        PlaceByProjection.OffsetMax = EditorGUILayout.Slider(PlaceByProjection.OffsetMax, 0f, 1f);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Distance",EditorStyles.boldLabel);
        PlaceByProjection.Distance = EditorGUILayout.Slider(PlaceByProjection.Distance, 0.01f, 1f);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Border", EditorStyles.boldLabel);
        PlaceByProjection.Border = EditorGUILayout.Slider(PlaceByProjection.Border, 0.01f,1f);

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Random Y Rotation", EditorStyles.boldLabel);
        PlaceByProjection.RandomY = EditorGUILayout.Toggle("", PlaceByProjection.RandomY);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Random XZ Rotation", EditorStyles.boldLabel);
        PlaceByProjection.RandomXZ = EditorGUILayout.Toggle("", PlaceByProjection.RandomXZ);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Random Y Scale", EditorStyles.boldLabel);
        PlaceByProjection.RandomYScale = EditorGUILayout.Toggle("", PlaceByProjection.RandomYScale);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Random Seed", EditorStyles.boldLabel);
        PlaceByProjection.seed = EditorGUILayout.IntSlider(PlaceByProjection.seed, 1 , 10000);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Normal Direction", EditorStyles.boldLabel);
        PlaceByProjection.NormalDirection = EditorGUILayout.Toggle("", PlaceByProjection.NormalDirection);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Calculate", EditorStyles.boldLabel);
        PlaceByProjection.Calculate = EditorGUILayout.Toggle("", PlaceByProjection.Calculate);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        Event evt = Event.current;

        GUIStyle BoxStyle = new GUIStyle(GUI.skin.box);
        BoxStyle.alignment = TextAnchor.MiddleCenter;
        BoxStyle.fontStyle = FontStyle.Italic;
        BoxStyle.fontSize = 12;
        GUI.skin.box = BoxStyle;

        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 75.0f, GUILayout.ExpandWidth(true));

        GUI.Box(drop_area, "Drag Prefabs here!", BoxStyle);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        switch (evt.type)
        {

            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (PlaceByProjection.ReplaceGameObject.Count != 0 && PlaceByProjection.TotalListTexture.Count != 0)
                    PlaceByProjection.ReplaceGameObject.Clear();
                    PlaceByProjection.TotalListTexture.Clear();

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        PlaceByProjection.ReplaceGameObject.Add(DragAndDrop.objectReferences[i] as GameObject);
                        PlaceByProjection.TotalListTexture.Add(AssetPreview.GetAssetPreview(DragAndDrop.objectReferences[i]));
                    }
                }
                break;
        }

        Rect boxRect = EditorGUILayout.BeginVertical();
        GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

        PlaceByProjection.Index = GUILayout.SelectionGrid(PlaceByProjection.Index, PlaceByProjection.TotalListTexture.ToArray(), 6);

        EditorGUILayout.EndVertical();
        GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

        EditorGUILayout.Separator();

        if (GUILayout.Button("Create"))
        {
            //PlaceByProjection.Calculate = false;
            PlaceByProjection.Remove();
            PlaceByProjection.Place();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Remove"))
        {
            PlaceByProjection.Remove();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Bake Meshes"))
        {
            PlaceByProjection.BakeMesh();
        }

        EditorGUILayout.Separator();

        //if (GUILayout.Button("Bake Meshes by materials"))
        //{
        //    CastRayBulk.BakeMeshByMaterials();
        //}

        if (GUI.changed || cachePosition != PlaceByProjection.transform.position || cacheScale != PlaceByProjection.transform.localScale || PlaceByProjection.transform.rotation.eulerAngles != cacheRotation)
        {
            PlaceByProjection.UpdateRaycast();
        }

        cachePosition = PlaceByProjection.transform.position;
        cacheScale = PlaceByProjection.transform.localScale;
        cacheRotation = PlaceByProjection.transform.rotation.eulerAngles;

        this.Repaint();

        EditorGUILayout.Separator();
    }

    Vector3 cachePosition;
    Vector3 cacheScale;
    Vector3 cacheRotation;

}
