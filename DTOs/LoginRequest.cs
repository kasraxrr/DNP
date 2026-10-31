namespace DTOs;

// What the Blazor app sends to POST /auth/login.
// It lives in the shared DTOs project, so both the WebAPI and the Blazor app can use it.
public record LoginRequest(string UserName, string Password);
