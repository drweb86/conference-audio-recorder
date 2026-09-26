using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ConferenceAudioRecorder.Legal;

public abstract record MarkdownBlock
{
    public sealed record Heading(string Text, int Level) : MarkdownBlock;
    public sealed record Paragraph(IReadOnlyList<MarkdownInline> Inlines) : MarkdownBlock;
    public sealed record Bullet(IReadOnlyList<MarkdownInline> Inlines) : MarkdownBlock;
}

public abstract record MarkdownInline
{
    public sealed record Text(string Value, bool Bold = false) : MarkdownInline;
    public sealed record Link(string Label, string Url) : MarkdownInline;
    public sealed record Code(string Value) : MarkdownInline;
}

public static class MarkdownParser
{
    public static IReadOnlyList<MarkdownBlock> Parse(string src)
    {
        var blocks = new List<MarkdownBlock>();
        var paragraph = new StringBuilder();

        void FlushParagraph()
        {
            var text = paragraph.ToString().Trim();
            paragraph.Clear();
            if (text.Length > 0)
                blocks.Add(new MarkdownBlock.Paragraph(ParseInlines(text)));
        }

        foreach (var raw in (src ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = raw.Trim();
            if (trimmed.Length == 0 || trimmed == "[Languages](README.md)")
            {
                FlushParagraph();
                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal))
            {
                FlushParagraph();
                blocks.Add(new MarkdownBlock.Heading(trimmed[4..].Trim(), 3));
            }
            else if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushParagraph();
                blocks.Add(new MarkdownBlock.Heading(trimmed[3..].Trim(), 2));
            }
            else if (trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushParagraph();
                blocks.Add(new MarkdownBlock.Heading(trimmed[2..].Trim(), 1));
            }
            else if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                FlushParagraph();
                blocks.Add(new MarkdownBlock.Bullet(ParseInlines(trimmed[2..].Trim())));
            }
            else
            {
                if (paragraph.Length > 0)
                    paragraph.Append(' ');
                paragraph.Append(trimmed);
            }
        }

        FlushParagraph();
        return blocks;
    }

    public static IReadOnlyList<MarkdownInline> ParseInlines(string text)
    {
        var result = new List<MarkdownInline>();
        var regex = new Regex(
            @"\*\*(.+?)\*\*|\[([^\]]+)\]\(([^)]+)\)|`([^`]+)`",
            RegexOptions.Singleline);
        var index = 0;
        foreach (Match match in regex.Matches(text))
        {
            if (match.Index > index)
                result.Add(new MarkdownInline.Text(text[index..match.Index]));

            if (match.Groups[1].Success)
                result.Add(new MarkdownInline.Text(match.Groups[1].Value, true));
            else if (match.Groups[2].Success)
                result.Add(new MarkdownInline.Link(match.Groups[2].Value, match.Groups[3].Value));
            else
            {
                var code = match.Groups[4].Value;
                if (code.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || code.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    result.Add(new MarkdownInline.Link(code, code));
                else
                    result.Add(new MarkdownInline.Code(code));
            }

            index = match.Index + match.Length;
        }

        if (index < text.Length)
            result.Add(new MarkdownInline.Text(text[index..]));

        return result;
    }
}
