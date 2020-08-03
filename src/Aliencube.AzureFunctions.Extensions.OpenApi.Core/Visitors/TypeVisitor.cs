using System;
using System.Collections.Generic;
using System.Linq;

using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Extensions;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Core.Visitors
{
    /// <summary>
    /// This represents the visitor entity for type. This MUST be inherited.
    /// </summary>
    public abstract class TypeVisitor : IVisitor
    {
        /// <summary>
        /// Gets the <see cref="Type"/> object.
        /// </summary>
        protected Type Type { get; private set; }

        /// <inheritdoc />
        public virtual bool IsVisitable(Type type)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void Visit(IAcceptor acceptor, KeyValuePair<string, Type> type, NamingStrategy namingStrategy, params Attribute[] attributes)
        {
            return;
        }

        /// <inheritdoc />
        public virtual bool IsNavigatable(Type type)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual bool IsParameterVisitable(Type type)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual OpenApiSchema ParameterVisit(Type type, NamingStrategy namingStrategy)
        {
            return default;
        }

        /// <inheritdoc />
        public virtual bool IsPayloadVisitable(Type type)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual OpenApiSchema PayloadVisit(Type type, NamingStrategy namingStrategy)
        {
            return default;
        }

        /// <summary>
        /// Checks whether the type is visitable or not.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="code"><see cref="TypeCode"/> value.</param>
        /// <returns>Returns <c>True</c>, if the type is visitable; otherwise returns <c>False</c>.</returns>
        protected bool IsVisitable(Type type, TypeCode code)
        {
            var @enum = Type.GetTypeCode(type);
            var isVisitable = @enum == code;

            if (isVisitable)
            {
                this.Type = type;
            }

            return isVisitable;
        }

        /// <summary>
        /// Checks whether the type can be referenced or not.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>Returns <c>True</c>, if the type can be referenced; otherwise returns <c>False</c>.</returns>
        protected bool IsReferential(Type type)
        {
            var @enum = Type.GetTypeCode(type);
            var isReferential = @enum == TypeCode.Object;

            if (type == typeof(Guid))
            {
                isReferential = false;
            }
            if (type == typeof(DateTime))
            {
                isReferential = false;
            }
            if (type == typeof(DateTimeOffset))
            {
                isReferential = false;
            }
            if (type.IsOpenApiNullable())
            {
                isReferential = false;
            }
            if (type.IsUnflaggedEnumType())
            {
                isReferential = false;
            }
            if (type.IsJObjectType())
            {
                isReferential = false;
            }

            return isReferential;
        }

        /// <summary>
        /// Visits and processes the acceptor.
        /// </summary>
        /// <param name="acceptor"><see cref="IAcceptor"/> instance.</param>
        /// <param name="name">Property name.</param>
        /// <param name="title">Type name.</param>
        /// <param name="dataType">Data type.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <param name="attributes">List of attribute instances.</param>
        /// <returns>Returns the name as the schema key.</returns>
        protected string Visit(IAcceptor acceptor, string name, string title, string dataType, string dataFormat, params Attribute[] attributes)
        {
            var instance = acceptor as OpenApiSchemaAcceptor;
            if (instance.IsNullOrDefault())
            {
                return null;
            }

            if (instance.Schemas.ContainsKey(name))
            {
                return null;
            }

            var schema = new OpenApiSchema()
            {
                Title = title,
                Type = dataType,
                Format = dataFormat
            };

            // Adds the visibility property.
            if (attributes.Any())
            {
                var visibilityAttribute = attributes.OfType<OpenApiSchemaVisibilityAttribute>().SingleOrDefault();
                if (!visibilityAttribute.IsNullOrDefault())
                {
                    var extension = new OpenApiString(visibilityAttribute.Visibility.ToDisplayName());

                    schema.Extensions.Add("x-ms-visibility", extension);
                }
            }

            instance.Schemas.Add(name, schema);

            return name;
        }

        /// <summary>
        /// Visits and processes the <see cref="OpenApiSchema"/> for parameters.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <returns>Returns <see cref="OpenApiSchema"/> instance.</returns>
        protected OpenApiSchema ParameterVisit(string dataType, string dataFormat)
        {
            var schema = new OpenApiSchema()
            {
                Type = dataType,
                Format = dataFormat
            };

            return schema;
        }

        /// <summary>
        /// Visits and processes the <see cref="OpenApiSchema"/> for payloads.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <returns>Returns <see cref="OpenApiSchema"/> instance.</returns>
        protected OpenApiSchema PayloadVisit(string dataType, string dataFormat)
        {
            var schema = new OpenApiSchema()
            {
                Type = dataType,
                Format = dataFormat
            };

            return schema;
        }
    }
}