
using UnityEngine;
using MuhurluKapi.Door;

public class FinalPortalTrigger : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private DoorController orrinDoor;

    private bool completed;

    private void OnTriggerEnter(Collider other)
    {
        if (completed) return;

        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<CharacterController>() == null)
            return;

        if (orrinDoor == null)
        {
            Debug.LogWarning("Final portalı kilitli: Orrin DoorController bağlantısı eksik.", this);
            return;
        }

        if (!orrinDoor.IsOpen)
        {
            Debug.Log("Final portalı kilitli: Önce Orrin'in kapısını açmalısın.", this);
            return;
        }

        completed = true;
        TempleAudio.Play(TempleSound.Victory, transform.position);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Debug.Log("Muhurlu Kapi - Bolum Tamamlandi!");
    }
}
