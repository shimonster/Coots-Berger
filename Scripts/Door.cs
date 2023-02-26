using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Transform particleObject;

    void Start()
    {
        InitPosition();
    }

    public void InitPosition()
    {
        RaycastHit groundHit;
        Physics.Raycast(transform.position + Vector3.up * 10000, Vector3.down, out groundHit, 100000, 1 << 8);

        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, groundHit.normal), groundHit.normal);
        transform.position = groundHit.point + transform.up * 1.5f;
        particleObject.rotation = Quaternion.identity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            FindObjectOfType<GameManager>().FinishLevel();
        }
    }
}
