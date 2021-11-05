using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMe : MonoBehaviour
{
    [SerializeField] private float Speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;
        var position = transform.position;
        transform.position = new Vector3(position.x + Speed * deltaTime, position.y,
            position.z);
    }
}
