#region Includes
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
#endregion
#region Internal Includes
using Basic.Web.Functional;
using Basic.Web.Functional.Extensions;
#endregion

namespace Basic.Web {
    
    ///<summary>Extra basic web server configuration class.</summary>
    ///<remarks>The goal of this startup class is to be extra basic with just embedded responses</remarks>
    public partial class ExtraBasicWebServerStartup {
        
        private static readonly Dictionary<int, string> Users = new Dictionary<int, string>()
            { 
                { 1, "John" },
                { 2, "Jane" },
                { 3, "Cheetah" },
            };


        public void Configure(IApplicationBuilder appBuilder) =>
        
            appBuilder.Run(async (HttpContext context) => 
            {   
                var users = ExtraBasicWebServerStartup.Users;
                var idOrName = context.Request.Path.ToString().Substring(1);
                idOrName
                    .Validate( (v) => v.All(char.IsNumber) )
                    .When<string, int>( (v) => int.Parse(v) )
                    .Alternative(idOrName)
                    .When<int, KeyValuePair<int, string>>( (id) => users.FirstOrDefault( (kv) => kv.Key == id ) )
                    .When<string, KeyValuePair<int, string>>( (name) => users.FirstOrDefault( (kv) => kv.Value == name ) )
                    .Validate<(int id, string name)?>( (IMaybe maybe) => 
                        maybe.IsValid && maybe.AsValid<KeyValuePair<int, string>>().Value.Value is not null
                            ? (maybe.AsValid<KeyValuePair<int, string>>().Value.Key, maybe.AsValid<KeyValuePair<int, string>>().Value.Value)
                            : null
                    )
                    .When<(int id, string name)?>( async (user) => await context.Response.WriteAsync($"User is {user?.id}:{user?.name}") )
                    .WhenInvalid( async () => await context.Response.WriteAsync($"{idOrName} is not a valid user id or user name") );
            });

    }
}