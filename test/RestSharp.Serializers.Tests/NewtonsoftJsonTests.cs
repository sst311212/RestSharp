using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;

namespace RestSharp.Serializers.Tests; 

public class NewtonsoftJsonTests {
    static readonly Fixture Fixture = new();

    string _body;

    readonly JsonSerializerSettings _jsonSerializerSettings = new() {
        ContractResolver = new DefaultContractResolver {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.None
    };

    void CaptureBody(HttpListenerRequest request, HttpListenerResponse response) => _body = request.InputStream.StreamToString();

    [Fact]
    public void Serialize_multiple_objects_within_one_thread() {
        var serializer             = new JsonNetSerializer();
        var dummy1                 = Fixture.Create<TestClass>();
        var dummy2                 = Fixture.Create<TestClass>();
        var dummy3                 = Fixture.Create<TestClass>();
        var expectedSerialization1 = JsonConvert.SerializeObject(dummy1, _jsonSerializerSettings);
        var expectedSerialization2 = JsonConvert.SerializeObject(dummy2, _jsonSerializerSettings);
        var expectedSerialization3 = JsonConvert.SerializeObject(dummy3, _jsonSerializerSettings);

        var actualSerialization1 = serializer.Serialize(dummy1);
        var actualSerialization2 = serializer.Serialize(dummy2);
        var actualSerialization3 = serializer.Serialize(dummy3);

        actualSerialization1.Should().Be(expectedSerialization1);
        actualSerialization2.Should().Be(expectedSerialization2);
        actualSerialization3.Should().Be(expectedSerialization3);
    }

    [Fact]
    public void Serialize_within_multiple_threads() {
        var serializer = new JsonNetSerializer();

        Parallel.For(
            0,
            100,
            _ => {
                var dummy                 = Fixture.Create<TestClass>();
                var expectedSerialization = JsonConvert.SerializeObject(dummy, _jsonSerializerSettings);
                var actualSerialization   = serializer.Serialize(dummy);

                actualSerialization.Should().Be(expectedSerialization);
            }
        );
    }

    [Fact]
    public void Use_JsonNet_For_Requests() {
        using var server = HttpServerFixture.StartServer(CaptureBody);
        _body = null;
        var serializer = new JsonNetSerializer();

        var testData = Fixture.Create<TestClass>();

        var client  = new RestClient(server.Url).UseNewtonsoftJson();
        var request = new RestRequest().AddJsonBody(testData);

        client.Post(request);

        var actual = serializer.Deserialize<TestClass>(new RestResponse { Content = _body! });

        actual.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public void Use_JsonNet_For_Response() {
        var expected = Fixture.Create<TestClass>();

        using var server = HttpServerFixture.StartServer(
            (_, response) => {
                var serializer = new JsonNetSerializer();

                response.ContentType     = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.OutputStream.WriteStringUtf8(serializer.Serialize(expected)!);
            }
        );

        var client = new RestClient(server.Url).UseNewtonsoftJson();

        var actual = client.Get<TestClass>(new RestRequest()).Data;

        actual.Should().BeEquivalentTo(expected);
    }
}