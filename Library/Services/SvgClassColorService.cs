using System.Text;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace Library.Services;

public class SvgClassColorService : ISvgClassColorService
{
    // Matches a CSS rule for a class like .xx { ... }
    private static Regex BuildClassRuleRegex(string className)
    {
        var escaped = Regex.Escape(className);
        return new Regex($@"\.{escaped}\s*\{{[^}}]*\}}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    }

    public string UpsertClassFill(string svgXml, string className, SKColor color)
    {
        if (string.IsNullOrWhiteSpace(svgXml) || string.IsNullOrWhiteSpace(className))
            return svgXml;

        var styleStartIndex = svgXml.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
        // Ensure a <style> element exists as a direct child of <svg>
        if (styleStartIndex < 0)
        {
            // Insert a new style block right after the opening <svg ...>
            var svgOpenIndex = svgXml.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
            if (svgOpenIndex >= 0)
            {
                var tagClose = svgXml.IndexOf('>', svgOpenIndex);
                if (tagClose > svgOpenIndex)
                {
                    var styleBlock = "<style type=\"text/css\"><![CDATA[\n/* app rules */\n]]></style>\n";
                    svgXml = svgXml.Insert(tagClose + 1, styleBlock);
                    styleStartIndex = svgXml.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        // If for some reason still no style, just append one at the end before </svg>
        if (styleStartIndex < 0)
        {
            var endSvg = svgXml.LastIndexOf("</svg>", StringComparison.OrdinalIgnoreCase);
            if (endSvg >= 0)
            {
                svgXml = svgXml.Insert(endSvg, "\n<style type=\"text/css\"><![CDATA[\n/* app rules */\n]]></style>\n");
                styleStartIndex = svgXml.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
            }
        }

        // Build the desired rule text
        var hex = ColorToCss(color);
        var desiredRule = $".{className}{{ fill: {hex} !important; }}";

        // Find the end of the first style block's content (we will work inside CDATA if present)
        // Prefer editing inside <![CDATA[ ... ]]>
        var cdataStart = svgXml.IndexOf("<![CDATA[", styleStartIndex, StringComparison.OrdinalIgnoreCase);
        var cdataEnd = cdataStart >= 0 ? svgXml.IndexOf("]]>", cdataStart, StringComparison.OrdinalIgnoreCase) : -1;
        if (cdataStart >= 0 && cdataEnd > cdataStart)
        {
            var contentStart = cdataStart + "<![CDATA[".Length;
            var length = cdataEnd - contentStart;
            var css = svgXml.Substring(contentStart, length);
            var updatedCss = UpsertCssRule(css, className, desiredRule);
            var sb = new StringBuilder(svgXml);
            sb.Remove(contentStart, length);
            sb.Insert(contentStart, updatedCss);
            return sb.ToString();
        }
        else
        {
            // No CDATA; insert/update between <style> and </style>
            var styleClose = svgXml.IndexOf("</style>", styleStartIndex, StringComparison.OrdinalIgnoreCase);
            if (styleClose > styleStartIndex)
            {
                var openEnd = svgXml.IndexOf('>', styleStartIndex);
                if (openEnd > styleStartIndex)
                {
                    var css = svgXml.Substring(openEnd + 1, styleClose - (openEnd + 1));
                    var updatedCss = UpsertCssRule(css, className, desiredRule);
                    var sb = new StringBuilder(svgXml);
                    sb.Remove(openEnd + 1, css.Length);
                    sb.Insert(openEnd + 1, updatedCss);
                    return sb.ToString();
                }
            }
        }

        return svgXml;
    }

    public ISet<string> ExtractClasses(string svgXml)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(svgXml)) return set;

        // Find class attributes and split by whitespace
        var rx = new Regex("class\\s*=\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in rx.Matches(svgXml))
        {
            var content = m.Groups[1].Value;
            foreach (var token in content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // ignore common non-country classes if any
                set.Add(token.Trim('.'));
            }
        }
        return set;
    }

    public string ApplyClassFills(string svgXml, IReadOnlyDictionary<string, SKColor> fills, string? hoveredClass = null, float hoverLighten = 0.18f)
    {
        if (string.IsNullOrWhiteSpace(svgXml) || fills == null || fills.Count == 0)
            return svgXml;

        // Build a single CSS with rules for each class
        var sbCss = new StringBuilder();
        foreach (var kvp in fills)
        {
            var c = kvp.Value;
            if (!string.IsNullOrWhiteSpace(hoveredClass) && string.Equals(kvp.Key, hoveredClass, StringComparison.OrdinalIgnoreCase))
            {
                c = Lighten(c, hoverLighten);
            }
            var hex = ColorToCss(c);
            sbCss.Append('.').Append(kvp.Key).Append("{ fill: ").Append(hex).Append(" !important; stroke: none !important; }")
                .Append('\n');
        }

        // Ensure a <style> block exists then replace/append our rules under /* app rules */ marker
        var styleStartIndex = svgXml.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
        if (styleStartIndex < 0)
        {
            var svgOpenIndex = svgXml.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
            if (svgOpenIndex >= 0)
            {
                var tagClose = svgXml.IndexOf('>', svgOpenIndex);
                if (tagClose > svgOpenIndex)
                {
                    var styleBlock = "<style type=\"text/css\"><![CDATA[\n/* app rules */\n]]></style>\n";
                    svgXml = svgXml.Insert(tagClose + 1, styleBlock);
                    styleStartIndex = svgXml.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        // Try to find CDATA marked block
        var cdataStart = styleStartIndex >= 0 ? svgXml.IndexOf("<![CDATA[", styleStartIndex, StringComparison.OrdinalIgnoreCase) : -1;
        var cdataEnd = cdataStart >= 0 ? svgXml.IndexOf("]]>", cdataStart, StringComparison.OrdinalIgnoreCase) : -1;
        if (cdataStart >= 0 && cdataEnd > cdataStart)
        {
            var contentStart = cdataStart + "<![CDATA[".Length;
            var length = cdataEnd - contentStart;
            var css = svgXml.Substring(contentStart, length);
            var updatedCss = UpsertOrReplaceAppRules(css, sbCss.ToString());
            var sb = new StringBuilder(svgXml);
            sb.Remove(contentStart, length);
            sb.Insert(contentStart, updatedCss);
            return sb.ToString();
        }
        else if (styleStartIndex >= 0)
        {
            var styleClose = svgXml.IndexOf("</style>", styleStartIndex, StringComparison.OrdinalIgnoreCase);
            if (styleClose > styleStartIndex)
            {
                var openEnd = svgXml.IndexOf('>', styleStartIndex);
                if (openEnd > styleStartIndex)
                {
                    var css = svgXml.Substring(openEnd + 1, styleClose - (openEnd + 1));
                    var updatedCss = UpsertOrReplaceAppRules(css, sbCss.ToString());
                    var sb = new StringBuilder(svgXml);
                    sb.Remove(openEnd + 1, css.Length);
                    sb.Insert(openEnd + 1, updatedCss);
                    return sb.ToString();
                }
            }
        }

        return svgXml;
    }

    private static string UpsertOrReplaceAppRules(string css, string newRules)
    {
        const string startMarker = "/* app rules */";
        // Try to find previous block demarcated by markers; if not found, append at end
        var idx = css.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            // Replace from marker until next marker or end
            var nextMarker = css.IndexOf(startMarker, idx + startMarker.Length, StringComparison.OrdinalIgnoreCase);
            var before = css.Substring(0, idx + startMarker.Length);
            var after = nextMarker > idx ? css.Substring(nextMarker) : string.Empty;
            return before + "\n" + newRules + (after.Length > 0 ? "\n" + after : string.Empty);
        }
        var needsNewline = css.Length > 0 && !css.EndsWith("\n");
        return css + (needsNewline ? "\n" : string.Empty) + startMarker + "\n" + newRules;
    }

    private static SKColor Lighten(SKColor color, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        byte L(byte v) => (byte)Math.Clamp(v + (255 - v) * amount, 0, 255);
        return new SKColor(L(color.Red), L(color.Green), L(color.Blue), color.Alpha);
    }

    private static string UpsertCssRule(string css, string className, string desiredRule)
    {
        var rx = BuildClassRuleRegex(className);
        if (rx.IsMatch(css))
        {
            return rx.Replace(css, desiredRule);
        }
        else
        {
            var needsNewline = css.Length > 0 && !css.EndsWith("\n");
            return css + (needsNewline ? "\n" : string.Empty) + desiredRule + "\n";
        }
    }

    private static string ColorToCss(SKColor color)
    {
        // Ignore alpha for fill when fully opaque; include alpha if not 255
        if (color.Alpha == 255)
        {
            return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
        }
        return $"rgba({color.Red},{color.Green},{color.Blue},{color.Alpha / 255.0:F3})";
    }
}
