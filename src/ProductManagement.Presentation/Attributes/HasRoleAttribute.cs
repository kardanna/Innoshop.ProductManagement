using Microsoft.AspNetCore.Authorization;

namespace ProductManagement.Presentation.Attributes;

public class HasRoleAttribute : AuthorizeAttribute
{
    public HasRoleAttribute(params string[] roles)
    {
        if (roles.Length != 0)
        {
            Roles = string.Join(",", roles);
        }
    }
}