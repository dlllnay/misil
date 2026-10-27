
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CompanionController))]
public class CompanionSealCommand : MonoBehaviour
{
    [SerializeField] private Transform muhurHedef;
    [SerializeField] private SealStone sealStone;

    private CompanionController _controller;

    private void Awake()
    {
        _controller = GetComponent<CompanionController>();
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        if (muhurHedef == null || sealStone == null)
        {
            Debug.LogWarning(
                "MuhurHedef veya SealStone atanmamis!");
            return;
        }

        _controller.CommandMoveTo(
            muhurHedef.position,
            sealStone);

        Debug.Log("Kivilcim muhur tasina gidiyor!");
    }
}
