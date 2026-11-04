
using UnityEngine;
using MuhurluKapi.Llm;

namespace MuhurluKapi.Door
{
    public class DoorController : MonoBehaviour
    {
        public const int RequiredTrust = 3;

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

        public void OpenDoor(NpcResponse response, int trustPoints)
        {
            if (IsOpen)
                return;

            if (response == null || !response.IsValid() ||
                response.Action != "open_door")
            {
                Debug.LogWarning("Kapı açılamaz: Geçerli LLM open_door kararı yok!", this);
                return;
            }

            if (trustPoints < RequiredTrust)
            {
                Debug.LogWarning("Kapı açılamaz: Orrin güveni yetersiz!", this);
                return;
            }

            // Orrin requires the actual seal. Legacy puzzle bypass settings or
            // a disabled obstacle are not sufficient authorization to open it.
            if (sealStone == null || !sealStone.IsActivated)
            {
                Debug.LogWarning(
                    "Kapı açılamaz: Orrin mühür taşı eksik veya etkin değil!", this);
                return;
            }

            IsOpen = true;
            transform.Rotate(0f, 90f, 0f);

            Debug.Log("Mühürlü kapı açıldı!");
        }
    }
}
