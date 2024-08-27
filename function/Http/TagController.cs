using System.Net.Http;
using System.Threading.Tasks;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.Security.Helper;
using AHI.Infrastructure.Service.Tag.Model;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;

namespace Function.Http
{
    public class TagController
    {
        private readonly ITenantContext _tenantContext;
        private readonly IConfiguration _configuration;
        private readonly ITagService _tagService;

        public TagController(
                IConfiguration configuration,
                ITenantContext tenantContext,
                ITagService tagService)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _tagService = tagService;
        }
        [FunctionName("DeleteTagBinding")]
        public async Task<IActionResult> DeleteTagBinding([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "fnc/tbl/tags")] HttpRequestMessage req, ExecutionContext context)
        {
            if (!await SecurityHelper.AuthenticateRequestAsync(req, _configuration))
                return new UnauthorizedResult();

            _tenantContext.RetrieveFromHeader(req.Headers);
            var content = await req.Content.ReadAsByteArrayAsync();
            var deleteTagMessage = content.Deserialize<DeleteTagMessage>();
            await _tagService.DeleteTagsAsync(deleteTagMessage.TagIds);
            return new OkResult();
        }
    }
}
