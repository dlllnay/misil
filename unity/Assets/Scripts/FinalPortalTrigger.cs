
using UnityEngine;

public class FinalPortalTrigger : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    private bool completed;

    private void OnTriggerEnter(Collider other)
    {
        if (completed) return;

        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<CharacterController>() == null)
            return;

        completed = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Debug.Log("Muhurlu Kapi - Bolum Tamamlandi!");
    }
}
