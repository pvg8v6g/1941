using SkiaSharp;

namespace Library.Services;

public interface ISvgClassColorService
{
    /// <summary>
    /// Updates or inserts a CSS rule that sets the fill color for the provided class selector
    /// (e.g., .fr) inside the given SVG xml text and returns the updated SVG xml text.
    /// The rule is emitted as ".{className} { fill: #RRGGBB; }" with !important to help override inline fills.
    /// </summary>
    /// <param name="svgXml">Input SVG xml as text.</param>
    /// <param name="className">SVG class name without leading dot.</param>
    /// <param name="color">Color to apply to fill.</param>
    /// <returns>Modified SVG xml.</returns>
    string UpsertClassFill(string svgXml, string className, SKColor color);

    /// <summary>
    /// Extracts all distinct class tokens that appear in attributes like class="a b c".
    /// </summary>
    ISet<string> ExtractClasses(string svgXml);

    /// <summary>
    /// Returns a new SVG xml with a <style> block that sets fills for provided classes.
    /// The dictionary maps class name (without dot) to fill color. Optionally lightens
    /// the fill of <paramref name="hoveredClass"/> for hover effect.
    /// </summary>
    string ApplyClassFills(string svgXml, IReadOnlyDictionary<string, SKColor> fills, string? hoveredClass = null, float hoverLighten = 0.18f);
}
