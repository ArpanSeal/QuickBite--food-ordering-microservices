using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;

namespace ProjectSolution.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        // When we are working with cookies, we need to inject IHttpContextAccessor to access the HttpContext.
        // Why would we need to access HttpContext? Because cookies are part of the HTTP request/response cycle and HttpContext provides access to the request and response objects, which contain the cookies.

        // When we are working with cookies, why do we need to inject IHttpContextAccessor - gept c# 2

        //🔹 Short answer

        //We inject IHttpContextAccessor only when we need to read or write cookies outside controllers or middleware, because cookies live on HttpContext, and non-controller classes don’t have access to it by default.

        //🔹 Where cookies actually live

        //Cookies are part of the HTTP request/response:

        //HTTP Request
        // ├── Headers
        // ├── Cookies   ← Request cookies
        // └── Body

        //HTTP Response
        // └── Set-Cookie ← Response cookies


        //In ASP.NET Core, all of this is represented by:

        //HttpContext


        //Specifically:

        //HttpContext.Request.Cookies
        //HttpContext.Response.Cookies

        //🔹 Controllers already have HttpContext

        //Inside a controller:

        //public IActionResult Login()
        //        {
        //            Response.Cookies.Append("token", "abc");
        //            var value = Request.Cookies["token"];
        //            return Ok();
        //        }


        //✅ No IHttpContextAccessor needed
        //Why?
        //Because ControllerBase already exposes HttpContext.

        //🔹 Services do NOT have HttpContext

        //Now look at a service:

        //public class AuthService
        //        {
        //            public void SetCookie()
        //            {
        //                // ❌ No HttpContext here
        //            }
        //        }


        //        Services are:

        //Created by DI

        //HTTP-agnostic by design

        //ASP.NET Core intentionally hides HttpContext from services to keep them clean and testable.

        //🔹 That’s why IHttpContextAccessor exists

        //IHttpContextAccessor is a bridge between:

        //HTTP world (HttpContext)
        //        ↓
        //Service / business layer

        //Registration
        //builder.Services.AddHttpContextAccessor();

        //        Injection
        //public class AuthService
        //        {
        //            private readonly IHttpContextAccessor _httpContextAccessor;

        //            public AuthService(IHttpContextAccessor httpContextAccessor)
        //            {
        //                _httpContextAccessor = httpContextAccessor;
        //            }

        //            public void SetCookie(string token)
        //            {
        //                _httpContextAccessor.HttpContext!
        //                    .Response
        //                    .Cookies
        //                    .Append("token", token);
        //            }
        //        }


        //        Now the service can access cookies.

        //🔹 Why ASP.NET Core doesn’t expose HttpContext automatically


        //        This is by design:


        //        Reasons:


        //        Separation of concerns

        //        Business logic ≠ HTTP logic


        //        Testability

        //        Services should work without HTTP


        //        Thread safety


        //        HttpContext is request-scoped

        //        Avoid hidden dependencies

        //        So you must opt-in explicitly by injecting IHttpContextAccessor.

        //🔹 Common real-world cookie scenarios that need it

        //✔ Authentication service setting auth cookies
        //        ✔ Token refresh logic
        //        ✔ Reading user session cookies
        //✔ Forwarding cookies to downstream APIs

        //⚠️ Important cautions (interview-critical)
        //❌ Don’t use in Singleton services

        //HttpContext is request-scoped

        //Singleton lives forever → race conditions

        //❌ Don’t put business rules around cookies

        //Keep it at infrastructure layer

        //🔹 When you do NOT need IHttpContextAccessor
        //Scenario    Need it?
        //Controller	❌ No
        //Middleware	❌ No
        //Razor Page	❌ No
        //Scoped service	✔️ Sometimes
        //Singleton service	❌ Never
        //🎯 Interview-ready explanation

        //Cookies are part of HttpContext.Controllers and middleware already have access to HttpContext, but services do not.When cookie logic is required inside a service, IHttpContextAccessor is injected to safely access the current request and response context.

        //🧠 One-line summary

        //We inject IHttpContextAccessor when working with cookies in services because cookies live in HttpContext, which services cannot access directly.




        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;
            bool? hasToken = _httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return (hasToken == true) ? token : null;
        }

        public void SetToken(string token)
        {
            if (token == null) return;
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}

// Important: MVC is a server-rendered framework, so all cookie operations happen on the server side. The browser sends cookies in the request, and the server sets or deletes cookies in the response. This is why we read cookies from HttpContext.Request and write/delete cookies using HttpContext.Response.


//when setting and clearing a cookie, we are using _contextAccessor.HttpContext.Response, but while getting a cookie why are we using _contextAccessor.HttpContext.Request?

//This is a very good observation 👍
//The reason is rooted in how HTTP works, not ASP.NET Core specifically.

//Let’s explain it conceptually first, then with ASP.NET Core code, and finally with an easy mental model.

//🔹 Core rule (memorize this)

//You read cookies from the Request and write cookies to the Response.

//This is not optional — it’s how the HTTP protocol is designed.

//🔁 How cookies actually flow in HTTP
//1️⃣ Browser → Server (Request)

//When a browser sends a request, it includes cookies it already has:

//GET /orders HTTP/1.1
//Host: example.com
//Cookie: authToken=abc123; theme=dark


//➡️ These cookies are part of the incoming request

//So on the server:

//HttpContext.Request.Cookies

//2️⃣ Server → Browser (Response)

//When the server wants to set or delete a cookie, it sends instructions back:

//HTTP/1.1 200 OK
//Set-Cookie: authToken=abc123; HttpOnly; Secure


//➡️ These instructions are part of the response

//So on the server:

//HttpContext.Response.Cookies

//🔹 Why ASP.NET Core separates them

//ASP.NET Core mirrors the HTTP model:

//Operation	    Where it lives	    Why
//Read cookie	Request.Cookies	    Sent by browser
//Set cookie	Response.Cookies	Sent to browser
//Delete cookie	Response.Cookies	Server instruction
//🔹 ASP.NET Core example (with IHttpContextAccessor)
//✅ Reading a cookie
//var token = _contextAccessor.HttpContext?
//    .Request
//    .Cookies["AuthToken"];


//Why?
//➡️ Because the cookie already exists and came from the browser.

//✅ Setting a cookie
//_contextAccessor.HttpContext?
//    .Response
//    .Cookies
//    .Append("AuthToken", token);


//Why?
//➡️ Because you are telling the browser:

//“Please store this cookie.”

//✅ Clearing a cookie
//_contextAccessor.HttpContext?
//    .Response
//    .Cookies
//    .Delete("AuthToken");


//Why?
//➡️ Deleting a cookie is also a response instruction.

//🔹 Why you CANNOT read cookies from Response
//HttpContext.Response.Cookies // ❌ Not possible


//Because:

//Response hasn’t been sent yet

//Cookies don’t exist there yet

//Response only instructs, it doesn’t hold stored cookies

//🧠 Mental model (easy to remember)

//Think of cookies like letters:

//📨 Request = letters the browser sends you
//📤 Response = letters you send back to the browser

//You:

//Read letters you receive → Request

//Write letters you send → Response

//🎯 Interview-ready explanation

//Cookies are part of the HTTP protocol. Cookies sent by the browser are available in the request, so we read them from HttpContext.Request.Cookies. When setting or deleting cookies, the server sends instructions back to the browser via response headers, so we use HttpContext.Response.Cookies.

//🧠 One-line summary

//We read cookies from Request because the browser sends them, and we write or delete cookies using Response because the server instructs the browser what to store.This is a very good observation 👍
