
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float distance = speed * Time.deltaTime;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(
            transform.position,
            direction,
            out RaycastHit hit,
            distance))
        {
            CrystalTarget crystal =
                hit.collider.GetComponentInParent<CrystalTarget>();

            if (crystal != null)
            {
                crystal.Activate();
            }

            Destroy(gameObject);
            return;
        }

        transform.position += direction * distance;
    }
}
