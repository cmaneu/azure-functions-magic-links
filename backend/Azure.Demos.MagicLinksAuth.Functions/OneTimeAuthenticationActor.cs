using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        public void SetRequestId(string requestId)
        {
            RequestId = requestId;
        }

        public void SetEmail(string email)
        {
            RequestId = Entity.Current.EntityKey;
            UserEmail = email;
            JwtToken = "jwtToken";
            RedirectUri = $"https://localhost:44355/auth/callback/{JwtToken}";
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
