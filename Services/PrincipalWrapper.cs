using System;
using System.Security.Claims;
using System.Security.Principal;

namespace AspNetCoreApi.Services
{
    public class PrincipalWrapper : IPrincipalWrapper
    {
        private readonly IPrincipal _principal;

        public PrincipalWrapper(IPrincipal principal)
        {
            _principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        // Implementing GetUserId as before
        public string GetUserId()
        {
            var claimsPrincipal = _principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                throw new InvalidOperationException("Principal is not of type ClaimsPrincipal.");
            }

            return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Implementing FindFirstValue from IPrincipalWrapper interface
        public string FindFirstValue(string claimType)
        {
            var claimsPrincipal = _principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                throw new InvalidOperationException("Principal is not of type ClaimsPrincipal.");
            }

            // Retrieve and return the claim value based on claimType
            return claimsPrincipal.FindFirstValue(claimType);
        }
    }
}
