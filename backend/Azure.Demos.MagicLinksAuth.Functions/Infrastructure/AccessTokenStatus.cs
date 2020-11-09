namespace Azure.Demos.MagicLinksAuth.Functions.Infrastructure
{
    public enum AccessTokenStatus
    {
        Valid,
        Expired,
        Error,
        NoToken
    }
}