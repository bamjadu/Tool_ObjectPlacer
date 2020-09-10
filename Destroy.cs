using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Destroy : MonoBehaviour
{
    private Rigidbody MyRigidbody;
    float TimeToDestroy = 7f;

    void Start()
    {
        MyRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

       // Physics.Simulate(Time.fixedDeltaTime);
       // Physics.autoSimulation = true;

        TimeToDestroy -= Time.deltaTime;

        if (TimeToDestroy < 0f)
        {
            if (MyRigidbody.velocity.magnitude > 0.2f)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                //DestroyImmediate(gameObject.GetComponent<Rigidbody>());
                //DestroyImmediate(gameObject.GetComponent<Collider>());
                DestroyImmediate(this);
            }
        }
    }
}
