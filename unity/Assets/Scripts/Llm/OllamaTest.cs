
using System;
using UnityEngine;

namespace MuhurluKapi.Llm
{
    public class OllamaTest : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("Orrin: JSON yaniti test ediliyor...");

            try
            {
                OllamaClient client = new OllamaClient();

                NpcResponse cevap = await client.GenerateNpcResponseAsync(
                    "Orrin, lütfen kapıyı aç."
                );

                Debug.Log("Orrin konusmasi: " + cevap.Dialogue);
                Debug.Log("Orrin karari: " + cevap.Action);
            }
            catch (Exception ex)
            {
                Debug.LogError("Ollama JSON test hatasi: " + ex.Message);
            }
        }
    }
}
