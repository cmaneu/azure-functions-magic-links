using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Azure.Demos.MagicLinksAuth.Functions.Infrastructure
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AUTH_HEADER_NAME = "auth.token";
        private const string BEARER_PREFIX = "";
        private string _issuerToken;
        private readonly string _audience;
        private readonly string _issuer;
        private readonly ApplicationSettings _appSettings;

        public AccessTokenProvider(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            _audience = "https://courses.scubabous.fr";
            _issuer = "https://courses.scubabous.fr";
        }

        public AccessTokenResult ValidateToken(HttpRequest request)
        {
            // TODO Fix this
            #region Secret
            _issuerToken = "5efb368ffb0c40b7bfd05cc512a75ed30091b91384cf48e6bbb5248b2898d04a2408d200ea144f958a5eadd74770cd3e58ee27bff8a74d15bc2947bd1215b3c13540ebf67a9a47e1ae5c4e1d95145aee";

            #endregion
            try
            {
                // Get the token from the header
                if (request != null &&
                    request.Headers.ContainsKey(AUTH_HEADER_NAME) &&
                    request.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX))
                {
                    var token = request.Headers[AUTH_HEADER_NAME].ToString().Substring(BEARER_PREFIX.Length);

                    // Create the parameters
                    var tokenParams = new TokenValidationParameters()
                    {
                        RequireSignedTokens = true,
                        ValidAudience = _audience,
                        ValidateAudience = true,
                        ValidIssuer = _issuer,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_issuerToken))
                    };

                    // Validate the token
                    var handler = new JwtSecurityTokenHandler();
                    var result = handler.ValidateToken(token, tokenParams, out var securityToken);
                    return AccessTokenResult.Success(result);
                }
                else
                {
                    return AccessTokenResult.NoToken();
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }
    }
}