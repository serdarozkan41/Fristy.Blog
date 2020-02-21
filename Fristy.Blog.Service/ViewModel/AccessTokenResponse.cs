using System;

namespace Fristy.Blog.Service
{
    public sealed class AccessTokenResponse
    {
        public string AccessToken { get; set; }

        public DateTime Expireation { get; set; }

    }
}
