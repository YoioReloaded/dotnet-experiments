#region Includes
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
#endregion
#region Internal Includes
using Basic.Web;
#endregion

// Create the host and run it
Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults( webBuilder => { webBuilder.UseStartup<ExtraBasicWebServerStartup>(); })
    .StartAsync();
