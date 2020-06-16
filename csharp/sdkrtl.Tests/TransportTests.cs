using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Looker.RTL;
using Xunit;

namespace sdkrtl.Tests
{
    public class TransportTests
    {
        /**
         * These tests require the local Looker server is running and versions can be retrieved
         */
        private const string HtmlUrl = "https://github.com/looker-open-source/sdk-codegen";

        private const string HtmlContent = "One SDK to rule them all";

        private TestConfig _config;
        private dynamic _contentTypes;
        
        public TransportTests()
        {
            _config = new TestConfig();
            _contentTypes = _config.TestData["content_types"];
        }

        [Fact]
        public void BinaryModeTest()
        {
            var contents = _contentTypes["binary"];
            Assert.NotNull(contents);
            foreach (var content in contents)
            {
                var s = Convert.ToString(content);
                var actual = Constants.ResponseMode(s);
                if (actual != ResponseMode.Binary)
                {
                    Console.WriteLine($"{s} is not binary");
                }
                Assert.Equal(ResponseMode.Binary, actual);
            }
        }
        
        [Fact]
        public void StringModeTest()
        {
            var contents = _contentTypes["string"];
            Assert.NotNull(contents);
            foreach (var content in contents)
            {
                var s = Convert.ToString(content);
                var actual = Constants.ResponseMode(s);
                if (actual != ResponseMode.String)
                {
                    Console.WriteLine($"{s} is not test/string");
                }
                Assert.Equal(ResponseMode.String, actual);
            }
        }

        [Fact]
        public void EncodeParamTest()
        {
            // TODO figure out the always painful DateTime conversions
            // var date = DateTime.Parse("2020-01-01T14:48:00.00Z");
            // Assert.Equal("2020-01-01T14%3A48%3A00.000Z", Transport.EncodeParam(date));
            Assert.Equal("foo%2Fbar", Transport.EncodeParam("foo%2Fbar"));
            Assert.Equal("foo%2Fbar", Transport.EncodeParam("foo/bar"));
            var actual = Transport.EncodeParam(true);
            Assert.Equal("true", actual);
            actual = Transport.EncodeParam(2.3);
            Assert.Equal("2.3", actual);

        }
        [Fact]
        public async Task GetHtmlUrlTest()
        {
            var xp = new Transport(_config.Settings);
            var actual = await xp.RawRequest(
                HttpMethod.Get,
                HtmlUrl
            );
            Assert.True(actual.Ok);
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Contains("text/html", actual.ContentType);
            var content = actual.Body.ToString();
            Assert.NotNull(content);
            Assert.Contains(HtmlContent, content);
        }

        [Fact]
        public async Task GetJsonUrlTest()
        {
            var xp = new Transport(_config.Settings);
            var actual = await xp.RawRequest(
                HttpMethod.Get,
                "/versions"
            );
            Assert.True(actual.Ok);
            Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
            Assert.Contains("application/json", actual.ContentType);
            var content = actual.Body.ToString();
            Assert.NotNull(content);
            Assert.Contains("looker_release_version", content);
            var json = JsonSerializer.Deserialize<Values>(content);
            Assert.Equal(json.Keys, new string[] {"looker_release_version", "current_version", "supported_versions"});
        }

        [Fact]
        public async Task GetTypedUrlTest()
        {
            var xp = new Transport(_config.Settings);
            var actual = await xp.Request<Values, Exception>(
                HttpMethod.Get,
                "/versions"
            );
            Assert.True(actual.Ok);
            Assert.Equal(actual.Value.Keys,
                new string[] {"looker_release_version", "current_version", "supported_versions"});
        }
    }
}