namespace Authentication.Domain.Constants
{
    public static class AuthConstants
    {
        public static class ProviderNames
        {
            public const string Microsoft = "Microsoft";
            public const string Google = "Google";
        }
        public static class Cookies
        {
            public const string AccessToken = "access_token";
            public const string RefreshToken = "refresh_token";
            public const string RootPath = "/";
            public const string AuthPath = "/api/auth";
        }
        public static class Claims
        {
            public const string Role = "role";
            public const string DisplayName = "display_name";
            public const string MicrosoftObjectId = "oid";
            public const string Subject = "sub";
            public const string Email = "email";
            public const string PreferredUsername = "preferred_username";
            public const string Name = "name";
        }
    }
}
