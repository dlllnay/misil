
using System;
using UnityEngine;

public class OrrinDialogue : MonoBehaviour
{
    public string GetResponse(string playerMessage)
    {
        if (string.IsNullOrWhiteSpace(playerMessage))
        {
            return "Seni dinliyorum, yolcu.";
        }

        string message = playerMessage
            .ToLower(new System.Globalization.CultureInfo("tr-TR"));

        if (message.Contains("kimsin") ||
            message.Contains("adın"))
        {
            return "Ben Orrin. Bu kadim kapının bekçisiyim.";
        }

        if (message.Contains("kapı") &&
            (message.Contains("ardında") ||
             message.Contains("arkasında")))
        {
            return "Kapının ardında kadim bir sır saklı.";
        }

        if (message.Contains("kapı") &&
            (message.Contains("aç") ||
             message.Contains("geç")))
        {
            return "Önce bana güvenebileceğini göstermelisin.";
        }

        if (message.Contains("yardım") ||
            message.Contains("ne yapmalıyım"))
        {
            return "Sabırlı ol, yolcu. Cevaplar zamanla gelecek.";
        }

        return "Bu sorunun cevabı henüz gizli, yolcu.";
    }
}
