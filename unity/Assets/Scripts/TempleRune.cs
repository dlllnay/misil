using UnityEngine;

public enum TempleRune { Purple, Turquoise, Gold }

public static class TempleRuneAppearance
{
    public static string Label(this TempleRune rune) => rune switch
    {
        TempleRune.Purple => "Mor",
        TempleRune.Turquoise => "Turkuaz",
        _ => "Altın"
    };

    public static string Phrase(this TempleRune rune) => rune switch
    {
        TempleRune.Purple => "gecenin mor gölgesi",
        TempleRune.Turquoise => "kadim suların turkuaz nefesi",
        _ => "altın şafak"
    };

    public static Color Color(this TempleRune rune) => rune switch
    {
        TempleRune.Purple => new Color(.64f, .26f, 1f),
        TempleRune.Turquoise => new Color(.09f, .86f, .84f),
        _ => new Color(1f, .68f, .19f)
    };
}
