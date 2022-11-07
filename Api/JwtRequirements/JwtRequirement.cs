using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Api.JwtRequirements
{
    public class JwtRequirement : IAuthorizationRequirement
    {
        public class JwtRequirementHandler : AuthorizationHandler<JwtRequirement>
        {
            private readonly HttpClient _client;
            private readonly HttpContext _httpContext;
            public JwtRequirementHandler(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            {
                _client = httpClientFactory.CreateClient();
                _httpContext = httpContextAccessor.HttpContext;
            }


            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context, 
                JwtRequirement requirement)
            {
                if (_httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var accessToken = authHeader.ToString().Split(' ')[1];
                    var response = await _client
                        .GetAsync($"https://localhost:44324/oauth/validate?access_token={accessToken}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}
