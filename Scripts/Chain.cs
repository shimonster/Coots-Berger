using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    public Vector3 start;
    public Transform target;
    public float length;
    float segmentLength
    {
        get
        {
            return length / (balls.Count - 1);
        }
    }

    LineRenderer chainRenderer;

    [SerializeField] List<GameObject> balls;
    [SerializeField] int iterations;

    private void Start()
    {
        chainRenderer = GetComponent<LineRenderer>();
        chainRenderer.positionCount = balls.Count;
    }

    void Update()
    {
        Vector3[] positions = new Vector3[balls.Count];
        int l = 0;
        foreach (GameObject ball in balls)
        {
            positions[l] = ball.transform.position;
            l++;
        }

        for (int i = 0; i < iterations; i++)
        {
            for (int j = balls.Count - 1; j > 0; j--)
            {
                if (j == balls.Count - 1)
                {
                    positions[j] = target.position;
                }
                else
                {
                    Vector3 offset = positions[j] - positions[j + 1];
                    if (offset.sqrMagnitude > segmentLength * segmentLength)
                    {
                        positions[j] = positions[j + 1] + offset.normalized * segmentLength;
                    }
                }
            }

            for (int j = 0; j < balls.Count; j++)
            {
                if (j == 0)
                {
                    positions[j] = start;
                }
                else
                {
                    Vector3 offset = positions[j] - positions[j - 1];
                    if (offset.sqrMagnitude > segmentLength * segmentLength)
                    {
                        positions[j] = positions[j - 1] + offset.normalized * segmentLength;
                    }
                }
            }
        }

        for (int i = 0; i < balls.Count; i++)
        {
            RaycastHit groundPos;
            Physics.Raycast(positions[i] + Vector3.up * 500, Vector3.down, out groundPos, 1000, 1 << 8);
            Debug.DrawLine(positions[i] + Vector3.up * 500, groundPos.point, Color.red);
            if (groundPos.point.y > positions[i].y)
                balls[i].transform.position = groundPos.point + Vector3.up * 0.5f;
            else
                balls[i].transform.position = positions[i] + Vector3.up * 0.5f;

            chainRenderer.SetPosition(i, balls[i].transform.position);
        }
    }
}
