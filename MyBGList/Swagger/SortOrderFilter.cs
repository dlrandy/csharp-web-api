﻿using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using MyBGList.Attributes;
using System;
namespace MyBGList.Swagger
{
	public class SortOrderFilter : IParameterFilter
	{
		public SortOrderFilter()
		{
		}

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var attributes = context.ParameterInfo?
                .GetCustomAttributes(true)
                .Union(
                    context.ParameterInfo.ParameterType.GetProperties()
                    .Where(p => p.Name == parameter.Name)
                    .SelectMany(p => p.GetCustomAttributes(true))
                 )
                .OfType<SortOrderValidatorAttribute>();
            Console.WriteLine(attributes);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    parameter.Schema.Extensions.Add("pattern",
                        new OpenApiString(string.Join("|", attribute.AllowedValues.Select(v => $"^{v}$")))
                        );
                }
            }
        }
    }
}

