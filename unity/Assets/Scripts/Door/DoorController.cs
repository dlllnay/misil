
using UnityEngine;

namespace MuhurluKapi.Door
{
    public class DoorController : MonoBehaviour
    {
        private bool isOpen = false;

        public void OpenDoor()
        {
            if (isOpen) return;

            isOpen = true;
            transform.Rotate(0f, 90f, 0f);

            Debug.Log("Mühürlü kapı açıldı!");
        }
    }
}
