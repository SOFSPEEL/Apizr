﻿using Apizr.Caching;
using Apizr.Logging;
using Apizr.Policing;
using Apizr.Requesting;
using HttpTracer;
using Newtonsoft.Json;

namespace Apizr.Sample.Api.Models
{
    [CrudEntity("https://reqres.in/api/users", typeof(int), typeof(PagedResult<>))]
    [CacheReadAll(CacheMode.GetAndFetch)]
    [CacheRead(CacheMode.GetOrFetch)]
    [LogAll(HttpMessageParts.None, ApizrLogLevel.None)]
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
