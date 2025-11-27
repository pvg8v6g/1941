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
