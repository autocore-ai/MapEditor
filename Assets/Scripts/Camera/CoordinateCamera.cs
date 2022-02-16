using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateCamera : MonoBehaviour
{
    public Transform mTransform;
    public Transform targetTransform;
    public Transform mainCamTransform;
    public float dis2Target=5;
    // Start is called before the first frame update
    void Start()
    {
        mTransform = transform;
        mainCamTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        mTransform.rotation = mainCamTransform.rotation;
        mTransform.position = targetTransform.position-mainCamTransform.forward * dis2Target;
    }
}
