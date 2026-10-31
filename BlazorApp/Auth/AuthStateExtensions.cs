using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Auth;

// Small helper so pages don't repeat the "find the Id claim" code.
public static class AuthStateExtensions
{
    // Returns the logged-in user's id, or null if nobody is logged in.
    public static async Task<int?> GetUserIdAsync(this Task<AuthenticationState> stateTask)
    {
        AuthenticationState state = await stateTask;
        ClaimsPrincipal user = state.User;

        if (user.Identity is null || !user.Identity.IsAuthenticated)
            return null;

        string? idAsString = user.Claims.SingleOrDefault(c => c.Type == "Id")?.Value;
        return int.TryParse(idAsString, out int id) ? id : null;
    }
}
