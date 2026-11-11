using System.Reflection;
using Microsoft.OpenApi;

namespace DireitoFacil.Api.Extensions;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddDireitoFacilSwagger(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = configuration["Swagger:Title"] ?? "DireitoFácil API",
                Version = "v1",
                Description = configuration["Swagger:Description"]
                              ?? "Documentos e pedidos de direitos sociais do cidadão (CitizenTech)."
            });

            // Botão "Authorize": cole o accessToken retornado por POST /api/auth/login.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Cole apenas o token JWT retornado no login (sem a palavra Bearer)."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xml))
                options.IncludeXmlComments(xml, includeControllerXmlComments: true);
        });

        return services;
    }
}
