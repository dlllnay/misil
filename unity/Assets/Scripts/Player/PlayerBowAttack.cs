
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
    [SerializeField] private float aimAssistRadius = 0.65f;

    private Animator animator;
    private MuhurluKapi.Interaction.PlayerInteraction interaction;
    private bool isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        interaction = GetComponent<MuhurluKapi.Interaction.PlayerInteraction>();
    }

    private void Update()
    {
        if (TempleMenu.BlocksGameplay || Mouse.current == null || (interaction != null && interaction.IsDialogueOpen)) return;

        bool shiftPressed =
            Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed ||
             Keyboard.current.rightShiftKey.isPressed);


        if (Mouse.current.rightButton.isPressed &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            !shiftPressed &&
            !isAttacking)
        {
            StartCoroutine(Shoot());
        }

    }

    private IEnumerator Shoot()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogWarning("Arrow Prefab veya Spawn Point eksik!");
            yield break;
        }

        isAttacking = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(shootDelay);

        if (interaction != null && interaction.IsDialogueOpen)
        {
            isAttacking = false;
            yield break;
        }

        Camera cam = Camera.main;

        Vector3 aimPoint =
            arrowSpawnPoint.position +
            transform.forward * maxAimDistance;

        if (cam != null)
        {
            Ray ray = cam.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

            RaycastHit[] hits =
                Physics.RaycastAll(ray, maxAimDistance);

            System.Array.Sort(
                hits,
                (a, b) => a.distance.CompareTo(b.distance)
            );

            float firstHitDistance = maxAimDistance;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform))
                    continue;

                aimPoint = hit.point;
                firstHitDistance = hit.distance;
                break;
            }

            // Nisangahin yakinindaki kristalleri ara.
            RaycastHit[] nearbyHits = Physics.SphereCastAll(
                ray,
                aimAssistRadius,
                maxAimDistance
            );

            CrystalTarget bestCrystal = null;
            float bestDistance = float.MaxValue;

            foreach (RaycastHit hit in nearbyHits)
            {
                CrystalTarget crystal =
                    hit.collider.GetComponentInParent<CrystalTarget>();

                if (crystal == null || crystal.IsActivated)
                    continue;

                Vector3 center =
                    hit.collider.bounds.center;

                float alongRay = Vector3.Dot(
                    center - ray.origin,
                    ray.direction
                );

                // Duvarin arkasindaki hedefe kilitlenme.
                if (alongRay < 0f ||
                    alongRay > firstHitDistance + 0.8f)
                    continue;

                float distanceFromRay = Vector3.Distance(
                    center,
                    ray.GetPoint(alongRay)
                );

                if (distanceFromRay < bestDistance)
                {
                    bestDistance = distanceFromRay;
                    bestCrystal = crystal;
                }
            }

            if (bestCrystal != null)
            {
                Collider crystalCollider =
                    bestCrystal.GetComponent<Collider>();

                aimPoint = crystalCollider != null
                    ? crystalCollider.bounds.center
                    : bestCrystal.transform.position;
            }
        }

        Vector3 direction =
            aimPoint - arrowSpawnPoint.position;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        Quaternion rotation =
            Quaternion.LookRotation(direction.normalized);

        Instantiate(
            arrowPrefab,
            arrowSpawnPoint.position,
            rotation
        );
        TempleAudio.Play(TempleSound.Arrow, arrowSpawnPoint.position);

        yield return new WaitForSeconds(0.7f);
        isAttacking = false;
    }
}
