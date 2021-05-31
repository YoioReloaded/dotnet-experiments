#region Includes
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endregion
#region Internal Includes
using Basic.Web.Internals.Extensions;
#endregion

namespace Basic.Web {
    
    ///<summary>Extra basic web server configuration class.</summary>
    ///<remarks>The goal of this startup class is to be extra basic with just embedded responses</remarks>
    public partial class ExtraBasicWebServerStartup {

        public void Configure(IApplicationBuilder appBuilder) =>
        
            appBuilder.Run(async (HttpContext context) => 
            {   
                string requestProps = " *not set*";
                try
                {
                    requestProps = context.Request.Serialize(depth:3);
                }
                finally
                {
                    await context.Response.WriteAsync(
$@"
Request ---{requestProps}
");
                }
            });

    }
}