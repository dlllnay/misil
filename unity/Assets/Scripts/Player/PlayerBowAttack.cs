
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerBowAttack : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float shootDelay = 0.5f;
    [SerializeField] private float maxAimDistance = 100f;

    private Animator animator;
    private bool isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            !isAttacking)
        {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogWarning("Arrow veya Spawn Point atanmamis!");
            yield break;
        }

        isAttacking = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(shootDelay);

        Camera cam = Camera.main;

        Vector3 aimPoint =
            arrowSpawnPoint.position + transform.forward * 100f;

        if (cam != null)
        {
            Ray cameraRay = cam.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

            aimPoint = cameraRay.GetPoint(maxAimDistance);

            RaycastHit[] hits = Physics.RaycastAll(
                cameraRay,
                maxAimDistance
            );

            System.Array.Sort(hits,
                (a, b) => a.distance.CompareTo(b.distance)
            );

            foreach (RaycastHit hit in hits)
            {
                // Ranger'in kendi Collider'ini yok say.
                if (hit.collider.transform.IsChildOf(transform))
                    continue;

                aimPoint = hit.point;
                Debug.Log("Nisan alinan nesne: " +
                          hit.collider.name);
                break;
            }
        }

        Vector3 direction =
            (aimPoint - arrowSpawnPoint.position).normalized;

        Quaternion rotation = Quaternion.LookRotation(direction);

        Instantiate(
            arrowPrefab,
            arrowSpawnPoint.position,
            rotation
        );

        Debug.DrawLine(
            arrowSpawnPoint.position,
            aimPoint,
            Color.yellow,
            3f
        );

        Debug.Log("Ok firlatildi!");

        yield return new WaitForSeconds(0.7f);
        isAttacking = false;
    }
}
