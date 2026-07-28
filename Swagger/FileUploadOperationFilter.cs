using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PhotographyCMS.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
                return;

            var fromFormParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.GetCustomAttribute<FromFormAttribute>() != null)
                .ToList();

            var fileParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile)
                            || p.ParameterType == typeof(IFormFileCollection)
                            || (p.ParameterType.IsGenericType && p.ParameterType.GetGenericArguments().Any(t => t == typeof(IFormFile))))
                .ToList();

            var properties = new Dictionary<string, IOpenApiSchema>();

            foreach (var p in fileParams)
            {
                properties[p.Name!] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" };
            }

            foreach (var param in fromFormParams)
            {
                var dtoType = param.ParameterType;
                foreach (var prop in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (properties.ContainsKey(prop.Name))
                        continue;

                    properties[prop.Name] = GetSchemaForProperty(prop.PropertyType);
                }
            }

            if (!properties.Any())
                return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Properties = properties
                        }
                    }
                }
            };

            if (operation.Parameters != null && operation.Parameters.Any())
            {
                var removeNames = fileParams.Select(p => p.Name).ToHashSet();
                operation.Parameters = operation.Parameters.Where(p => !removeNames.Contains(p.Name)).ToList();
            }
        }

        private IOpenApiSchema GetSchemaForProperty(Type type)
        {
            if (type == typeof(IFormFile))
                return new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" };

            if (type == typeof(IFormFileCollection) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && type.GetGenericArguments()[0] == typeof(IFormFile)))
            {
                return new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" }
                };
            }

            if (type == typeof(string))
                return new OpenApiSchema { Type = JsonSchemaType.String };

            if (type == typeof(int) || type == typeof(int?))
                return new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" };

            if (type == typeof(bool) || type == typeof(bool?))
                return new OpenApiSchema { Type = JsonSchemaType.Boolean };

            if (type == typeof(long) || type == typeof(long?))
                return new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" };

            if (type == typeof(float) || type == typeof(float?) || type == typeof(double) || type == typeof(double?) || type == typeof(decimal) || type == typeof(decimal?))
                return new OpenApiSchema { Type = JsonSchemaType.Number };

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = type.GetGenericArguments()[0];
                return new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = GetSchemaForProperty(itemType)
                };
            }

            return new OpenApiSchema { Type = JsonSchemaType.String };
        }
    }
}
