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
}
