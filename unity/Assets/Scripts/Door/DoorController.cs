
using UnityEngine;

namespace MuhurluKapi.Door
{
    public class DoorController : MonoBehaviour
    {
        [Header("Bulmaca Kosulu")]
        [SerializeField] private bool requirePuzzle = true;

        [SerializeField] private SealStone sealStone;

        [SerializeField] private GameObject puzzleObstacle;

        public bool IsOpen { get; private set; }

        public bool IsPuzzleCompleted
        {
            get
            {
                if (!requirePuzzle)
                    return true;

                if (sealStone != null)
                    return sealStone.IsActivated;

                return puzzleObstacle != null &&
                       !puzzleObstacle.activeSelf;
            }
        }

        public void OpenDoor()
        {
            if (IsOpen)
                return;

            if (!IsPuzzleCompleted)
            {
                Debug.LogWarning(
                    "Kapı açılamaz: Bulmaca henüz tamamlanmadı!");
                return;
            }

            IsOpen = true;
            transform.Rotate(0f, 90f, 0f);

            Debug.Log("Mühürlü kapı açıldı!");
        }
    }
}
