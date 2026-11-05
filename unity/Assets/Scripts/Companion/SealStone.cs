
using UnityEngine;
using UnityEngine.InputSystem;

public class SealStone : MonoBehaviour
{
    [SerializeField] private GameObject engel;
    [SerializeField] private float interactionDistance = 2.5f;

    private bool _activated;
    private Transform player;

    public bool IsActivated => _activated;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogWarning("SealStone: Player bulunamadı!");
    }

    private void Update()
    {
        if (TempleMenu.BlocksGameplay || _activated ||
            player == null ||
            Keyboard.current == null)
            return;

        if (Vector3.Distance(
                player.position,
                transform.position) > interactionDistance)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Activate();
    }

    public virtual void Activate()
    {
        if (_activated)
            return;

        _activated = true;

        Debug.Log("Muhur tasi etkinlestirildi!");

        if (engel != null)
        {
            engel.SetActive(false);
            Debug.Log("Engel kaldirildi!");
        }
    }
}
