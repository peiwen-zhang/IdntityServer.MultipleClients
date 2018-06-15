using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyIdentityServer.TMP
{
    public class MyInteractionResponseGenerator : AuthorizeInteractionResponseGenerator
    {
        public MyInteractionResponseGenerator(ISystemClock clock, ILogger<AuthorizeInteractionResponseGenerator> logger, IConsentService consent, IProfileService profile)
            : base(clock, logger, consent, profile)
        {

        }

        public override async Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            var result = await base.ProcessInteractionAsync(request, consent);

            //用户账户分组设置
            //var client = request.ClientId;
            //var clientGroup = GetClientGroup(client);
            //var user = ??????????????;
            //if (user.IsAuthenticated() && !result.IsConsent && !user.Claims.Any(x => x.Type == "client_group" && x.Value == clientGroup))
            //{
            //    result.IsLogin = true;
            //}

            if ((request.ClientId == "netcore.client") && (request.Subject.Claims.FirstOrDefault(x => x.Type == request.ClientId) == null))
            {
                result.IsLogin = true;
            }

            return result;
        }

        private string GetClientGroup(string client)
        {
            switch (client)
            {
                case "mvc2":
                    return "Group1";
                case "mvc3":
                    return "Group2";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
