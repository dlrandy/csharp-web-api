﻿using System;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
namespace MyBGList.Swagger
{
	public class AuthRequirementFilter:IOperationFilter
	{
		public void Apply(OpenApiOperation operation,OperationFilterContext context)
		{
			if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
			{
				return;
			}
			operation.Security = new List<OpenApiSecurityRequirement> {

				new OpenApiSecurityRequirement{

					{
						new OpenApiSecurityScheme{

							Name = "Bearer",
							In = ParameterLocation.Header,
							Reference = new OpenApiReference{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[]{ }

					}
				}

			};
		}
	}
}

