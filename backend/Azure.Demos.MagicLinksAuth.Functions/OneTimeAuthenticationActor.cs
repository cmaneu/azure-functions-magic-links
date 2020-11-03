using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Azure.Demos.MagicLinksAuth.Functions
{
    public interface IOneTimeAuthenticationActor
    {
        void SetActor(string email, string name, string jwtToken, string redirectUri);
        bool UseToken();
        void Delete();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class OneTimeAuthenticationActor : IOneTimeAuthenticationActor
    {
        [JsonProperty("used")]
        public bool IsTokenUsed { get; set; }

        [JsonProperty("email")]
        public string UserEmail { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("token")]
        public string JwtToken { get; set; }

        [JsonProperty("redirect")]
        public string RedirectUri { get; set; }

        public void SetActor(string email, string name, string jwtToken, string redirectUri)
        {
            UserEmail = email;
            UserName = name;
            JwtToken = jwtToken;
            RedirectUri = redirectUri;
        }

        public bool UseToken()
        {
            if (string.IsNullOrEmpty(JwtToken) || IsTokenUsed)
                return false;

            IsTokenUsed = true;
            return IsTokenUsed;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(OneTimeAuthenticationActor))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<OneTimeAuthenticationActor>();
    }
}
