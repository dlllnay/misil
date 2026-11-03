
using UnityEngine;

public class OrrinDialogueTest : MonoBehaviour
{
    private void Start()
    {
        OrrinDialogue orrin = GetComponent<OrrinDialogue>();

        if (orrin == null)
        {
            Debug.LogError("OrrinDialogue bulunamadi!");
            return;
        }

        TestQuestion(orrin, "Sen kimsin?");
        TestQuestion(orrin, "Kapının ardında ne var?");
        TestQuestion(orrin, "Kapıyı nasıl açabilirim?");
        TestQuestion(orrin, "Bana yardım eder misin?");
    }

    private void TestQuestion(OrrinDialogue orrin, string question)
    {
        string answer = orrin.GetResponse(question);

        Debug.Log("Oyuncu: " + question);
        Debug.Log("Orrin: " + answer);
    }
}
