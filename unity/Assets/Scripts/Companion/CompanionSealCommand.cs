
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
        if (TempleMenu.BlocksGameplay || Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        // No companion seal command is configured in this scene. E may belong
        // to another interaction (for example Orrin's independent seal).
        if (muhurHedef == null && sealStone == null)
            return;

        if (muhurHedef == null || sealStone == null)
        {
            Debug.LogWarning(
                "MuhurHedef veya SealStone atanmamis!");
            return;
        }
        Debug.Log($"MuhurHedef dunya konumu: {muhurHedef.position}");

        _controller.CommandMoveTo(
            muhurHedef.position,
            sealStone);

        Debug.Log("Kivilcim muhur tasina gidiyor!");
    }
}
