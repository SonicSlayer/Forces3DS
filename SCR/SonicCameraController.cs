using UnityEngine;

public class SonicCameraController : MonoBehaviour
{
    public Transform target; 
    public float distance = 5.0f; 
    public float height = 2.0f; 
    public float rotationSpeed = 100.0f; 
    public float followSpeed = 10.0f; 
    public float verticalAngleLimit = 40.0f; 


    public float circlePadDeadZone = 0.2f;
    public float circlePadSensitivity = 1.5f;

    private float currentRotationY = 0.0f;
    private float currentRotationX = 15.0f;
    private Vector3 offset;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isRotating = false;

    void Start()
    {
        if (!target)
        {
            Debug.LogWarning("Camera target not set! Looking for Sonic.");
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }


        currentRotationY = target.eulerAngles.y;
        offset = new Vector3(0, height, -distance);

я
        UpdateCameraPosition(true);
    }

    void LateUpdate()
    {
        if (!target) return;


        Handle3DSInput();


        UpdateCameraPosition();

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }

   // private void Handle3DSInput()

    private void UpdateCameraPosition(bool immediate = false)
    {
  
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);


        targetPosition = target.position + rotation * offset;

        Vector3 lookDirection = target.position + Vector3.up * (height * 0.5f) - targetPosition;
        targetRotation = Quaternion.LookRotation(lookDirection);

        RaycastHit hit;
        Vector3 castStart = target.position + Vector3.up * height;
        Vector3 castDirection = (targetPosition - castStart).normalized;
        float castDistance = Vector3.Distance(castStart, targetPosition);

        if (Physics.Raycast(castStart, castDirection, out hit, castDistance))
        {
            if (!hit.collider.isTrigger)
            {
                targetPosition = hit.point - castDirection * 0.3f;
            }
        }

        if (immediate)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    public Vector3 GetCameraForward()
    {
        return Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
    }

    public Vector3 GetCameraRight()
    {
        return transform.right;
    }

    public bool IsRotating()
    {
        return isRotating;
    }
}