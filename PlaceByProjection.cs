using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;

[ExecuteInEditMode]
public class PlaceByProjection : MonoBehaviour
{
    Vector2 RaysXZ;

    [Range(0.01f, 1f)]
    public float Distance = 0.2f;

    [Range(0.01f, 15f)]
    public float Border = 0.2f;

    public int layerMask;

    public float Z;
    public List<GameObject> TotalList = new List<GameObject>(0);
    public List<Texture2D> TotalListTexture = new List<Texture2D>(0);

    public List<GameObject> ReplaceGameObject = new List<GameObject>(0);
    public int Index;

    Ray raycast;
    Ray raycastForward;
    Ray raycastBack;
    Ray raycastRight;
    Ray raycastLeft;

    RaycastHit hit;
    RaycastHit hitForward;
    RaycastHit hitBack;
    RaycastHit hitRight;
    RaycastHit hitLeft;

    bool isHit;
    bool isHitForward;
    bool isHitBack;
    bool isHitLeft;
    bool isHitRight;

    float hitDistance;
    float hitDistanceForward;
    float hitDistanceBack;
    float hitDistanceRight;
    float hitDistanceLeft;

    public List<GameObject> ChildGameObject;

    [Range(0.01f, 0.5f)]
    public float doubleCheckRadius = 0.2f;

    int GrassLayerMask = 11;

    private GameObject Holder;
    public bool RandomY;
    public bool RandomXZ;
    public bool RandomYScale;

    public bool NormalDirection;
    public bool Calculate;

    private GameObject Preview;

    [Range(0f, 1f)]
    public float OffsetMax = 0f;

    private float RandomOffsetX;
    private float RandomOffsetZ;

    [SerializeField]
    private GameObject CombinedObject;

    public int seed = 12345;

    public Vector2 ProjectionSize = Vector2.one;



    void Awake()
    {
        if (!Application.isEditor)
            Destroy(this);

        if (ReplaceGameObject.Count != 0 && TotalListTexture.Count != 0)
            ReplaceGameObject.Clear();
            TotalListTexture.Clear();
    }

    public void Place()
    {
        if (ReplaceGameObject.Count != 0 && TotalListTexture.Count != 0)
        {
            for (int i = 0; i < hitPoints.Count; i++)
            {
                CR(i);
            }
        }
        else
        {
            Debug.Log("No objects to be placed");
        }
    }
    
    float RandomRange (float min, float max, float key)
    {
        float offset = Mathf.Lerp(min, max, Mathf.Sin(seed*(key*100)));
        return offset;
    }

    int RandomRange(int min, int max, float key)
    {
        float offset = Mathf.Lerp(min, max-1, Mathf.Sin(seed*(key*100)));
        return Mathf.Clamp((int)offset, min, max-1);
    }



    public void CR(int index)
    {
        float randomseedKey = hitPoints[index].magnitude;

        RandomOffsetX = RandomRange(-OffsetMax, OffsetMax, randomseedKey);
        RandomOffsetZ = RandomRange(-OffsetMax, OffsetMax, randomseedKey);

        if (NormalDirection)
        {
            Preview = Instantiate(ReplaceGameObject[RandomRange(0, ReplaceGameObject.Count, randomseedKey)] as GameObject, hitPoints[index], Quaternion.FromToRotation(Vector3.up, normalDirections[index]));
        }
        else
        {
            Preview = Instantiate(ReplaceGameObject[RandomRange(0, ReplaceGameObject.Count, randomseedKey)] as GameObject, hitPoints[index], Quaternion.identity);
        }

        if (RandomY)
        {
            Preview.transform.rotation *= Quaternion.AngleAxis(RandomRange(-180f, 180f, randomseedKey), Vector3.up);
        }

        if (RandomXZ)
        {
            Preview.transform.rotation *= Quaternion.AngleAxis(RandomRange(-20f, 20f, randomseedKey), Vector3.forward);
            Preview.transform.rotation *= Quaternion.AngleAxis(RandomRange(-20f, 20f, randomseedKey), Vector3.back);
            Preview.transform.rotation *= Quaternion.AngleAxis(RandomRange(-20f, 20f, randomseedKey), Vector3.right);
            Preview.transform.rotation *= Quaternion.AngleAxis(RandomRange(-20f, 20f, randomseedKey), Vector3.left);
        }

        if (RandomYScale)
        {
            Preview.transform.localScale = new Vector3(Preview.transform.localScale.x, RandomRange(Preview.transform.localScale.y - 0.5f, Preview.transform.localScale.y + 0.5f, randomseedKey), Preview.transform.localScale.z);
        }

        if (Holder == null)
        {
            Holder = new GameObject("GrassHolder");
        }
        Holder.SetActive(true);

        Preview.name = Preview.name.Replace("(Clone)", "");
        Preview.transform.parent = Holder.transform;
    }

    List<Vector3> hitPoints = new List<Vector3>(0);
    List<Vector3> normalDirections = new List<Vector3>(0);

    [ContextMenu("Update Positions")]
    public void UpdateRaycast()
    {
        if (!Calculate) return;
        
        hitPoints.Clear();
        normalDirections.Clear();

        float xSteps = transform.localScale.x / Distance;
        float zSteps = transform.localScale.z / Distance;

        Vector3 bottomLeft = transform.position - (transform.forward * (transform.localScale.z*0.5f) + (transform.right * (transform.localScale.x * 0.5f)));
        Vector3 bottomRight = transform.position - (transform.forward * (transform.localScale.z * 0.5f) - (transform.right * (transform.localScale.x * 0.5f)));
        Vector3 topLeft = transform.position + (transform.forward * (transform.localScale.z * 0.5f) - (transform.right * (transform.localScale.x * 0.5f)));
        Vector3 topRight = transform.position + (transform.forward * (transform.localScale.z * 0.5f) + (transform.right * (transform.localScale.x * 0.5f)));

        LayerMask _layerMask = 1 << layerMask;

        for (int i = 0; i < (int)xSteps; i++)
        {
            Vector3 rowLeft = Vector3.Lerp(bottomLeft, topLeft, Distance * i / transform.localScale.x);
            Vector3 rowRight = Vector3.Lerp(bottomRight, topRight, Distance * i / transform.localScale.x);

            for (int a = 0; a < (int)zSteps; a++)
            {
                Vector3 position = Vector3.Lerp(rowLeft, rowRight, Distance * a / transform.localScale.z);


                RandomOffsetX = RandomRange(-OffsetMax, OffsetMax, position.magnitude);
                RandomOffsetZ = RandomRange(-OffsetMax, OffsetMax, position.magnitude);
                raycast = new Ray(position + new Vector3(RandomOffsetX, 0f, RandomOffsetZ), -transform.up * 50);

                if (Physics.Raycast(raycast, out hit, 50f, _layerMask))
                {
                    raycastForward = new Ray(hit.point + new Vector3(Border, 0.5f, 0f), -transform.up * 50);
                    raycastBack = new Ray(hit.point + new Vector3(-Border, 0.5f, 0f), -transform.up * 50);
                    raycastRight = new Ray(hit.point + new Vector3(0F, 0.5f, Border), -transform.up * 50);
                    raycastLeft = new Ray(hit.point + new Vector3(0F, 0.5f, -Border), -transform.up * 50);

                    if (Physics.Raycast(raycastForward, out hitForward, 50, _layerMask) && Physics.Raycast(raycastBack, out hitBack, 50, _layerMask) && Physics.Raycast(raycastRight, out hitRight, 50, _layerMask) && Physics.Raycast(raycastLeft, out hitLeft, 50, _layerMask))
                    {
                        hitPoints.Add(hit.point);
                        normalDirections.Add(hit.normal);
                    }
                }
            }
        }
    }

    

    private void OnDrawGizmosSelected()
    {
        if (hitPoints.Count == 0)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.blue;

        

        if (Calculate)
        {

            GameObject targetGo = null;

            if (ReplaceGameObject.Count != 0)
                targetGo = ReplaceGameObject[0];



            for (int i = 0; i < hitPoints.Count; i++)
            {

                if (NormalDirection)
                {
                    
                    if (ReplaceGameObject.Count != 0)
                        Gizmos.DrawWireMesh(targetGo.GetComponentInChildren<MeshFilter>().sharedMesh, hitPoints[i], Quaternion.FromToRotation(Vector3.up, normalDirections[i]));

                    Gizmos.DrawRay(hitPoints[i], normalDirections[i] * 0.2f);
                }
                else
                {
                    if (ReplaceGameObject.Count != 0)
                        Gizmos.DrawWireMesh(targetGo.GetComponentInChildren<MeshFilter>().sharedMesh, hitPoints[i], Quaternion.identity);

                    Gizmos.DrawRay(hitPoints[i], transform.up * 0.2f);
                }
            }
        }

        
    }


    string GetGizmo()
    {
        string gizmoPath = "IconProjection";

        string packagePath = "Packages/com.unity.production.objectplacer";

        if (Directory.Exists(Path.GetFullPath(packagePath)))
        {
            gizmoPath = string.Format("{0}/Gizmos/{1}.png", packagePath, gizmoPath);
        }

        return gizmoPath;
    }

    private void OnDrawGizmos()
    {
        if (hitPoints.Count == 0)
        {
            Gizmos.color = Color.red;
            Handles.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.blue;
            Handles.color = Color.blue;
        }


        float size = HandleUtility.GetHandleSize(transform.position);

        //Handles.color = Color.blue;
        transform.position = Handles.Slider(transform.position, -transform.up, size, Handles.ArrowHandleCap, 0f);

        //Handles.DrawWireDisc(PlaceByProjection.transform.position, PlaceByProjection.transform.up, 2f);


        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(ProjectionSize.x, 0, ProjectionSize.y));

        //Gizmos.DrawIcon(transform.position, "IconProjection");
        Gizmos.DrawIcon(transform.position, GetGizmo());


    }

    public void Remove()
    {
        if(Holder !=null)
        {
            DestroyImmediate(Holder);
        }
    }

    public void BakeMesh()
    {
#if UNITY_EDITOR
        if (Holder != null)
        {
            MeshFilter[] meshFilter = Holder.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combiners = new CombineInstance[meshFilter.Length];

            for (int a = 0; a < meshFilter.Length; a++)
            {
                //combiners[a].subMeshIndex = 0;
                combiners[a].mesh = meshFilter[a].sharedMesh;
                combiners[a].transform = meshFilter[a].transform.localToWorldMatrix;
            }

            Mesh FinalMesh = new Mesh();
            FinalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            FinalMesh.CombineMeshes(combiners);
            FinalMesh.RecalculateBounds();
            FinalMesh.RecalculateNormals();
            UnityEditor.MeshUtility.Optimize(FinalMesh);
            FinalMesh.Optimize();

            CombinedObject = new GameObject(meshFilter[1].name);

            CombinedObject.AddComponent<MeshRenderer>();
            CombinedObject.AddComponent<MeshFilter>();
            CombinedObject.GetComponent<MeshFilter>().sharedMesh = FinalMesh;
            CombinedObject.GetComponent<MeshRenderer>().material = meshFilter[1].GetComponent<MeshRenderer>().sharedMaterial;

            Holder.SetActive(false);
            Debug.Log(meshFilter.Length + " Meshes combined!");

        }
#endif
    }

    public void BakeMeshByMaterials()
    {
        if (Holder != null)
        {
        //    MeshFilter[] filters = Holder.GetComponentsInChildren<MeshFilter>(false);

        //    List<Material> materials = new List<Material>();
        //    MeshRenderer[] renderers = Holder.GetComponentsInChildren<MeshRenderer>(false);

        //    foreach (MeshRenderer renderer in renderers)
        //    {
        //        if (renderer.transform == transform)
        //            continue;
        //        Material[] localMats = renderer.sharedMaterials;
        //        foreach (Material localMat in localMats)
        //            if (!materials.Contains(localMat))
        //                materials.Add(localMat);
        //    }

        //    List<Mesh> submeshes = new List<Mesh>();
        //    foreach(Material material in materials)
        //    {
        //        List<CombineInstance> combiners = new List<CombineInstance>();

        //        foreach(MeshFilter filter in filters)
        //        {

        //            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
        //            if (renderer == null)
        //            {
        //                Debug.LogError(filter.name + " has no MeshRenderer");
        //                continue;
        //            }

        //            Material[] localMaterials = renderer.sharedMaterials;
        //            for (int mati = 0; mati < localMaterials.Length; mati++)
        //            {
        //                if (localMaterials[mati] != material)
        //                    continue;

        //                CombineInstance ci = new CombineInstance;
        //                ci.mesh = filter.sharedMesh;
        //                ci.subMeshIndex = mati;
        //                ci.transform = Matrix4x4.identity;
        //                combiners.Add(ci);
        //            }
        //            Mesh mesh = new Mesh();
        //            mesh.CombineMeshes(combiners.ToArray(), true);
        //            submeshes.Add(mesh);
        //        }
        //    }

        //    List <CombineInstance> finalCombiners = new List<CombineInstance>();

        //    foreach (Mesh mesh in submeshes)
        //    {
        //        CombineInstance ci = new CombineInstance();
        //        ci.mesh = mesh;
        //        ci.subMeshIndex = 0;
        //        ci.transform = Matrix4x4.identity;
        //        finalCombiners.Add(ci);
        //    }

        //    Mesh finalMesh = new Mesh();
        //    finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
        //    //myMeshFilter.sharedMesh = finalMesh;
        //    Debug.Log("Final mesh has " + submeshes.Count + " materials.");
        }
    }
}