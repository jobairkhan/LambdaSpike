using System;
using Microsoft.AspNetCore.Mvc;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace Authentication.Controllers
{
    [Route("token")]
    public class TokenController : Controller
    {
        IMemcachedClient memcachedClient;

        public TokenController(IMemcachedClient memcachedClient)
        {
            this.memcachedClient = memcachedClient;
        }

        [HttpGet("{key}")]
        public TokenData Get(string key)
        {
            key = $"TEMP{key}";

            var result = memcachedClient.Get<TokenData>(key);

            return (result != null) ? result : new TokenData() { Token = "NO DATA", EventTypes = new string[0] { } };
        }

        [HttpPost]
        public string Post([FromBody]TokenData token)
        {
            var key = $"TEMP{token.Token}";

            var value = memcachedClient.Get(key);
            var message = "ADDED";

            if (value == null)
            {
                memcachedClient.Store(StoreMode.Add, key, token, TimeSpan.FromMinutes(1));
            }
            else
            {
                message = "REPLACED";
                memcachedClient.Store(StoreMode.Replace, key, token, TimeSpan.FromMinutes(1));
            }

            return message;
        }
    }

    public class TokenData
    {
        public string Token { get; set; }

        public string[] EventTypes { get; set; }
    }
}
