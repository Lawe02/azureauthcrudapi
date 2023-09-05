using FunctionsApi;  // This should be the namespace of your Functions project
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using FunctionsApi.Services;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerBuilder =>
    {
        workerBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,  
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "https://localhost:4200",
                    ValidAudience = "https://localhost:4200",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QJF0R46v4nYFL6PilN2bMd1VhxvG3uJHpdSH28mg"))
                };
            });

        //workerBuilder.Services.AddCors(options =>
        //{
        //    options.AddDefaultPolicy(builder =>
        //    {
        //        builder.WithOrigins("https://localhost:4200")
        //            .AllowAnyHeader()
        //            .AllowAnyMethod();
        //    });
        //});

        // Add other services if needed

        // Register your functions

        workerBuilder.Services.AddTransient<LoginService>();
        workerBuilder.Services.AddTransient<BookService>();
    })
    .Build();

host.Run();
