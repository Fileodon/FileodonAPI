using Controller.Middleware.Attribute;
using Infrastructure.Models.Enums;

namespace Test;

public class PermissionAttributeTest
{
    [Fact]
    public void TestPermissionSuccess()
    {
        // Arrange
        var attribute = new PermissionAttribute(PermissionFlags.Read | PermissionFlags.Create);
        // Act
        var requiredPermissions = attribute.Permission;
        // Assert
        Assert.Equal(PermissionFlags.Read | PermissionFlags.Create, requiredPermissions);
    }

    [Fact]
    public void TestPermissionFailure()
    {
        // Arrange
        var attribute = new PermissionAttribute(PermissionFlags.Update | PermissionFlags.Delete);
        // Act
        var requiredPermissions = attribute.Permission;
        // Assert
        Assert.NotEqual(PermissionFlags.Read | PermissionFlags.Create, requiredPermissions);
    }
}
