using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Transform spawnPoint;
    public float moveDelay = 0.5f;
    public float lookSpeed = 1.5f;
    public float viewMargin = 10.0f;
    public Vector3 targetPos = Vector3.zero;
    private List<Transform> targets = new List<Transform>();
    private Vector3 currentCameraVelocity = Vector3.zero;

    public void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players){
            this.targets.Add(player.transform);
        }
    }

    public void Start()
    {
        Reset();
    }

    public void Reset()
    {
        this.targetPos = this.spawnPoint.position;
    }

    private void Update()
    {
        if(this.targets.Count > 0)
        {
            float tanFov = Mathf.Tan(Mathf.Deg2Rad * GetComponent<Camera>().fieldOfView / 2.0f);
            Vector3 minPos = targets[0].position;
            Vector3 maxPos = targets[0].position;

            foreach(Transform target in this.targets){
                maxPos = Vector3.Max(maxPos, target.position);
                minPos = Vector3.Min(minPos, target.position);
            }
            maxPos += Vector3.one*viewMargin;
            minPos -= Vector3.one*viewMargin;

            Vector3 spread = (maxPos - minPos);
            Vector3 targetLook = minPos + spread*0.5f;
            float cameraDistance = (0.5f*spread.magnitude/GetComponent<Camera>().aspect) / tanFov;
            Vector3 cameraDirection = (targetLook - spawnPoint.position).normalized;

            this.targetPos = targetLook - cameraDirection * cameraDistance;
            this.transform.RotateAround(targetLook, Vector3.up, Time.deltaTime*lookSpeed);

            this.targetPos = (transform.position - targetLook).normalized * cameraDistance + targetLook;

            this.transform.position = Vector3.SmoothDamp(
                this.transform.position, 
                this.targetPos, 
                ref this.currentCameraVelocity, 
                this.moveDelay
            );
        
            this.transform.rotation = Quaternion.Slerp(
                this.transform.rotation, 
                Quaternion.LookRotation(targetLook - this.transform.position), 
                Time.deltaTime*this.lookSpeed
            );            
        }
    }
}