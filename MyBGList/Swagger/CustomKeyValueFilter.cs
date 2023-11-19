using System;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MyBGList.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyBGList.Swagger
{
	public class CustomKeyValueFilter:ISchemaFilter
	{
		public CustomKeyValueFilter()
		{
		}

        void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var caProvider = context.MemberInfo ?? context.ParameterInfo as ICustomAttributeProvider;
            var attributes = caProvider?.GetCustomAttributes(true).OfType<CustomKeyValueAttribute>();
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    schema.Extensions.Add(attr.Key, new OpenApiString(attr.Value));
                }
            }
        }
    }
}

