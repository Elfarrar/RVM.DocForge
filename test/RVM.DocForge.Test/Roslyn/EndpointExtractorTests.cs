using Microsoft.CodeAnalysis.CSharp;
using RVM.DocForge.API.Services.Roslyn;

namespace RVM.DocForge.Test.Roslyn;

public class EndpointExtractorTests
{
    [Fact]
    public void Should_Extract_HttpGet_Endpoint()
    {
        var code = """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/[controller]")]
            public class UsersController : ControllerBase
            {
                [HttpGet]
                public IActionResult GetAll() => Ok();
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Endpoints);
        var ep = extractor.Endpoints[0];
        Assert.Equal("UsersController", ep.Controller);
        Assert.Equal("GetAll", ep.Action);
        Assert.Equal("GET", ep.HttpMethod);
        Assert.Contains("api/[controller]", ep.Route);
    }

    [Fact]
    public void Should_Extract_HttpPost_With_Route()
    {
        var code = """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/users")]
            public class UsersController : ControllerBase
            {
                [HttpPost("create")]
                public IActionResult Create([FromBody] string request) => Ok();
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Endpoints);
        var ep = extractor.Endpoints[0];
        Assert.Equal("POST", ep.HttpMethod);
        Assert.Contains("create", ep.Route);
    }

    [Fact]
    public void Should_Extract_Parameters_With_Attributes()
    {
        var code = """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/users")]
            public class UsersController : ControllerBase
            {
                [HttpGet("{id}")]
                public IActionResult GetById([FromRoute] int id, [FromQuery] string filter) => Ok();
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Endpoints);
        var ep = extractor.Endpoints[0];
        Assert.Equal(2, ep.Parameters.Count);
        Assert.Equal("Route", ep.Parameters[0].Source);
        Assert.Equal("Query", ep.Parameters[1].Source);
    }

    [Fact]
    public void Should_Extract_Multiple_Endpoints()
    {
        var code = """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/items")]
            public class ItemsController : ControllerBase
            {
                [HttpGet]
                public IActionResult GetAll() => Ok();

                [HttpPost]
                public IActionResult Create() => Ok();

                [HttpDelete("{id}")]
                public IActionResult Delete(int id) => Ok();
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Equal(3, extractor.Endpoints.Count);
        Assert.Contains(extractor.Endpoints, e => e.HttpMethod == "GET");
        Assert.Contains(extractor.Endpoints, e => e.HttpMethod == "POST");
        Assert.Contains(extractor.Endpoints, e => e.HttpMethod == "DELETE");
    }

    [Fact]
    public void Should_Ignore_Non_Controller_Classes()
    {
        var code = """
            public class MyService
            {
                public void DoWork() { }
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Empty(extractor.Endpoints);
    }

    [Fact]
    public void Should_Extract_XmlDoc_Summary()
    {
        var code = """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/items")]
            public class ItemsController : ControllerBase
            {
                /// <summary>
                /// Gets all items
                /// </summary>
                [HttpGet]
                public IActionResult GetAll() => Ok();
            }
            """;

        var extractor = new EndpointExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Endpoints);
        Assert.NotNull(extractor.Endpoints[0].Summary);
        Assert.Contains("Gets all items", extractor.Endpoints[0].Summary);
    }
}
