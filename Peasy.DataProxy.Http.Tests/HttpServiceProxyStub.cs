using Orders.com.DAL.Http;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Peasy.DataProxy.Http.Tests
{
    public class HttpServiceProxyStub : HttpServiceProxyBase<Customer, long>
    {
        private HttpClient _client;

        public bool OnParseResponseWasInvoked { get; private set; } = false;
        public bool BuildConfiguredClientWasInvoked { get; private set; } = false;
        public bool GetMediaTypeFormatterWasInvoked { get; private set; } = false;
        public bool OnFormatServerErrorWasInvoked { get; private set; } = false;

        public MediaTypeFormatter Formatter { get; set; }

        public HttpServiceProxyStub(HttpClient client)
        {
            _client = client;
        }

        protected override string RequestUri { get { return "http://api/customers"; } }

        protected override MediaTypeFormatter GetMediaTypeFormatter()
        {
            GetMediaTypeFormatterWasInvoked = true;
            return Formatter ?? base.GetMediaTypeFormatter();
        }

        protected override HttpClient BuildConfiguredClient()
        {
            BuildConfiguredClientWasInvoked = true;
            return _client;
        }

        protected override void OnParseResponse<TOut>(HttpResponseMessage response, TOut entity)
        {
            OnParseResponseWasInvoked = true;
            base.OnParseResponse<TOut>(response, entity);
        }

        protected override string OnFormatServerError(string message)
        {
            OnFormatServerErrorWasInvoked = true;
            return message;
        }
    }
}
