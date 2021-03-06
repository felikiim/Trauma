﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class EnemyCulpabilite : MonoBehaviour
{
    //Detection
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask playerLayer;
    public LayerMask groundLayer;

    public Material[] mats;

    //Rotation
    public float timeBetweenRotation;
    public float tinyTiming;
    float timer;
    float tinyTimer;
    public int state1;
    public int state2;
    public int state3;
    bool state1activ;
    bool state2activ;
    bool state3activ;
    bool state1inactiv;
    bool state3inactiv;
    bool changeDone;
    public List<Transform> visibleTargets = new List<Transform>();
    public float meshResolution;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;
    public MeshRenderer viewMeshRenderer;

    private void Start()
    {
        state1activ = true;
        changeDone = false;
        transform.rotation = Quaternion.Euler(0f, 0f, state1);
        StartCoroutine("FindTargetWithDelay", .2f);
        viewMesh = new Mesh();
        viewMesh.name = " View Mesh";
        viewMeshFilter.mesh = viewMesh;
        viewMeshRenderer.material = mats[0];
    }
    private void Update()
    {
        DrawFieldOfView();
        timer -= Time.deltaTime;
        tinyTimer -= Time.deltaTime;

        if (timer < 0)
        {
            ChangeEyeState();
        }
        if(tinyTimer < 0)
        {
            changeDone = false;
        }
    }

    IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTarget();
        }
    }

    void FindVisibleTarget()
    {
        visibleTargets.Clear();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, playerLayer);
        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            if (Vector2.Angle(transform.up, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                if(Physics.Raycast(transform.position,dirToTarget,distanceToTarget,groundLayer) == false)
                {
                    viewMeshRenderer.material = mats[1];
                    target.GetComponent<Controller2D>().isDead = true;
                    visibleTargets.Add(target);
                    Debug.Log("t mort salope");
                }
                else
                {
                    viewMeshRenderer.material = mats[0];
                }
            }
        }
    }

    void DrawFieldOfView()
    {
        int stepCount =Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for(int i = 0; i<= stepCount; i++)
        {
            float angle = -transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount-1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, groundLayer))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees,bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees -= transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad),0f);
    }

    private void ChangeEyeState()
    {
        if (state1activ == true && changeDone == false)
        {
            timer = timeBetweenRotation;
            transform.rotation = Quaternion.Euler(0f, 0f, state2);
            state1inactiv = true;
            state2activ = true;
            state1activ = false;
            changeDone = true;
            tinyTimer = tinyTiming;
        }
        if (state3activ == true && changeDone == false)
        {
            timer = timeBetweenRotation;
            transform.rotation = Quaternion.Euler(0f, 0f, state2);
            state3inactiv = true;
            state2activ = true;
            state3activ = false;
            changeDone = true;
            tinyTimer = tinyTiming;
        }
        if (state2activ == true && changeDone == false)
        {
            if (state1inactiv == true && changeDone == false)
            {
                timer = timeBetweenRotation;
                transform.rotation = Quaternion.Euler(0f, 0f, state3);
                state1inactiv = false;
                state3activ = true;
                state2activ = false;
                changeDone = true;
                tinyTimer = tinyTiming;
            }
            if (state3inactiv == true && changeDone == false)
            {
                timer = timeBetweenRotation;
                transform.rotation = Quaternion.Euler(0f, 0f, state1);
                state3inactiv = false;
                state1activ = true;
                state2activ = false;
                changeDone = true;
                tinyTimer = tinyTiming;
            }
        }
    }
}
