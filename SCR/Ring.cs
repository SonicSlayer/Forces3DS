using UnityEngine;

public class Ring : MonoBehaviour
{
    public int value = 1;
    public float rotationSpeed = 90f;
    public AudioClip collectSound;

    private SpriteRenderer spriteRenderer;
    private Collider ringCollider;
    private bool isCollected = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ringCollider = GetComponent<Collider>();

        if (spriteRenderer != null)
        {
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        ModernSonicController player = other.GetComponent<ModernSonicController>();
        if (player != null)
        {
            CollectRing(player);
        }
    }

    private void CollectRing(ModernSonicController player)
    {
        isCollected = true;
        player.AddRings(value);

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        DisableRing();
        Destroy(gameObject, 0.5f);
    }

    private void DisableRing()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (ringCollider != null) ringCollider.enabled = false;
    }
}