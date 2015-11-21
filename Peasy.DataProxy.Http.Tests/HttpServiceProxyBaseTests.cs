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
    [TestClass]
    public class HttpServiceProxyBaseTests
    {
        #region GetAll

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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.GetAll();
        }

        [TestMethod]
        public void GetAll_invokes_expected_virtual_methods()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.GetAll();

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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.GetAll();
            }
            catch (AggregateException ex)
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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.GetAll();
            }
            catch (AggregateException ex)
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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            try
            {
                proxy.GetAll();
            }
            catch (AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(false);
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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customers = proxy.GetAll();

            customers.FirstOrDefault(f => f.ID == 1).ShouldNotBeNull();
            customers.FirstOrDefault(f => f.ID == 2).ShouldNotBeNull();
        }

        [TestMethod]
        public async Task GetAll_invokes_an_HTTP_GET_against_the_correct_uri_async()
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

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.GetAllAsync();
        }

        [TestMethod]
        public async Task GetAll_invokes_expected_virtual_methods_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.GetAllAsync();

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task GetAll_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.GetAllAsync();
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public async Task GetAll_throws_ServiceException_when_server_status_code_is_BAD_REQUEST_Async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.GetAllAsync();
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedMediaTypeException))]
        public async Task GetAll_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            await proxy.GetAllAsync();
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public async Task GetAll_returns_enumerable_list_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customers = await proxy.GetAllAsync();

            customers.FirstOrDefault(f => f.ID == 1).ShouldNotBeNull();
            customers.FirstOrDefault(f => f.ID == 2).ShouldNotBeNull();
        }

        #endregion

        #region GetByID

        [TestMethod]
        public void GetByID_invokes_an_HTTP_GET_against_the_correct_uri()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    r.RequestUri.AbsoluteUri.ShouldBe("http://api/customers/1");
                    Assert.AreEqual(HttpMethod.Get, r.Method);
                });

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.GetByID(1);
        }

        [TestMethod]
        public void GetByID_invokes_expected_virtual_methods()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.GetByID(1);

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public void GetByID_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.GetByID(1);
            }
            catch (System.AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<NotImplementedException>();
            }
        }

        [TestMethod]
        public void GetByID_throws_ServiceException_when_server_status_code_is_BAD_REQUEST()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.GetByID(1);
            }
            catch (AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<ServiceException>();
            }
        }

        [TestMethod]
        public void GetByID_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            try
            {
                proxy.GetByID(1);
            }
            catch (AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(false);
                ex.GetBaseException().ShouldBeOfType<UnsupportedMediaTypeException>();
            }
        }

        [TestMethod]
        public void GetByID_returns_expected_item()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customer = proxy.GetByID(1);

            customer.ID.ShouldBe(1);
        }

        [TestMethod]
        public async Task GetByID_invokes_an_HTTP_GET_against_the_correct_uri_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    r.RequestUri.AbsoluteUri.ShouldBe("http://api/customers/1");
                    Assert.AreEqual(HttpMethod.Get, r.Method);
                });

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.GetByIDAsync(1);
        }

        [TestMethod]
        public async Task GetByID_invokes_expected_virtual_methods_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.GetByIDAsync(1);

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task GetByID_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.GetByIDAsync(1);
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public async Task GetByID_throws_ServiceException_when_server_status_code_is_BAD_REQUEST_Async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.GetByIDAsync(1);
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedMediaTypeException))]
        public async Task GetByID_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            await proxy.GetByIDAsync(1);
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public async Task GetByID_returns_expected_item_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customer = await proxy.GetByIDAsync(1);

            customer.ID.ShouldBe(1);
        }

        #endregion

        #region Insert

        [TestMethod]
        public void Insert_invokes_an_HTTP_GET_against_the_correct_uri()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    r.RequestUri.AbsoluteUri.ShouldBe("http://api/customers");
                    Assert.AreEqual(HttpMethod.Post, r.Method);
                });

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.Insert(new Customer());
        }

        [TestMethod]
        public void Insert_invokes_expected_virtual_methods()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            proxy.Insert(new Customer());

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public void Insert_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.Insert(new Customer());
            }
            catch (System.AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<NotImplementedException>();
            }
        }

        [TestMethod]
        public void Insert_throws_ServiceException_when_server_status_code_is_BAD_REQUEST()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            try
            {
                proxy.Insert(new Customer());
            }
            catch (AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
                ex.GetBaseException().ShouldBeOfType<ServiceException>();
            }
        }

        [TestMethod]
        public void Insert_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"ID\": 1}, {\"ID\":2}]", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            try
            {
                proxy.Insert(new Customer());
            }
            catch (AggregateException ex)
            {
                proxy.OnFormatServerErrorWasInvoked.ShouldBe(false);
                ex.GetBaseException().ShouldBeOfType<UnsupportedMediaTypeException>();
            }
        }

        [TestMethod]
        public void Insert_returns_expected_item()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customer = proxy.Insert(new Customer());

            customer.ID.ShouldBe(1);
        }

        [TestMethod]
        public async Task Insert_invokes_an_HTTP_POST_against_the_correct_uri_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    r.RequestUri.AbsoluteUri.ShouldBe("http://api/customers");
                    Assert.AreEqual(HttpMethod.Post, r.Method);
                });

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.InsertAsync(new Customer());
        }

        [TestMethod]
        public async Task Insert_invokes_expected_virtual_methods_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            await proxy.InsertAsync(new Customer());

            proxy.BuildConfiguredClientWasInvoked.ShouldBe(true);
            proxy.GetMediaTypeFormatterWasInvoked.ShouldBe(true);
            proxy.OnParseResponseWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task Insert_throws_NotImplementedException_when_server_status_code_is_NOT_IMPLEMENTED_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent("Method not implemented", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.InsertAsync(new Customer());
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public async Task Insert_throws_ServiceException_when_server_status_code_is_BAD_REQUEST_Async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("some known error occurred and was handled", Encoding.UTF8, "application/json")
                }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));

            await proxy.InsertAsync(new Customer());
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedMediaTypeException))]
        public async Task Insert_throws_UnsupportedMediaTypeException_when_bad_formatter_is_supplied_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object)) { Formatter = new XmlMediaTypeFormatter() };

            await proxy.InsertAsync(new Customer());
            proxy.OnFormatServerErrorWasInvoked.ShouldBe(true);
        }

        [TestMethod]
        public async Task Insert_returns_expected_item_async()
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"ID\": 1}", Encoding.UTF8, "application/json") }));

            var proxy = new HttpServiceProxyStub(new HttpClient(handler.Object));
            var customer = await proxy.InsertAsync(new Customer());

            customer.ID.ShouldBe(1);
        }

        #endregion

        #region Update
        #endregion

        #region Delete
        #endregion
    }
}
