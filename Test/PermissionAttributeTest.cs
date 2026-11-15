using Controller.Middleware.Attribute;
using Infrastructure.Models.Enums;

namespace Test;

public class PermissionAttributeTest
{
    [Fact]
    public void TestPermissionSuccess()
    {
        // Arrange
        var attribute = new PermissionAttribute(PermissionFlags.OrganizationRead | PermissionFlags.OrganizationCreate);
        // Act
        var requiredPermissions = attribute.Permission;
        // Assert
        Assert.Equal(PermissionFlags.OrganizationRead | PermissionFlags.OrganizationCreate, requiredPermissions);
    }

    [Fact]
    public void TestPermissionFailure()
    {
        // Arrange
        var attribute = new PermissionAttribute(PermissionFlags.OrganizationUpdate | PermissionFlags.UserOrganizationDelete);
        // Act
        var requiredPermissions = attribute.Permission;
        // Assert
        Assert.NotEqual(PermissionFlags.OrganizationRead | PermissionFlags.OrganizationCreate, requiredPermissions);
    }
}
