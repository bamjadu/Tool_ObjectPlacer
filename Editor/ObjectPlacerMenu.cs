
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.EditorTools;

namespace TemplateTools
{
    public class ObjectPlacerMenu
    {
        [MenuItem("Template/Object Placement/Run \"Physics Object Placer\"", priority = 100)]
        static void OnActivatePhysicsObjectPlacer()
        {
            EditorTools.SetActiveTool<PhysicsObjectPlacerTool>();
        }

        [MenuItem("Template/Object Placement/Run \"Place By Projection\"", priority = 130)]
        static void OnActivatePlaceByProjection()
        {
            EditorTools.SetActiveTool<PlaceByProjectionTool>();
        }

    }
}

#endif
