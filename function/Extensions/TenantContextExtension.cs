using System.Linq;
using System.Net.Http;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace Function.Extension
{
    public static class TenantContextExtension
    {
        public static void MapTenantInfo(this ITenantContext tenantContext, HttpRequestMessage request)
        {
            var tenantId = request.Headers.GetValues("x-tenant-id").FirstOrDefault();
            var subscriptionId = request.Headers.GetValues("x-subscription-id").FirstOrDefault();
            var projectId = request.Headers.GetValues("x-project-id").FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantId))
            {
                tenantContext.SetTenantId(tenantId);
            }
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                tenantContext.SetSubscriptionId(subscriptionId);
            }
            if (!string.IsNullOrEmpty(projectId))
            {
                tenantContext.SetProjectId(projectId);
            }
        }
    }
}