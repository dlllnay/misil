using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MuhurluKapi.Dialogue
{
    // Shared presentation for both Orrins. Dialogue and quest state live in the callers.
    public sealed class TempleDialogueView
    {
        private static readonly Color Gold = new Color(0.82f, 0.65f, 0.36f);
        private static readonly Color Ink = new Color(0.065f, 0.038f, 0.13f, 0.97f);
        private readonly TMP_FontAsset font;
        private readonly ScrollRect scroll;
        private readonly RectTransform content;
        private TMP_Text pending;
        private GameObject pendingPlayerBubble;

        public TempleDialogueView(RectTransform root, TMP_FontAsset font, TMP_InputField input,
            Button send, string title, string subtitle, Action close)
        {
            this.font = font;
            Place(root, new Vector2(.09f, .045f), new Vector2(.91f, .59f), Vector2.zero, Vector2.zero);
            root.GetComponent<Image>().color = Ink;
            var frame = Box(root, "Altın çerçeve", Gold);
            Place(frame, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            frame.SetAsFirstSibling();
            var inset = Box(frame, "Mor zemin", Ink);
            Place(inset, Vector2.zero, Vector2.one, Vector2.one * 2, -Vector2.one * 2);

            var heading = Text(root, title, 23, Gold);
            Place(heading.rectTransform, Vector2.up, Vector2.one, new Vector2(18, -35), new Vector2(-120, -8));
            var caption = Text(root, subtitle, 13, new Color(.76f, .68f, .88f));
            Place(caption.rectTransform, Vector2.up, Vector2.one, new Vector2(18, -54), new Vector2(-120, -35));
            var rule = Box(root, "Başlık çizgisi", Gold);
            Place(rule, Vector2.up, Vector2.one, new Vector2(18, -57), new Vector2(-18, -56));

            var closeButton = UnityEngine.Object.Instantiate(send, root);
            closeButton.name = "Kapat";
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener(() => close());
            StyleButton(closeButton, "Kapat");
            Place((RectTransform)closeButton.transform, Vector2.one, Vector2.one,
                new Vector2(-100, -45), new Vector2(-16, -12));

            var viewport = Box(root, "Konuşma geçmişi", new Color(.028f, .018f, .063f, .72f));
            viewport.GetComponent<Image>().raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            Place(viewport, Vector2.zero, Vector2.one, new Vector2(16, 62), new Vector2(-16, -64));
            content = new GameObject("Mesajlar", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.pivot = new Vector2(.5f, 1);
            Place(content, Vector2.up, Vector2.one, Vector2.zero, Vector2.zero);
            var stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(8, 8, 8, 8);
            stack.spacing = 7;
            stack.childControlWidth = stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            Place((RectTransform)input.transform, Vector2.zero, Vector2.right,
                new Vector2(16, 13), new Vector2(-144, 51));
            if (input.targetGraphic != null) input.targetGraphic.color = new Color(.17f, .12f, .26f);
            if (input.textComponent != null) input.textComponent.color = Color.white;
            if (input.placeholder is TMP_Text placeholder)
            {
                placeholder.text = "Mesajını yaz...";
                placeholder.color = new Color(.75f, .68f, .86f);
            }
            StyleButton(send, "Gönder");
            Place((RectTransform)send.transform, Vector2.right, Vector2.right,
                new Vector2(-132, 13), new Vector2(-16, 51));
        }

        public void Begin(string message)
        {
            pendingPlayerBubble = Add("Sen", message, true).transform.parent.gameObject;
            pending = Add("Orrin", "Orrin düşünüyor...", false);
            ScrollToLatest();
        }

        public void Complete(string answer)
        {
            if (pending == null) return;
            pending.text = answer.StartsWith("Orrin: ", StringComparison.Ordinal) ? answer.Substring(7) : answer;
            pending = null;
            pendingPlayerBubble = null;
            ScrollToLatest();
        }

        public void Cancel()
        {
            if (pending == null) return;
            UnityEngine.Object.Destroy(pending.transform.parent.gameObject);
            UnityEngine.Object.Destroy(pendingPlayerBubble);
            pending = null;
            pendingPlayerBubble = null;
        }

        public void ScrollToLatest()
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f;
        }

        public void Notice(string message)
        {
            Add("Orrin", message, false);
            ScrollToLatest();
        }

        private TMP_Text Add(string speaker, string message, bool player)
        {
            var bubble = Box(content, speaker, player ? new Color(.19f, .16f, .28f) : new Color(.22f, .12f, .32f));
            var layout = bubble.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(player ? 18 : 11, 11, 6, 8);
            layout.spacing = 3;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            Text(bubble, speaker, 14, Gold);
            return Text(bubble, message, 17, new Color(.96f, .92f, 1f));
        }

        private TextMeshProUGUI Text(Transform parent, string value, float size, Color color)
        {
            var label = new GameObject("Metin", typeof(RectTransform), typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.richText = false;
            label.raycastTarget = false;
            label.text = value;
            return label;
        }

        private static RectTransform Box(Transform parent, string name, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            image.raycastTarget = false;
            return image.rectTransform;
        }

        private static void StyleButton(Button button, string caption)
        {
            var colors = button.colors;
            colors.normalColor = new Color(.46f, .24f, .65f);
            colors.highlightedColor = new Color(.63f, .39f, .81f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.32f, .14f, .48f);
            button.colors = colors;
            if (button.targetGraphic != null) button.targetGraphic.color = Color.white;
            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text == null) return;
            text.text = caption;
            text.color = Gold;
            text.fontSize = 17;
        }

        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = low;
            rect.offsetMax = high;
        }
    }
}
