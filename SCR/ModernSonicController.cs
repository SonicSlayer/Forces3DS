
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ModernSonicController : MonoBehaviour
{
    [Header("Models")]
    public GameObject sonicModel;
    public GameObject ballModel;

    [Header("Movement")]
    public float runSpeedMax = 15f;
    public float boostSpeed = 25f;
    public float acceleration = 6f;
    public float boostAcceleration = 10f;
    public float deceleration = 12f;
    public float gravity = 25f;
    public float jumpPower = 9f;
    public float airControl = 0.5f;
    public float rotationSpeed = 14f;

    [Header("Homing Attack")]
    public float homingRange = 5f;
    public float homingSpeed = 25f;
    public float homingCooldown = 0.5f;
    public LayerMask homingTargetMask;

    [Header("Quick Step / Drift / Slide")]
    public float quickStepDistance = 2f;
    public float quickStepCooldown = 0.25f;
    public float driftTurnSpeed = 40f;
    public float slideSpeed = 12f;
    public float slideDuration = 0.6f;
    public float stompSpeed = -30f;

    [Header("2.5D Mode")]
    public bool is2D = false;

    [Header("Rail & AutoMove")]
    public bool isOnRail = false;
    public Transform[] railPoints;
    public float railSpeed = 20f;
    private int railIndex = 0;
    public bool autoMoving = false;
    public Transform[] autoPath;
    public float autoMoveSpeed = 15f;
    private int autoIndex = 0;

    private CharacterController controller;
    private Animator animator;
    private SonicCameraController cameraController;

    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed = 0f;
    private float verticalVelocity = 0f;
    private float quickStepTimer = 0f;
    private float slideTimer = 0f;
    private float homingTimer = 0f;
    private bool isSliding = false;
    private bool isStomping = false;
    private bool isDrifting = false;
    private bool homingUsed = false;

    private int ringCount = 0;

    [Header("UI & Boost Gauge")]
    public UnityEngine.UI.Text ringText;
    public UnityEngine.UI.Text speedText;
    public UnityEngine.UI.Image boostGauge;
    private float boostEnergy = 100f;
    private float boostMax = 100f;
    private float boostDrainRate = 30f;
    private float boostRegenRate = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<SonicCameraController>();

        if (sonicModel != null) sonicModel.SetActive(true);
        if (ballModel != null) ballModel.SetActive(false);
    }

    bool canControl = true;

    void Update()
    {
        if (isOnRail)
        {
            canControl = false;
        {
            HandleRail();
            return;
        }
        if (autoMoving)
        {
            canControl = false;
        {
            HandleAutoMove();
            return;
        }

        float h, v;
        bool boost, jump, qL, qR, drift, action;
        GetInput(out h, out v, out boost, out jump, out qL, out qR, out drift, out action);

        Vector3 inputDir = canControl ? GetInputDir(h, v) : Vector3.zero;
        bool grounded = controller.isGrounded;

        isDrifting = drift && grounded && inputDir.magnitude > 0.1f;
        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            float turnSpeed = isDrifting ? driftTurnSpeed : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Boost Gauge control
        if (boost && boostEnergy > 0f)
        {
            boostEnergy -= boostDrainRate * Time.deltaTime;
            if (boostEnergy < 0f) boostEnergy = 0f;
        }
        else
        {
            boostEnergy += boostRegenRate * Time.deltaTime;
            if (boostEnergy > boostMax) boostEnergy = boostMax;
        }

        float targetSpeed = boost && boostEnergy > 0f ? boostSpeed : runSpeedMax;
        float accel = boost ? boostAcceleration : acceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, inputDir.magnitude > 0.1f ? targetSpeed : 0, accel * Time.deltaTime);

        Vector3 move = inputDir * currentSpeed;

        if (grounded)
        {
            verticalVelocity = -1f;
            if (jump)
            {
                verticalVelocity = jumpPower;
                homingUsed = false;
            }
            if (action)
            {
                isSliding = true;
                slideTimer = slideDuration;
                SetBallMode(true);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            move *= airControl;

            if (!grounded && jump && !homingUsed && homingTimer <= 0f)
            {
                Transform target = FindHomingTarget();
                if (target != null)
                {
                    move = (target.position - transform.position).normalized * homingSpeed;
                    verticalVelocity = 0;
                }
                else move = transform.forward * homingSpeed;

                homingUsed = true;
                homingTimer = homingCooldown;
                SetBallMode(true);
            }
            if (action && !isStomping)
            {
                verticalVelocity = stompSpeed;
                isStomping = true;
                SetBallMode(true);
            }
        }

        quickStepTimer -= Time.deltaTime;
        if (quickStepTimer <= 0f && grounded && currentSpeed > runSpeedMax * 0.6f)
        {
            if (qL)
            {
                StartCoroutine(PerformQuickStep(-1));
            }
            else if (qR)
            {
                StartCoroutine(PerformQuickStep(1));
            }
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            move = transform.forward * slideSpeed;
            if (slideTimer <= 0f)
            {
                isSliding = false;
                SetBallMode(false);
            }
        }
        if (grounded && !isSliding)
        {
            isStomping = false;
            SetBallMode(false);
        }

        moveDirection = move;
        moveDirection.y = verticalVelocity;
        controller.Move(moveDirection * Time.deltaTime);

        if (is2D)
        {
            Vector3 pos = transform.position;
            pos.x = 0f;
            transform.position = pos;
        }

        homingTimer -= Time.deltaTime;
        animator.SetFloat("Speed", controller.velocity.magnitude);

        // Update UI
        if (ringText != null) ringText.text = "Rings: " + ringCount;
        if (speedText != null) speedText.text = "Speed: " + ((int)(controller.velocity.magnitude * 10f)) + " km/h";
        if (boostGauge != null) boostGauge.fillAmount = boostEnergy / boostMax;
        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("IsSliding", isSliding);
    }

    void GetInput(out float h, out float v, out bool boost, out bool jump, out bool qL, out bool qR, out bool drift, out bool action)
    {
        h = v = 0f; boost = jump = qL = qR = drift = action = false;
#if UNITY_N3DS
        if (UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Emulation_Left)) h = -1f;
        if (UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Emulation_Right)) h = 1f;
        if (UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Emulation_Up)) v = 1f;
        if (UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Emulation_Down)) v = -1f;
        boost = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Y);
        jump = UnityEngine.N3DS.GamePad.GetButtonTrigger(N3dsButton.B);
        qL = UnityEngine.N3DS.GamePad.GetButtonTrigger(N3dsButton.X) && h < 0;
        qR = UnityEngine.N3DS.GamePad.GetButtonTrigger(N3dsButton.X) && h > 0;
        drift = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.R);
        action = UnityEngine.N3DS.GamePad.GetButtonTrigger(N3dsButton.A);
#else
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        boost = Input.GetKey(KeyCode.LeftShift);
        jump = Input.GetKeyDown(KeyCode.Space);
        qL = Input.GetKeyDown(KeyCode.Q);
        qR = Input.GetKeyDown(KeyCode.E);
        drift = Input.GetKey(KeyCode.LeftControl);
        action = Input.GetKeyDown(KeyCode.LeftAlt);
#endif
    }

    Vector3 GetInputDir(float h, float v)
    {
        Vector3 forward = cameraController.GetCameraForward();
        Vector3 right = cameraController.GetCameraRight();
        return (forward * v + right * h).normalized;
    }

    Transform FindHomingTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, homingRange, homingTargetMask);
        float minAngle = 60f;
        Transform best = null;
        foreach (Collider hit in hits)
        {
            Vector3 dir = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle < minAngle)
            {
                minAngle = angle;
                best = hit.transform;
            }
        }
        return best;
    }

    void SetBallMode(bool active)
    {
        if (sonicModel != null) sonicModel.SetActive(!active);
        if (ballModel != null) ballModel.SetActive(active);
    }

    void HandleRail()
    {
        if (railPoints == null || railPoints.Length == 0) return;
        Transform target = railPoints[railIndex];
        Vector3 dir = (target.position - transform.position).normalized;
        controller.Move(dir * railSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(dir);
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            railIndex++;
            if (railIndex >= railPoints.Length)
            {
                isOnRail = false;
                railIndex = 0;
            }
        }
    }

    void HandleAutoMove()
    {
        if (autoPath == null || autoPath.Length == 0) return;
        Transform target = autoPath[autoIndex];
        Vector3 dir = (target.position - transform.position).normalized;
        controller.Move(dir * autoMoveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(dir);
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            autoIndex++;
            if (autoIndex >= autoPath.Length)
            {
                autoMoving = false;
                autoIndex = 0;
            }
        }
    }

    public void StartRail(Transform[] points)
    {
        railPoints = points;
        railIndex = 0;
        isOnRail = true;
        SetBallMode(true);
    }

    public void StartAutoMove(Transform[] path)
    {
        autoPath = path;
        autoIndex = 0;
        autoMoving = true;
        SetBallMode(true);
    }

    public void TakeDamage()
    {
        if (ringCount > 0)
        {
            ringCount = 0;
            boostEnergy = 0f;
            Debug.Log("Rings lost!");
        }
        else
        {
            Debug.Log("Sonic defeated (no rings)");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ring"))
        {
            ringCount++;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("To2D"))
        {
            is2D = true;
        }
        else if (other.CompareTag("To3D"))
        {
            is2D = false;
        }
        else if (other.CompareTag("RailStart"))
        {
            RailPath path = other.GetComponent<RailPath>();
            if (path != null) StartRail(path.points);
        }
        else if (other.CompareTag("AutoMoveStart"))
        {
            RailPath path = other.GetComponent<RailPath>();
            if (path != null) StartAutoMove(path.points);
        }
    }
    public void AddRings(int amount)
    {
        ringCount += amount;
        boostEnergy += amount * 2f;
        if (boostEnergy > boostMax) boostEnergy = boostMax;
        Debug.Log("Rings: " + ringCount);
    }

    IEnumerator PerformQuickStep(int direction)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        float dist = quickStepDistance;
        Vector3 start = transform.position;
        Vector3 target = transform.position + transform.right * direction * dist;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        quickStepTimer = quickStepCooldown;
    }
}
