
using UnityEngine;

public class SealStone : MonoBehaviour
{
    [SerializeField] private GameObject engel;

    private bool _activated;

    public void Activate()
    {
        if (_activated) return;

        _activated = true;

        Debug.Log("Muhur tasi etkinlestirildi!");

        if (engel != null)
        {
            engel.SetActive(false);
            Debug.Log("Engel kaldirildi!");
        }
        else
        {
            Debug.LogWarning("Engel atanmamis!");
        }
    }
}
