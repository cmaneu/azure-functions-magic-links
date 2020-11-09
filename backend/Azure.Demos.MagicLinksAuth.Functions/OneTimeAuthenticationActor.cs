using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Azure.Demos.MagicLinksAuth.Functions
{
    public interface IOneTimeAuthenticationActor
    {
        void SetRequestId(string requestId);
        void SetEmail(string email);
        void Delete();
        void SendInvite();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class OneTimeAuthenticationActor : IOneTimeAuthenticationActor
    {
        private IOptions<ApplicationSettings> _applicationSettingsOptions;

        [JsonProperty("email")]
        public string UserEmail { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("token")]
        public string JwtToken { get; set; }

        [JsonProperty("redirect")]
        public string RedirectUri { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }


        public OneTimeAuthenticationActor(IOptions<ApplicationSettings> applicationSettingsOptions)
        {
            _applicationSettingsOptions = applicationSettingsOptions;
        }

        public void SetRequestId(string requestId)
        {
            RequestId = requestId;
        }

        public void SetEmail(string email)
        {
            RequestId = Entity.Current.EntityKey;
            UserEmail = email;
            JwtToken = GenerateJwtToken(email);
            RedirectUri = $"https://localhost:44355/.auth/callback/{JwtToken}";
        }

        private string GenerateJwtToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_applicationSettingsOptions.Value.JwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.GivenName, "User")
                }),
                Expires = DateTime.UtcNow.AddDays(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = "https://courses.scubabous.fr",
                Issuer = "https://courses.scubabous.fr"
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        public void SendInvite()
        {
            
        }

        [FunctionName(nameof(OneTimeAuthenticationActor))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<OneTimeAuthenticationActor>();
    }
}
