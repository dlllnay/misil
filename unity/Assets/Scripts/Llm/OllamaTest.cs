
using System;
using UnityEngine;

namespace MuhurluKapi.Llm
{
    public class OllamaTest : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("Orrin: Ollama baglantisi test ediliyor...");

            try
            {
                ILlmClient client = new OllamaClient();

                string cevap = await client.GenerateResponseAsync(
                    "Sen Orrin adinda fantastik bir kapi bekcisisin. Oyuncuya Turkce tek cumleyle selam ver."
                );

                Debug.Log("Orrin cevabi: " + cevap);
            }
            catch (Exception ex)
            {
                Debug.LogError("Ollama baglanti hatasi: " + ex.Message);
            }
        }
    }
}
  