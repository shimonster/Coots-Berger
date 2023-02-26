using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    enum AnimationState
    {
        Normal,
        Sliding
    }

    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    [SerializeField] float constantDeceleration;
    [SerializeField] float slideForce;
    [SerializeField] float turnSpeed;
    [SerializeField] float gravityCoeficiant;
    [SerializeField] float turnTowardsSpinDirectionSpeed;
    [SerializeField] float maxTurnTowardsVelocity;
    [SerializeField] float walkParticleEmission;

    [SerializeField] Chain chain;
    [SerializeField] Transform armatureTransform;
    [SerializeField] Transform chainEndMarkerTransform;
    [SerializeField] ParticleSystem walkParticles;
    [SerializeField] ParticleSystem dashParticles;

    Animator animator;
    Rigidbody rb;
    [SerializeField] AudioManager audioManager;

    Vector3 tieLocation;
    float distFromTie;
    float lastSpeed;
    Vector3 newVelocity;
    bool lockAnimState = false;

    float verticalInput;
    float horizontalInput;
    bool shouldSlide;
    bool isMousePressed;
    bool isInDoorRange;
    Transform nearestDoorTransform;
    Vector3 prevPosition;

    Vector3 newFlatVelocity
    {
        get
        {
            return new Vector3(newVelocity.x, 0, newVelocity.z);
        }
    }

    AnimationState animState
    {
        set
        {
            animStateValue = value;

            if (!lockAnimState)
            {
                switch (value)
                {
                    case AnimationState.Normal:
                        animator.SetBool("IsSliding", false);
                        break;
                    case AnimationState.Sliding:
                        animator.SetBool("IsSliding", true);
                        break;
                }
            }
        }
        get { return animStateValue; }
    }

    AnimationState animStateValue = AnimationState.Normal;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        chain.gameObject.SetActive(false);
        //audioManager = Resources.FindObjectsOfTypeAll<AudioManager>()[0];

        InitPosition();
    }

    public void InitPosition()
    {
        RaycastHit groundHit;
        Physics.Raycast(transform.position + Vector3.up * 10000, Vector3.down, out groundHit, 100000, 1 << 8);

        transform.position = groundHit.point + Vector3.up * 10;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;

        audioManager.StartLoopingSound(transform, "Walk");
        walkParticles.Clear();
    }

    private void Update()
    {
        if (!GameManager.isPaused)
        {
            armatureTransform.rotation = Quaternion.LookRotation(newFlatVelocity);
            animator.SetFloat("MoveSpeed", newVelocity.magnitude);

            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");
            isMousePressed = Input.GetMouseButton(0);
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                shouldSlide = true;
                animState = AnimationState.Sliding;
                animator.SetTrigger("Slide");
                lockAnimState = true;

                audioManager.PlaySound(this, transform, "Dash");

                StopAllCoroutines();
                StartCoroutine(StopSlideTimer());

                dashParticles.Play();
            }


            RaycastHit mouseRayHit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out mouseRayHit);

            if (Input.GetMouseButtonDown(0))
            {
                audioManager.PlaySound(this, transform, "Grapple");

                tieLocation = mouseRayHit.point;
                distFromTie = (transform.position - tieLocation).magnitude + 0.05f;

                chain.start = tieLocation;
                chain.length = distFromTie;
                chain.gameObject.SetActive(true);
            }

            if (Input.GetMouseButton(0))
            {
                chainEndMarkerTransform.position = tieLocation;
            }
            else
            {
                chainEndMarkerTransform.position = mouseRayHit.point;
                chain.gameObject.SetActive(false);
            }
        }

        var emission = walkParticles.emission;
        if (shouldSlide)
        {
            emission.SetBurst(0, new ParticleSystem.Burst(walkParticles.time, emission.GetBurst(0).count));
        }

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, 2))
        {
            emission.rateOverDistance = walkParticleEmission;
            print("grounded");
        }
        else
        {
            emission.rateOverDistance = walkParticleEmission / 100f;
            print("not grounded");
        }
    }

    private void FixedUpdate()
    {
        lastSpeed = rb.velocity.magnitude;
        newVelocity = rb.velocity;
        float initialYVelocity = newVelocity.y;

        prevPosition = transform.position;

        if (verticalInput == 1)
        {
            if (newVelocity.sqrMagnitude > 4)
                newVelocity /= constantDeceleration;
            newVelocity += transform.forward * acceleration / (lastSpeed * lastSpeed + 1);
        }
        else
        {
            newVelocity /= deceleration;
        }

        if (shouldSlide)
        {
            newVelocity += (transform.forward + Vector3.down) * slideForce / Mathf.Sqrt(newFlatVelocity.magnitude);
            shouldSlide = false;
        }

        Vector3 vectorToTie = tieLocation - (transform.position + newVelocity * Time.fixedDeltaTime);
        if (isMousePressed && vectorToTie.sqrMagnitude >= distFromTie * distFromTie)
        {
            float newFlatVelocityMagnitude = newFlatVelocity.magnitude;

            Vector3 tangentVector = Vector3.Cross(vectorToTie, Vector3.up).normalized;
            if (Vector3.Dot(newVelocity, tangentVector) < 0)
            {
                tangentVector = -tangentVector;
            }

            newVelocity = tangentVector.normalized * newFlatVelocityMagnitude + Vector3.down * newVelocity.y;
        }

        if (isInDoorRange)
        {
            Vector3 dirToDoor = (nearestDoorTransform.position - transform.position).normalized;
            newVelocity = newVelocity / 2 + dirToDoor * 50;
        }

        RaycastHit groundHit;
        Physics.Raycast(transform.position + Vector3.up, Vector3.down, out groundHit);

        newVelocity = new Vector3(newVelocity.x, initialYVelocity, newVelocity.z);
        if (newVelocity.sqrMagnitude > 50000)
        {
            newVelocity = newVelocity.normalized * 100;
        }

        rb.AddForce(Vector3.down * gravityCoeficiant * (transform.position.y - groundHit.point.y));
        rb.velocity = newVelocity;

        audioManager.SetLoopingSoundVolume("Walk", newFlatVelocity.magnitude / 100);

        Debug.DrawRay(transform.position, newVelocity);

        Quaternion newRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, horizontalInput * turnSpeed * Time.fixedDeltaTime, 0));
        if (newFlatVelocity.sqrMagnitude > 100)
        {
            float maxTurnDegrees = (1 - Vector3.Dot(newFlatVelocity.normalized, transform.forward)) * maxTurnTowardsVelocity * Time.fixedDeltaTime;
            newRotation = Quaternion.RotateTowards(newRotation, Quaternion.LookRotation(newFlatVelocity + transform.forward * 0.001f, Vector3.up), maxTurnDegrees);
        }

        transform.rotation = newRotation;
    }

    IEnumerator StopSlideTimer()
    {
        yield return new WaitForSeconds(0.625f / 2f);

        if (animState == AnimationState.Sliding)
            OnFinishSlide();
    }

    public void OnFinishSlide()
    {
        lockAnimState = false;
        animState = AnimationState.Normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Door")
        {
            isInDoorRange = true;
            nearestDoorTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Door")
        {
            isInDoorRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(tieLocation, 0.5f);
    }
}
