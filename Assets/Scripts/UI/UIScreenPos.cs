using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenPos : MonoBehaviour
{
    public float followSpeed = 10.0f;
    public Transform target;
    public Vector3 offset = Vector3.zero;
    private Vector3 followTargetPos;

    void Start()
    {
        followTargetPos = target.position + offset;
    }
    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();
        followTargetPos = Vector3.MoveTowards(followTargetPos, target.position + offset, followSpeed*Time.deltaTime);
        rect.position = Camera.main.WorldToScreenPoint(followTargetPos + offset) ;        
    }
}
