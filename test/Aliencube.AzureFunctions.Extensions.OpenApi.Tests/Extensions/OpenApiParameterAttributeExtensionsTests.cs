using System;
using System.Collections.Generic;
using System.Linq;

using Aliencube.AzureFunctions.Extensions.OpenApi.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Enums;
using Aliencube.AzureFunctions.Extensions.OpenApi.Extensions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Tests.Fakes;

using FluentAssertions;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Tests.Extensions
{
    [TestClass]
    public class OpenApiParameterAttributeExtensionsTests
    {
        [TestMethod]
        public void Given_Null_When_ToOpenApiParameter_Invoked_Then_It_Should_Throw_Exception()
        {
            Action action = () => OpenApiParameterAttributeExtensions.ToOpenApiParameter(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Given_Value_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(string),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Name.Should().Be(attribute.Name);
            result.Description.Should().Be(attribute.Description);
            result.Required.Should().Be(attribute.Required);
            result.In.Should().Be(attribute.In);
        }

        [TestMethod]
        public void Given_Value_With_String_Type_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(string),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Schema.Type.Should().Be("string");
            result.Schema.Format.Should().BeNull();
        }

        [TestMethod]
        public void Given_Value_With_Int_Type_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(int),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Schema.Type.Should().Be("integer");
            result.Schema.Format.Should().Be("int32");
        }

        [TestMethod]
        public void Given_Value_With_Long_Type_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(long),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Schema.Type.Should().Be("integer");
            result.Schema.Format.Should().Be("int64");
        }

        [TestMethod]
        public void Given_Value_With_Enum_Type_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var strategy = new CamelCaseNamingStrategy();
            var names = typeof(FakeStringEnum).ToOpenApiStringCollection(strategy).Select(p => (p as OpenApiString).Value).ToList();
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(FakeStringEnum),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Query
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute, strategy);

            result.Style.Should().BeNull();
            result.Explode.Should().Be(attribute.Explode);

            result.Schema.Type.Should().Be("string");
            result.Schema.Format.Should().BeNull();

            result.Schema.Enum.Should().HaveCount(names.Count);
            (result.Schema.Default as OpenApiString).Value.Should().Be(names.First());
        }

        [TestMethod]
        public void Given_Value_With_List_Enum_Type_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var strategy = new CamelCaseNamingStrategy();
            var names = typeof(FakeStringEnum).ToOpenApiStringCollection(strategy).Select(p => (p as OpenApiString).Value).ToList();
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(List<FakeStringEnum>),
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Query
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute, strategy);

            result.Style.Should().Be(ParameterStyle.Form);
            result.Explode.Should().Be(attribute.Explode);

            result.Schema.Type.Should().Be("array");
            result.Schema.Format.Should().BeNull();
            result.Schema.Items.Type.Should().Be("string");
            result.Schema.Items.Enum.Should().HaveCount(names.Count);
            (result.Schema.Items.Default as OpenApiString).Value.Should().Be(names.First());
        }

        [TestMethod]
        public void Given_Value_With_Summary_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(long),
                Summary = "lorem ipsum",
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Extensions.Keys.Should().Contain("x-ms-summary");
            result.Extensions["x-ms-summary"].Should().BeOfType<OpenApiString>();
            (result.Extensions["x-ms-summary"] as OpenApiString).Value.Should().Be(attribute.Summary);
        }

        [TestMethod]
        public void Given_Value_With_Visibility_When_ToOpenApiParameter_Invoked_Then_It_Should_Return_Result()
        {
            var attribute = new OpenApiParameterAttribute("hello")
            {
                Type = typeof(long),
                Summary = "lorem ipsum",
                Description = "hello world",
                Required = true,
                In = ParameterLocation.Path,
                Visibility = OpenApiVisibilityType.Important
            };

            var result = OpenApiParameterAttributeExtensions.ToOpenApiParameter(attribute);

            result.Extensions.Keys.Should().Contain("x-ms-visibility");
            result.Extensions["x-ms-visibility"].Should().BeOfType<OpenApiString>();
            (result.Extensions["x-ms-visibility"] as OpenApiString).Value.Should().Be(attribute.Visibility.ToDisplayName());
        }
    }
}
