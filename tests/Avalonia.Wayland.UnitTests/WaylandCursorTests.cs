using System;
using Avalonia.Input;
using Avalonia.Wayland.Server.Persistent;
using Xunit;
using static NWayland.Protocols.CursorShapeV1.WpCursorShapeDeviceV1;

namespace Avalonia.Wayland.UnitTests;

public class WaylandCursorTests
{
    [Theory]
    [InlineData(StandardCursorType.UpArrow)]
    [InlineData((StandardCursorType)(-1))]
    [InlineData((StandardCursorType)999)]
    public void Unsupported_Cursor_Should_Use_Theme_Fallback(StandardCursorType cursorType)
    {
        Assert.Null(WaylandStandardCursor.GetShape(cursorType, 1));
        Assert.Null(WaylandStandardCursor.GetShape(cursorType, 2));
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(1, null)]
    [InlineData(2, ShapeEnum.AllResize)]
    public void SizeAll_Should_Require_Version_Two(int version, ShapeEnum? expected)
    {
        Assert.Equal(expected, WaylandStandardCursor.GetShape(StandardCursorType.SizeAll, version));
    }

    [Theory]
    [InlineData(StandardCursorType.Arrow, ShapeEnum.Default)]
    [InlineData(StandardCursorType.Ibeam, ShapeEnum.Text)]
    [InlineData(StandardCursorType.Wait, ShapeEnum.Wait)]
    [InlineData(StandardCursorType.Cross, ShapeEnum.Crosshair)]
    [InlineData(StandardCursorType.SizeWestEast, ShapeEnum.EwResize)]
    [InlineData(StandardCursorType.SizeNorthSouth, ShapeEnum.NsResize)]
    [InlineData(StandardCursorType.No, ShapeEnum.NotAllowed)]
    [InlineData(StandardCursorType.Hand, ShapeEnum.Pointer)]
    [InlineData(StandardCursorType.AppStarting, ShapeEnum.Progress)]
    [InlineData(StandardCursorType.Help, ShapeEnum.Help)]
    [InlineData(StandardCursorType.TopSide, ShapeEnum.NResize)]
    [InlineData(StandardCursorType.BottomSide, ShapeEnum.SResize)]
    [InlineData(StandardCursorType.LeftSide, ShapeEnum.WResize)]
    [InlineData(StandardCursorType.RightSide, ShapeEnum.EResize)]
    [InlineData(StandardCursorType.TopLeftCorner, ShapeEnum.NwResize)]
    [InlineData(StandardCursorType.TopRightCorner, ShapeEnum.NeResize)]
    [InlineData(StandardCursorType.BottomLeftCorner, ShapeEnum.SwResize)]
    [InlineData(StandardCursorType.BottomRightCorner, ShapeEnum.SeResize)]
    [InlineData(StandardCursorType.DragMove, ShapeEnum.Grabbing)]
    [InlineData(StandardCursorType.DragCopy, ShapeEnum.Copy)]
    [InlineData(StandardCursorType.DragLink, ShapeEnum.Alias)]
    public void Supported_Cursor_Should_Use_Shape_In_Both_Versions(StandardCursorType cursorType, ShapeEnum expected)
    {
        Assert.Equal(expected, WaylandStandardCursor.GetShape(cursorType, 1));
        Assert.Equal(expected, WaylandStandardCursor.GetShape(cursorType, 2));
    }

    [Fact]
    public void Missing_Protocol_Should_Use_Theme_For_All_Standard_Cursors()
    {
        foreach (var cursorType in Enum.GetValues<StandardCursorType>())
            Assert.Null(WaylandStandardCursor.GetShape(cursorType, 0));
    }

    [Fact]
    public void None_Should_Hide_Cursor_Without_Loading_Theme()
    {
        var cursor = new WaylandStandardCursor(StandardCursorType.None);
        // A hidden cursor must resolve without consulting the connection or theme manager.
        var (image, shape) = cursor.Resolve(null!);
        Assert.Null(image);
        Assert.Null(shape);
        Assert.Null(WaylandStandardCursor.GetShape(StandardCursorType.None, 1));
        Assert.Null(WaylandStandardCursor.GetShape(StandardCursorType.None, 2));
    }
}
