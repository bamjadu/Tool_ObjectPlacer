
//#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEditor;
using UnityEditor.EditorTools;

using System.IO;

//[System.Serializable]
[ExecuteInEditMode]
public class PhysicsObjectPlacer : MonoBehaviour
{

    Ray raycast;
    RaycastHit hit;
    bool isHit;
    float hitDistance;

    [Range(0.1f,3.0f)]
    public float Radius = 0.5f;

    public GameObject MyObject;
    public GameObject MyObjectClone;
    public List<GameObject> DragAndDropObjects = new List<GameObject>();

    public bool Activate = false;

    [Range(0.01f, 1.0f)]
    public float SpawnTime = 1f;


    [Range(0.1f, 1000f)]
    public float MaxRaycastDistance = 20f;


    float Timer;
    bool OutOfArea;
    public GameObject Holder;

    public int RandomValue;
    public int MaxRandomValue;

    //-----------------------------------------------------------

    void Awake()
    {

        if (Application.isPlaying)
        {
            Destroy(this);
        }
        Timer = SpawnTime;

        DragAndDropObjects = new List<GameObject>();
        DragAndDropObjects.Clear();
    }


    private void OnDestroy()
    {
        if (Tools.current == Tool.Custom)
        {
            Tools.current = Tool.None;
        }
    }


    void Update()
    {

        if (!Application.isEditor)
        {
            Destroy(this);
            return;
        }

        raycast = new Ray(transform.position, -transform.up * MaxRaycastDistance);
        isHit = false;

        int layerMask = 1 << 10;
        layerMask =~layerMask;

        if (Physics.Raycast(raycast, out hit, MaxRaycastDistance, layerMask))
        {
            isHit = true;
            hitDistance = Vector3.Distance(transform.position, hit.point);
        }

        if (Activate)
        {
            if(GameObject.Find("Holder") == null)
            {
                Holder = new GameObject("Holder");
            }
            else
            {
                Holder = GameObject.Find("Holder");
            }

            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                if(!OutOfArea)
                {
                    Spawn();
                    Timer = SpawnTime;
                }
            }
        }
        else
        {
            if (hitDistance < MaxRaycastDistance)
            {
                hitDistance++;
            }
        }
        StepPhysics();
    }

    private bool IsRigidbody(GameObject target)
    {
        if (target == null)
            return false;

        if (target.GetComponentsInChildren<Rigidbody>().Length == 0)
            return false;

        return true;
    }

    private bool HasCollider(GameObject target)
    {
        if (target == null)
            return false;

        if (target.GetComponentsInChildren<Collider>().Length == 0)
            return false;

        return true;
    }


    private GameObject AddRigidbodyComponent(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is null");
            return null;
        }

        Rigidbody targetRigidBody = targetObject.AddComponent<Rigidbody>();

        targetRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //targetRigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        return targetObject;
    }

    private GameObject AddColliderComponent(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is null");
            return null;
        }

        MeshFilter[] mFilters = targetObject.GetComponentsInChildren<MeshFilter>();

        List<Mesh> addedMeshes = new List<Mesh>();

        foreach(MeshFilter meshFilter in mFilters)
        {
            if (!addedMeshes.Contains(meshFilter.sharedMesh))
            {
                MeshCollider currentCollider = targetObject.AddComponent<MeshCollider>();
                currentCollider.convex = true;
                currentCollider.sharedMesh = meshFilter.sharedMesh;
                addedMeshes.Add(meshFilter.sharedMesh);
            }
        }

        addedMeshes.Clear();

        return targetObject;
    }

    private GameObject EnableConvex(GameObject targetObject)
    {

        if (targetObject == null)
        {
            Debug.LogError("Target object is null");
            return null;
        }

        MeshCollider[] cols = targetObject.GetComponentsInChildren<MeshCollider>();

        foreach(MeshCollider col in cols)
        {
            col.convex = true;
        }

        return targetObject;
    }



    private void Spawn()
    {

        Vector3 SpawnPosition = Random.insideUnitSphere * Radius + transform.position;

        GameObject targetObject = DragAndDropObjects[Random.Range(0, MaxRandomValue)];

        

        //MyObjectClone = Instantiate(DragAndDropObjects[Random.Range(0, MaxRandomValue)], new Vector3(SpawnPosition.x, this.transform.position.y, SpawnPosition.z), Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f))) as GameObject;
        MyObjectClone = Instantiate(targetObject, new Vector3(SpawnPosition.x, this.transform.position.y, SpawnPosition.z), Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f))) as GameObject;

        if (IsRigidbody(MyObjectClone) == false)
        {
            MyObjectClone = AddRigidbodyComponent(MyObjectClone);
        }

        if (HasCollider(MyObjectClone) == false)
        {
            MyObjectClone = AddColliderComponent(MyObjectClone);
        }
        else
        {
            MyObjectClone = EnableConvex(MyObjectClone);
        }


        //MyObjectClone.gameObject.layer = LayerMask.NameToLayer("Ignore");
        MyObjectClone.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        MyObjectClone.AddComponent<Destroy>();
        MyObjectClone.transform.parent = Holder.gameObject.transform;

    }

    private void StepPhysics()
    {
        Physics.autoSimulation = false;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.autoSimulation = true;
    }

#if UNITY_EDITOR



    private void OnDrawGizmosSelected()
    {
        
        

        if (isHit)
        {
            OutOfArea = false;
            Debug.DrawRay(transform.position, -transform.up * Vector3.Distance(transform.position, hit.point), Color.blue);
            Debug.DrawRay(hit.point, -transform.up * Vector3.Distance(transform.position - new Vector3(0f, MaxRaycastDistance, 0f), hit.point), Color.red);

            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.DrawWireDisc(hit.point, -transform.up, 0.025f);
            //UnityEditor.Handles.DrawWireDisc(hit.point, -transform.up, Radius);
            UnityEditor.Handles.DrawWireDisc(hit.point, hit.normal, Radius);

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(transform.position - new Vector3(0f, MaxRaycastDistance, 0f), -transform.up, Radius * 2f);

        }
        else
        {
            OutOfArea = true;
            GUIStyle ErrorStyle = new GUIStyle("Box");
            ErrorStyle.alignment = TextAnchor.MiddleCenter;
            ErrorStyle.normal.textColor = Color.red;
            

            UnityEditor.Handles.Label((transform.position + new Vector3(0f, 2.5f, 0f)), "No collision Detected", ErrorStyle);
        }
    }



    string GetGizmo()
    {
        string gizmoPath = "IconPlacement";

        string packagePath = "Packages/com.unity.production.objectplacer";

        if (Directory.Exists(Path.GetFullPath(packagePath)))
        {
            gizmoPath = string.Format("{0}/Gizmos/{1}.png", packagePath, gizmoPath);
        }

        return gizmoPath;
    }




    private void OnDrawGizmos()
    {

        //Gizmos.DrawIcon(this.transform.position, "IconPlacement");
        Gizmos.DrawIcon(this.transform.position, GetGizmo());

    }

#endif

    void CleanRigidBodyAndCollision()
    {
        if (Holder != null)
        {
            Rigidbody[] rBodies = Holder.GetComponentsInChildren<Rigidbody>();

            for (int i=0;i<rBodies.Length;i++)
            {
                DestroyImmediate(rBodies[i]);
            }

            Collider[] cols = Holder.GetComponentsInChildren<Collider>();

            for (int i = 0; i < cols.Length; i++)
            {
                DestroyImmediate(cols[i]);
            }

        }
    }

    public void BakeMeshes()
    {

        CleanRigidBodyAndCollision();



        // Need to think about "LOD Group" //

        Dictionary<Material, List<MeshFilter>> renderSet = new Dictionary<Material, List<MeshFilter>>();

        if (Holder != null)
        {
            Renderer[] rends = Holder.GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in rends)
            {
                MeshFilter mFilter = rend.gameObject.GetComponent<MeshFilter>();

                if (mFilter != null)
                {
                    if (renderSet.ContainsKey(rend.sharedMaterial) == false)
                    {
                        List<MeshFilter> mFilterList = new List<MeshFilter>();
                        mFilterList.Add(mFilter);
                        renderSet.Add(rend.sharedMaterial, mFilterList);
                    }
                    else
                    {
                        renderSet[rend.sharedMaterial].Add(mFilter);
                    }
                }
            }

            foreach(KeyValuePair<Material, List<MeshFilter>> pair in renderSet)
            {
                Debug.Log(pair.Key.name);

                foreach(MeshFilter mf in pair.Value)
                {
                    Debug.Log("\t" + mf.name);
                }

                MeshFilter[] meshFilter = pair.Value.ToArray();
                CombineInstance[] combiners = new CombineInstance[meshFilter.Length];

                for (int a = 0; a < meshFilter.Length; a++)
                {
                    //combiners[a].subMeshIndex = 0;
                    combiners[a].mesh = meshFilter[a].sharedMesh;
                    combiners[a].transform = meshFilter[a].transform.localToWorldMatrix;
                }

                Mesh FinalMesh = new Mesh();
                FinalMesh.name = "BakedMesh_" + pair.Key.name;
                FinalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                FinalMesh.CombineMeshes(combiners);
                FinalMesh.RecalculateBounds();
                FinalMesh.RecalculateNormals();
                UnityEditor.MeshUtility.Optimize(FinalMesh);
                FinalMesh.Optimize();

                //GameObject CombinedObject = new GameObject(meshFilter[1].name);
                GameObject CombinedObject = new GameObject("BakedMesh_" + pair.Key.name);

                CombinedObject.AddComponent<MeshRenderer>();
                CombinedObject.AddComponent<MeshFilter>();
                CombinedObject.GetComponent<MeshFilter>().sharedMesh = FinalMesh;
                CombinedObject.GetComponent<MeshRenderer>().material = pair.Key;
                //CombinedObject.GetComponent<MeshRenderer>().material = meshFilter[1].GetComponent<MeshRenderer>().sharedMaterial;

                Holder.SetActive(false);
                Debug.Log(meshFilter.Length + " Meshes combined!");



            }

            /*
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
            */
        }
    }

    public void CombineMeshes()
    {
        MeshFilter[] meshFilter = Holder.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combiners = new CombineInstance[meshFilter.Length];

        for (int a=0; a < meshFilter.Length; a++)
        {
            //combiners[a].subMeshIndex = 0;
            combiners[a].mesh = meshFilter[a].sharedMesh;
            combiners[a].transform = meshFilter[a].transform.localToWorldMatrix;
        }

        GameObject newMesh = new GameObject(MyObject.name + "Combined");

        newMesh.AddComponent<MeshFilter>();
        newMesh.AddComponent<MeshRenderer>();
        newMesh.GetComponent<MeshRenderer>().material = MyObject.GetComponent<MeshRenderer>().sharedMaterial;

        newMesh.GetComponent<MeshFilter>().mesh = new Mesh();


        
        //Mesh finalMesh = new Mesh();
        //newMesh.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        newMesh.GetComponent<MeshFilter>().mesh.CombineMeshes(combiners);
        newMesh.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        newMesh.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        newMesh.GetComponent<MeshFilter>().mesh.Optimize();

        DestroyImmediate(Holder);
        Debug.Log(meshFilter.Length + " Meshes combined!");
    }
}

//#endif