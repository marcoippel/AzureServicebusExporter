using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AzureServicebusExporter.Middleware
{
    public interface IPrometheusMiddlewareService
    {
        Func<HttpContext, Func<Task>, Task> Get(IApplicationBuilder app);
    }
}