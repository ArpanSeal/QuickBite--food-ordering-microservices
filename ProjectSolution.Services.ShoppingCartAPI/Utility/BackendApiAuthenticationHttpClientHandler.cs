using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace ProjectSolution.Services.ShoppingCartAPI.Utility
{
    public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Safely retrieve the access token from the current HTTP context
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Get the access token from the authentication properties of the current user session (not from headers or cookies directly) using default authentication scheme - see the notes below
                string? accessToken = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            // Proceed with the request
            return await base.SendAsync(request, cancellationToken);
        }
    }
}


/*
What is a Delegating Handler?

A Delegating Handler is a message handler that sits in the HttpClient pipeline and can inspect or modify outgoing HTTP requests and incoming responses.

It acts like middleware for HttpClient.

They are especially important when using HttpClientFactory and microservices.

Your Code
   ↓
Delegating Handler 1
   ↓
Delegating Handler 2
   ↓
HttpClientHandler (sends HTTP request)
   ↓
External API

Each handler can: Modify request, Add headers, Log, Retry, Handle errors

🔁 Similar to ASP.NET middleware
ASP.NET Middleware                      Delegating Handler
Handles incoming requests               Handles outgoing requests
Runs in server pipeline                 Runs in HttpClient pipeline

🔹 How DelegatingHandler works

It overrides:

SendAsync(HttpRequestMessage request, CancellationToken ct)

Example structure:

public class MyHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // BEFORE request
        Console.WriteLine("Before request");

        var response = await base.SendAsync(request, cancellationToken);

        // AFTER response
        Console.WriteLine("After response");

        return response;
    }
}

Just like middleware:

Code before base.SendAsync runs first
Code after runs on response

🔹 How to register Delegating Handlers
Step 1: Register handler in DI
builder.Services.AddTransient<AuthHeaderHandler>();

Step 2: Attach to HttpClient
builder.Services.AddHttpClient("MyClient")
    .AddHttpMessageHandler<AuthHeaderHandler>();


Or typed client:

builder.Services.AddHttpClient<IMyService, MyService>()
    .AddHttpMessageHandler<AuthHeaderHandler>();

🔹 Multiple handlers
.AddHttpMessageHandler<LoggingHandler>()
.AddHttpMessageHandler<AuthHandler>()


Execution order:

Logging → Auth → Request
Response → Auth → Logging


Like middleware pipeline.

🔹 When should you use Delegating Handlers?

✔ Add auth tokens
✔ Logging
✔ Retry logic
✔ Correlation IDs
✔ Request/response transformation
✔ Centralized error handling

🔥 Important: Delegating Handlers vs Middleware

Middleware:

Handles incoming server requests

Delegating Handler:

Handles outgoing HttpClient requests

🎯 Interview-ready explanation

Delegating Handlers are HttpClient pipeline components that intercept outgoing HTTP requests and incoming responses. They are used for cross-cutting concerns like authentication, logging, and retries, similar to middleware but for outgoing calls.

🧠 One-line summary

Delegating Handler = middleware for HttpClient.

*/










/*

✅ Yes, it is correct.
"access_token" is the standard key name used by ASP.NET Core to store the access token in the authentication session.

But let’s explain properly so it makes sense.

✅ What this line does
_accessor.HttpContext.GetTokenAsync("access_token");


This retrieves a token stored in the authentication properties of the current user session.

It does NOT read from headers or cookies directly — it reads from the auth middleware store.

🔍 Why "access_token"?

Because ASP.NET Core authentication uses predefined token names.

Common ones:

Token Name	Purpose
access_token	API access token (JWT)
refresh_token	Refresh token
id_token	Identity token (OpenID Connect)

These names come from:

OpenID Connect / OAuth standards

✅ When this works

This only works if:

options.SaveTokens = true;


is set during authentication setup.

Example config
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddOpenIdConnect(options =>
{
    options.SaveTokens = true; // IMPORTANT
});


Without SaveTokens = true:

👉 GetTokenAsync("access_token") returns null.

🧠 Where the token is actually stored

Not in claims.

Stored in:

AuthenticationProperties


which lives in:

Auth cookie / session


ASP.NET manages this.

🟢 Typical usage

Example:

var token = await HttpContext.GetTokenAsync("access_token");

_httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);


Used when:

MVC app calls APIs

BFF pattern

Microservices

⚠️ Common confusion

People think:

access_token == claim

❌ Wrong

Claims are user info.
Access token is for API authorization.

🏆 Interview-Ready Answer

“'access_token' is the standard key used by ASP.NET Core authentication to store OAuth/OIDC access tokens in the authentication properties. GetTokenAsync retrieves it from there, provided SaveTokens is enabled.”

💡 Bonus Knowledge (Senior-level)

You can also get:

GetTokenAsync("refresh_token");
GetTokenAsync("id_token");

🚀 Quick check for you

If you log:

var token = await HttpContext.GetTokenAsync("access_token");


and it’s null:

👉 You forgot SaveTokens = true.

 */