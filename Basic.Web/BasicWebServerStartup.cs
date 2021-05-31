#region Includes
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endregion

namespace Basic.Web {
    
    ///<summary>Extra basic web server configuration class.</summary>
    ///<remarks>The goal of this startup class is to be extra basic with just embedded responses</remarks>
    public partial class ExtraBasicWebServerStartup {

        public void Configure(IApplicationBuilder appBuilder) =>
        
            appBuilder.Run(async (HttpContext context) => 
            {   
                string requestUrl = " *not set*";
                try
                {
                    var req = context.Request;
                    requestUrl = $" {req.Method} {req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
                }
                finally
                {
                    await context.Response.WriteAsync(
$@"
Request ---{requestUrl}
");
                }
            });

    }
}