using System.Security.Claims;
using System.Security.Principal;

namespace AspNetCoreApi.Services
{
    public interface IPrincipalWrapper
    {
        string GetUserId();
        string FindFirstValue(string claimType); // Define the method signature here
    }
}
