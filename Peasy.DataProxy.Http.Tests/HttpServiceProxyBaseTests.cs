using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Orders.com.DAL.Http;
using Peasy.Core;
using Peasy.Exception;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peasy.DataProxy.Http.Tests
{
    public class HttpServiceProxyTestStub : HttpServiceProxyBase<Customer, long>
    {
        private HttpClient _client;

        public bool OnParseResponseWasInvoked { get; private set; } = false;
        public bool BuildConfiguredClientWasInvoked { get; private set; } = false;
        public bool GetMediaTypeFormatterWasInvoked { get; private set; } = false;
        public bool OnFormatServerErrorWasInvoked { get; private set; } = false;

        public MediaTypeFormatter Formatter { get; set; }

        public HttpServiceProxyTestStub(HttpClient client)
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

    [TestClass]
    public class HttpServiceProxyBaseTests
    {
        [TestMethod]
        public void GetAll_invokes_an_HTTP_GET_against_the_correct_uri()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}]", Encoding.UTF8, "application/json") }))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    r.RequestUri.AbsoluteUri.ShouldBe("http://api/customers");
                    Assert.AreEqual(HttpMethod.Get, r.Method);
                });

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object));
            proxy.GetAll();
        }

        [TestMethod]
        public void GetAll_invokes_expected_virtual_methods()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object));
            var customers = proxy.GetAll();

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public void GetAll_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                                        {
                                            Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                                        }));

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object));

            try
            {
                var customers = proxy.GetAll();
            }
            catch (System.AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<NotImplementedException>();
            }
        }

        [TestMethod]
        public void GetAll_throws_ServiceException_when_server_status_code_is_BAD_REQUEST()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                                        {
                                            Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                                        }));

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object));

            try
            {
                var customers = proxy.GetAll();
            }
            catch (System.AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<ServiceException>();
            }
        }

        [TestMethod]
        public void GetAll_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            try
            {
                var customers = proxy.GetAll();
            }
            catch (System.AggregateException ex)
            {
                ex.GetBaseException().ShouldBeOfType<UnsupportedMediaTypeException>();
            }
        }

        [TestMethod]
        public void GetAll_returns_enumerable_list()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyTestStub(new HttpClient(handler.Object));
            var customers = proxy.GetAll();

            customers.FirstOrDefault(f => f.ID == 1).ShouldNotBeNull();
            customers.FirstOrDefault(f => f.ID == 2).ShouldNotBeNull();
        }

        [TestMethod]
        public void GetByID_returns_an_item()
        {

        }

        [TestMethod]
        public void GetByID_throws_DomainObjectNotFound_on_404()
        {

        }

        [TestMethod]
        public void Insert_returns_an_item()
        {

        }


    }
}
