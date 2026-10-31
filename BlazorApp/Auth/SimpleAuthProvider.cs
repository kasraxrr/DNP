using System.Security.Claims;
using System.Text.Json;
using ApiContracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorApp.Auth;

// Tells the Blazor framework who is logged in (as a ClaimsPrincipal).
// Blazor calls GetAuthenticationStateAsync() whenever it needs to know,
// e.g. for <AuthorizeView>, @attribute [Authorize] and [CascadingParameter] Task<AuthenticationState>.
public class SimpleAuthProvider : AuthenticationStateProvider
{
    private const string StorageKey = "currentUser";

    private readonly HttpClient httpClient;
    private readonly IJSRuntime jsRuntime;

    // "Primary cache": the logged-in user for this circuit (browser tab).
    // If it's null we look in the browser's sessionStorage, which survives a page refresh.
    private ClaimsPrincipal? cachedPrincipal;

    public SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this.jsRuntime = jsRuntime;
    }

    public async Task LoginAsync(string userName, string password)
    {
        // 1) Ask the Web API to check username + password
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(userName, password));

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content); // e.g. "Incorrect username or password"
        }

        // 2) Read the UserDto that came back
        UserDto userDto = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        // 3) Remember the user in the browser, so a refresh doesn't log you out
        string serialisedData = JsonSerializer.Serialize(userDto);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, serialisedData);

        // 4) Turn it into claims and tell Blazor that the auth state changed
        cachedPrincipal = CreateClaimsPrincipal(userDto);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(cachedPrincipal)));
    }

    public async Task LogoutAsync()
    {
        cachedPrincipal = null;
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (cachedPrincipal is not null)
        {
            return new AuthenticationState(cachedPrincipal);
        }

        string? userAsJson;
        try
        {
            userAsJson = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        }
        catch (InvalidOperationException)
        {
            // JavaScript isn't available yet (while the page is being prerendered on the server).
            // Treat the user as "not logged in" for now; Blazor asks again once the page is interactive.
            return new AuthenticationState(new ClaimsPrincipal());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new ClaimsPrincipal()); // nobody logged in
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(userAsJson)!;
        cachedPrincipal = CreateClaimsPrincipal(userDto);
        return new AuthenticationState(cachedPrincipal);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(UserDto userDto)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, userDto.UserName), // becomes User.Identity.Name
            new Claim("Id", userDto.Id.ToString()),
            // Add more claims here if UserDto gets more properties, e.g.:
            // new Claim("Email", userDto.Email)
        };

        // The "apiauth" string is the authentication type. Any non-empty value
        // makes IsAuthenticated == true; an identity without one means "not logged in".
        ClaimsIdentity identity = new(claims, "apiauth");
        return new ClaimsPrincipal(identity);
    }
}
