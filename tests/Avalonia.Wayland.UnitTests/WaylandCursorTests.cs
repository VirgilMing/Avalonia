using Avalonia.Input;
using Avalonia.Wayland.Server.Transient;
using Xunit;

namespace Avalonia.Wayland.UnitTests;

public class WaylandCursorTests
{
    [Theory]
    [InlineData(StandardCursorType.UpArrow)]
    [InlineData((StandardCursorType)(-1))]
    [InlineData((StandardCursorType)999)]
    public void Unsupported_Cursor_Should_Use_Theme_Fallback(StandardCursorType cursorType)
    {
        Assert.Null(WaylandCursorManager.GetCursorShape(cursorType));
    }
}
