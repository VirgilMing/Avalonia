using Avalonia.Input;
using Avalonia.SourceGenerator;
using Avalonia.Wayland.Server.Transient;
using NWayland.Protocols.Wayland;
using static NWayland.Protocols.CursorShapeV1.WpCursorShapeDeviceV1;

namespace Avalonia.Wayland.Server.Persistent;

/// <summary>The surface + hotspot to hand to <c>wl_pointer.set_cursor</c>.</summary>
[DefinitelyNotARecord]
readonly partial struct WaylandCursorImage(WlSurface Surface, int HotspotX, int HotspotY);

/// <summary>
/// Worker-side representation of a pointer cursor. The UI thread only ever holds the generated
/// <c>WaylandCursorProxy</c> wrapper (see <see cref="IWaylandCursor"/>); choosing a compositor
/// shape or resolving a fallback <c>wl_surface</c> happens here, on the worker thread.
/// </summary>
abstract class WaylandCursor : IWaylandCursor
{
    /// <summary>
    /// Resolves a compositor shape or a surface + hotspot on the worker thread.
    /// When both are null, the pointer is hidden.
    /// </summary>
    public abstract (WaylandCursorImage? image, ShapeEnum? shape) Resolve(WaylandGlobals globals);

    /// <inheritdoc/>
    public abstract void Destroy();
}

/// <summary>
/// A standard cursor. Stateless apart from the requested type — it owns no wl_surface and
/// resolves a compositor shape or a themed surface on demand, so it doesn't
/// need to be a persistent object.
/// </summary>
sealed class WaylandStandardCursor : WaylandCursor
{
    public StandardCursorType CursorType { get; }

    public WaylandStandardCursor(StandardCursorType cursorType)
    {
        CursorType = cursorType;
    }

    public override (WaylandCursorImage? image, ShapeEnum? shape) Resolve(WaylandGlobals globals)
    {
        if (CursorType == StandardCursorType.None)
            return (null, null);

        if (GetShape(CursorType, globals.CursorShapeManager?.Version ?? 0) is { } shape)
            return (null, shape);

        return (globals.CursorManager.GetCursor(CursorType) is { } c
            ? new WaylandCursorImage(c.Surface, c.HotspotX, c.HotspotY)
            : null, null);
    }

    internal static ShapeEnum? GetShape(StandardCursorType cursorType, int version)
    {
        if (version < 1)
            return null;

        // NWayland does not expose per-enum version requirements. AllResize was added in v2.
        // UpArrow has no equivalent shape; preserve the theme's non-resize cursor instead.
        return cursorType switch
        {
            StandardCursorType.Arrow => ShapeEnum.Default,
            StandardCursorType.Ibeam => ShapeEnum.Text,
            StandardCursorType.Wait => ShapeEnum.Wait,
            StandardCursorType.Cross => ShapeEnum.Crosshair,
            StandardCursorType.SizeWestEast => ShapeEnum.EwResize,
            StandardCursorType.SizeNorthSouth => ShapeEnum.NsResize,
            StandardCursorType.SizeAll when version >= 2 => ShapeEnum.AllResize,
            StandardCursorType.No => ShapeEnum.NotAllowed,
            StandardCursorType.Hand => ShapeEnum.Pointer,
            StandardCursorType.AppStarting => ShapeEnum.Progress,
            StandardCursorType.Help => ShapeEnum.Help,
            StandardCursorType.TopSide => ShapeEnum.NResize,
            StandardCursorType.BottomSide => ShapeEnum.SResize,
            StandardCursorType.LeftSide => ShapeEnum.WResize,
            StandardCursorType.RightSide => ShapeEnum.EResize,
            StandardCursorType.TopLeftCorner => ShapeEnum.NwResize,
            StandardCursorType.TopRightCorner => ShapeEnum.NeResize,
            StandardCursorType.BottomLeftCorner => ShapeEnum.SwResize,
            StandardCursorType.BottomRightCorner => ShapeEnum.SeResize,
            StandardCursorType.DragMove => ShapeEnum.Grabbing,
            StandardCursorType.DragCopy => ShapeEnum.Copy,
            StandardCursorType.DragLink => ShapeEnum.Alias,
            _ => null
        };
    }

    // Nothing to release — the themed surfaces are owned by WaylandCursorManager.
    public override void Destroy()
    {
    }
}
