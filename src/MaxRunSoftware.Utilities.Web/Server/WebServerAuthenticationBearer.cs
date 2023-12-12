// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using EmbedIO;
using EmbedIO.Utilities;
using Microsoft.IdentityModel.Tokens;

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServerAuthenticationBearerModule : WebModuleBase
{
    private readonly ILogger log;
    private readonly Func<IWebServerAuthenticationBearerHandler?> getAuthenticationHandler;
    public override bool IsFinalHandler => false;

    public WebServerAuthenticationBearerModule(
        ILoggerProvider loggerProvider,
        Func<IWebServerAuthenticationBearerHandler?> getAuthenticationHandler,
        string baseRoute = "/"
    ) : base(baseRoute)
    {
        log = loggerProvider.CreateLogger<WebServerAuthenticationBearerModule>();
        this.getAuthenticationHandler = getAuthenticationHandler;
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        var authHandler = getAuthenticationHandler();
        if (authHandler != null)
        {
            var httpContext = new WebServerHttpContext(context);

            await OnRequestAsync(httpContext, authHandler);

        }
    }

    protected virtual async Task OnRequestAsync(WebServerHttpContext context, IWebServerAuthenticationBearerHandler handler)
    {
        if (handler.IsBearerRequestPath(context))
        {
            log.LogTrace("Requesting BEARER token at {Url}", context.RequestPath);
            context.SetResponseHeader(HttpResponseHeader.WwwAuthenticate, "Bearer");

            var isValidAuthentication = await Task.FromResult(handler.IsAuthenticationValid(context));
            if (isValidAuthentication)
            {
                log.LogTrace("Successful BEARER token request at {Url} so responding with BEARER token", context.RequestPath);
                await handler.RespondWithToken(context);
            }
            else
            {
                log.LogTrace("Failed BEARER token request at {Url}", context.RequestPath);
                throw HttpException.Unauthorized();
            }
        }
        else if (handler.IsPathRequiresBearerAuthentication(context))
        {
            log.LogTrace("Access to {Url} requires BEARER authentication", context.RequestPath);
            context.SetResponseHeader(HttpResponseHeader.WwwAuthenticate, "Bearer");

            var isBearerTokenValid = await Task.FromResult(handler.IsBearerAuthenticationTokenValid(context));
            if (isBearerTokenValid)
            {
                log.LogTrace("Access to {Url} success because of valid BEARER token", context.RequestPath);
            }
            else
            {
                log.LogTrace("Access to {Url} failed because of invalid BEARER token", context.RequestPath);
                throw HttpException.Unauthorized();
            }
        }
    }
}

[PublicAPI]
public interface IWebServerAuthenticationBearerHandler
{
    public bool IsBearerRequestPath(WebServerHttpContext context);
    public bool IsAuthenticationValid(WebServerHttpContext context);
    public Task RespondWithToken(WebServerHttpContext context);

    public bool IsPathRequiresBearerAuthentication(WebServerHttpContext context);
    public bool IsBearerAuthenticationTokenValid(WebServerHttpContext context);
}


public class WebServerAuthenticationBearerHandler : IWebServerAuthenticationBearerHandler
{
    public virtual required WebUrlPath BearerTokenRequestPath { get; init; }
    public bool IsBearerRequestPath(WebServerHttpContext context) => BearerTokenRequestPath.Equals(context.RequestPath);

    public bool IsAuthenticationValid(WebServerHttpContext context) => throw new NotImplementedException();

    public async Task RespondWithToken(WebServerHttpContext context)
    {
        if
        throw new NotImplementedException();
    }

    public bool IsPathRequiresBearerAuthentication(WebServerHttpContext context) => throw new NotImplementedException();
    public bool IsBearerAuthenticationTokenValid(WebServerHttpContext context) => throw new NotImplementedException();

    protected readonly Dictionary<string, Session> sessions = new(StringComparer.OrdinalIgnoreCase);
    protected readonly object sessionLocker = new();

    protected class Session
    {
        public required string Token { get; init; }
        public required string Username { get; init; }
        public required DateTimeOffset StartedOn { get; init; }
    }

}






/// <summary>
/// Basic Authorization Server Provider implementation.
/// </summary>
public class BasicAuthorizationServerProvider : IAuthorizationServerProvider
{
    /// <inheritdoc />
    public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context)
    {
        var data = await context.HttpContext.GetRequestFormDataAsync().ConfigureAwait(false);

        if (data?.ContainsKey("grant_type") == true && data["grant_type"] == "password")
        {
            context.Validated(data.ContainsKey("username") ? data["username"] : string.Empty);
        }
        else
        {
            context.Rejected();
        }
    }

    /// <inheritdoc />
    public long GetExpirationDate() => DateTime.UtcNow.AddHours(12).Ticks;
}


    /// <summary>
    /// EmbedIO module to allow authorizations with Bearer Tokens.
    /// </summary>
    public class BearerTokenModule : WebModuleBase
    {
        //private readonly string _tokenEndpoint;
        private readonly IAuthorizationServerProvider _authorizationServerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="endpoint">The endpoint for the authorization (relative to baseUrlPath).</param>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            SymmetricSecurityKey secretKey,
            string endpoint = "/token")
            : base(baseUrlPath)
        {
            SecretKey = secretKey ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF"));
            _tokenEndpoint = endpoint;
            _authorizationServerProvider = authorizationServerProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenModule" /> class.
        /// </summary>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="authorizationServerProvider">The authorization server provider.</param>
        /// <param name="secretKeyString">The secret key string.</param>
        /// <param name="endpoint">The endpoint for the authorization (relative to baseUrlPath).</param>
        /// <exception cref="ArgumentNullException">secretKeyString</exception>
        /// <exception cref="ArgumentException">A key must be 40 chars.</exception>
        public BearerTokenModule(
            string baseUrlPath,
            IAuthorizationServerProvider authorizationServerProvider,
            string secretKeyString,
            string endpoint = "/token")
            : this(
                baseUrlPath,
                authorizationServerProvider,
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString)),
                endpoint)
        {
            if (secretKeyString == null)
                throw new ArgumentNullException(nameof(secretKeyString));

            if (secretKeyString.Length != 40)
                throw new ArgumentException("A key must be 40 chars");
        }

        /// <summary>
        /// Gets the secret key.
        /// </summary>
        /// <value>
        /// The secret key.
        /// </value>
        public SymmetricSecurityKey SecretKey { get; }

        /// <summary>
        /// Gets or sets the on success transformation method.
        /// </summary>
        /// <value>
        /// The on success.
        /// </value>
        public Action<IDictionary<string, object>>? OnSuccessTransformation { get; set; }

        /// <inheritdoc />
        public override bool IsFinalHandler => false;

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context!.RequestedPath == _tokenEndpoint && context.Request.HttpVerb == HttpVerbs.Post)
            {
                await OnTokenRequest(context).ConfigureAwait(false);
                return;
            }

            ((IHttpContextImpl)context).User = context.GetPrincipal(SecretKey, out var securityToken);

            // decode token to see if it's valid
            if (securityToken != null)
            {
                return;
            }

            context.Rejected();
            context.SetHandled();
        }

        private async Task OnTokenRequest(IHttpContext context)
        {
            context.SetHandled();

            var validationContext = context.GetValidationContext();
            await _authorizationServerProvider.ValidateClientAuthentication(validationContext)
                .ConfigureAwait(false);

            if (!validationContext.IsValidated)
            {
                context.Rejected(validationContext.ErrorPayload);
                return;
            }

            var expiryDate = DateTime.SpecifyKind(
                DateTime.FromBinary(_authorizationServerProvider.GetExpirationDate()),
                DateTimeKind.Utc);

            var token = new BearerToken
            {
                Token = validationContext.GetToken(SecretKey, expiryDate),
                TokenType = "bearer",
                ExpirationDate = _authorizationServerProvider.GetExpirationDate(),
                Username = validationContext.IdentityName,
            };

            var dictToken = Json.Deserialize<Dictionary<string, object>>(Json.Serialize(token));

            OnSuccessTransformation?.Invoke(dictToken);

            await context
                .SendDataAsync(dictToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieves a ValidationContext.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The Validation Context from the HTTP Context.</returns>
        public static ValidateClientAuthenticationContext GetValidationContext(this IHttpContext context)
            => new ValidateClientAuthenticationContext(context);

        /// <summary>
        /// Rejects a authentication challenge.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="error">The error.</param>
        public static void Rejected(this IHttpContext context, object? error = null)
        {
            context.Response.Headers.Add(HttpHeaderNames.WWWAuthenticate, "Bearer");

            throw HttpException.Unauthorized(data: error);
        }

        /// <summary>
        /// Gets the <see cref="SecurityToken" /> of the current context.
        /// Returns null when the token is not found or not validated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken? GetSecurityToken(this IHttpContext context, string secretKey)
            => context.GetSecurityToken(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)));

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static SecurityToken? GetSecurityToken(this IHttpContext context, SymmetricSecurityKey? secretKey = null)
        {
            context.GetPrincipal(secretKey, out var securityToken);

            return securityToken;
        }

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The security token from the HTTP Context.</returns>
        public static ClaimsPrincipal? GetPrincipal(this IHttpContext context, SymmetricSecurityKey? secretKey = null)
            => context.GetPrincipal(secretKey, out _);

        /// <summary>
        /// Gets the principal.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>The claims.</returns>
        public static ClaimsPrincipal? GetPrincipal(
            this IHttpContext context,
            SymmetricSecurityKey? secretKey,
            out SecurityToken? securityToken)
        {
            var authHeader = context!.Request.Headers[HttpHeaderNames.Authorization];

            securityToken = null;

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                var token = authHeader.Replace("Bearer ", string.Empty);
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = secretKey,
                };

                return tokenHandler.ValidateToken(token, tokenParams, out securityToken);
            }
            catch (Exception ex)
            {
                securityToken = null;
                Console.Error.WriteLine(ex);
            }

            return null;
        }

        /// <summary>
        /// Fluent-like method to attach BearerToken.
        /// </summary>
        /// <param name="this">The webserver.</param>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <returns>
        /// The same web server.
        /// </returns>
        public static IWebServer WithBearerToken(
            this IWebServer @this,
            string baseUrlPath,
            SymmetricSecurityKey secretKey,
            IAuthorizationServerProvider? authorizationProvider = null) =>
            @this.WithModule(
                new BearerTokenModule(
                    baseUrlPath,
                    authorizationProvider ?? new BasicAuthorizationServerProvider(),
                    secretKey));

        /// <summary>
        /// Withes the bearer token.
        /// </summary>
        /// <param name="this">The webserver.</param>
        /// <param name="baseUrlPath">The base URL path.</param>
        /// <param name="secretKeyString">The secret key string.</param>
        /// <param name="authorizationProvider">The authorization provider.</param>
        /// <returns>
        /// The same web server.
        /// </returns>
        public static IWebServer WithBearerToken(
            this IWebServer @this,
            string baseUrlPath,
            string secretKeyString,
            IAuthorizationServerProvider? authorizationProvider = null) =>
            @this.WithModule(
                new BearerTokenModule(
                    baseUrlPath,
                    authorizationProvider ?? new BasicAuthorizationServerProvider(),
                    secretKeyString));
    }


    /// <summary>
    /// Authorization Server Provider interface.
    /// </summary>
    public interface IAuthorizationServerProvider
    {
        /// <summary>
        /// Validates a Client Authentication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A task representing the client authentication.
        /// </returns>
        Task ValidateClientAuthentication(ValidateClientAuthenticationContext context);

        /// <summary>
        /// Gets a Expiration Date.
        /// </summary>
        /// <returns>Ticks until expiration date.</returns>
        long GetExpirationDate();
    }


    /// <summary>
    /// Context to share data with AuthorizationServerProvider.
    /// </summary>
    public class ValidateClientAuthenticationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateClientAuthenticationContext"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <exception cref="ArgumentNullException">httpContext.</exception>
        public ValidateClientAuthenticationContext(IHttpContext httpContext)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            Identity = new ClaimsIdentity();
        }

        /// <summary>
        /// The Client Id.
        /// </summary>
        public string? IdentityName { get; protected set; }

        /// <summary>
        /// Flags if the Validation has errors.
        /// </summary>
        public bool HasError { get; protected set; }

        /// <summary>
        /// Indicates if the Validation is right.
        /// </summary>
        public bool IsValidated { get; protected set; }

        /// <summary>
        /// Http Context instance.
        /// </summary>
        public IHttpContext HttpContext { get; protected set; }

        /// <summary>
        /// Gets or sets the identity.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets the error payload.
        /// </summary>
        /// <value>
        /// The error payload.
        /// </value>
        public object? ErrorPayload { get; protected set; }

        /// <summary>
        /// Rejects a validation with optional payload to be sent as JSON.
        /// </summary>
        /// <param name="errorPayload">The error payload.</param>
        public void Rejected(object? errorPayload = null)
        {
            IsValidated = false;
            HasError = true;
            ErrorPayload = errorPayload;
        }

        /// <summary>
        /// Validates credentials with identity name.
        /// </summary>
        /// <param name="identityName">Name of the identity.</param>
        public void Validated(string? identityName = null)
        {
            IdentityName = identityName;
            Identity.AddClaim(new Claim(ClaimTypes.Name, identityName));
            IsValidated = true;
            HasError = false;
        }

        /// <summary>
        /// Retrieve JsonWebToken.
        /// </summary>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="expires">The expires.</param>
        /// <returns>
        /// The token string.
        /// </returns>
        public string GetToken(SymmetricSecurityKey secretKey, DateTime? expires = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var plainToken = tokenHandler
                .CreateToken(new SecurityTokenDescriptor
                {
                    Subject = Identity,
                    Issuer = "Embedio Bearer Token",
                    Expires = expires,
                    SigningCredentials = new SigningCredentials(secretKey,
                        SecurityAlgorithms.HmacSha256Signature),
                });

            return tokenHandler.WriteToken(plainToken);
        }
    }
