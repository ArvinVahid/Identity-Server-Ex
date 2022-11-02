using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Basic.CustomPolicyProvider
{
    public class SecurityLevelAttribute : AuthorizeAttribute
    {
        public SecurityLevelAttribute(int level)
        {
            Policy = $"{DynamicPolicies.SecurityLevel}.{level}";
        }
    }
    
    // types
    public static class DynamicPolicies
    {
        public static IEnumerable<string> Get()
        {
            yield return SecurityLevel;
            yield return Rank;
        } 

        public const string SecurityLevel = "SecurityLevel";
        public const string Rank = "Rank";
    }

    public class SecurityLevelRequirement : IAuthorizationRequirement
    {
        public SecurityLevelRequirement(int level)
        {
            Level = level;
        }
        public int Level { get; }
    }
    
    public static class DynamicAuthorizationPolicyFactory
    {
        public static AuthorizationPolicy Create(string policyName)
        {
            var parts = policyName.Split('.');
            var type = parts[0];
            var value = parts[1];
            
            switch (type)
            {
                case DynamicPolicies.Rank:
                    return new AuthorizationPolicyBuilder().RequireClaim("Rank", value).Build();
                
                case DynamicPolicies.SecurityLevel:
                    return new AuthorizationPolicyBuilder()
                        .AddRequirements(new SecurityLevelRequirement(int.Parse(value)))
                        .Build();

                default:
                    return null;
            }
        }
    }

    public class SecurityLevelHandler : AuthorizationHandler<SecurityLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SecurityLevelRequirement requirement)
        {
            var claimValue = int.Parse(context.User.Claims
                .FirstOrDefault(x => x.Type == DynamicPolicies.SecurityLevel)?.Value ?? "0");

            if (requirement.Level >= claimValue){
                
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }
        
        // type.value
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            foreach (var customPolicy in DynamicPolicies.Get())
            {
                if (policyName.StartsWith(customPolicy))
                {
                    var policy = DynamicAuthorizationPolicyFactory.Create(policyName);
                    return Task.FromResult(policy);
                }
            }
            
            return base.GetPolicyAsync(policyName);
        }
    }
}
