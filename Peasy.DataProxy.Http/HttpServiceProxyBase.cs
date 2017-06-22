using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Peasy.Exception;
using Peasy;
using System;
using System.Net;

namespace Peasy.DataProxy.Http
{
    public abstract class HttpServiceProxyBase<T, TKey> : IServiceDataProxy<T, TKey> where T : IDomainObject<TKey>
    {
        private readonly ISynchronousInvocationStrategy _synchronousInvocationStrategy = new WaitForResultStrategy();

        protected abstract string RequestUri { get; }

        public HttpServiceProxyBase() { } 

        public HttpServiceProxyBase(ISynchronousInvocationStrategy syncInvocationStrategy)
        {
            _synchronousInvocationStrategy = syncInvocationStrategy;
        }
        
        /// <summary>
        /// Invokes an HTTP GET against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public virtual IEnumerable<T> GetAll()
        {
            return GET<IEnumerable<T>>(RequestUri);
        }

        /// <summary>
        /// Invokes an HTTP GET against the configured <see cref="RequestUri" and supplied id/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public virtual T GetByID(TKey id)
        {
            string requestUri = $"{RequestUri}/{id}";
            return GET<T>(requestUri);
        }

        /// <summary>
        /// Invokes an HTTP POST against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public virtual T Insert(T entity)
        {
            return POST<T, T>(entity, RequestUri);
        }

        /// <summary>
        /// Invokes an HTTP PUT against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ConcurrencyException">Thrown when server returns 409 - Conflict</exception> 
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public virtual T Update(T entity)
        {
            string requestUri = $"{RequestUri}/{entity.ID}";
            return PUT<T, T>(entity, requestUri);
        }

        /// <summary>
        /// Invokes an HTTP DELETE against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public virtual void Delete(TKey id)
        {
            string requestUri = $"{RequestUri}/{id}";
            DELETE(requestUri);
        }

        /// <summary>
        /// Invokes an HTTP GET against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public async virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return await GETAsync<IEnumerable<T>>(RequestUri);
        }

        /// <summary>
        /// Invokes an HTTP GET against the configured <see cref="RequestUri" and supplied id/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public async virtual Task<T> GetByIDAsync(TKey id)
        {
            string requestUri = $"{RequestUri}/{id}";
            return await GETAsync<T>(requestUri);
        }

        /// <summary>
        /// Invokes an HTTP POST against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public async virtual Task<T> InsertAsync(T entity)
        {
            return await POSTAsync<T, T>(entity, RequestUri);
        }

        /// <summary>
        /// Invokes an HTTP PUT against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ConcurrencyException">Thrown when server returns 409 - Conflict</exception> 
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public async virtual Task<T> UpdateAsync(T entity)
        {
            string requestUri = $"{RequestUri}/{entity.ID}";
            return await PUTAsync<T, T>(entity, requestUri);
        }

        /// <summary>
        /// Invokes an HTTP DELETE against the configured <see cref="RequestUri"/> 
        /// </summary>
        /// <exception cref="DomainObjectNotFoundException">Thrown when server returns 404 - Not Found</exception>
        /// <exception cref="ServiceException">Thrown when server returns 400 - Bad Request</exception>
        /// <exception cref="NotImplementedException">Thrown when server returns 501 - Not Implemented</exception>
        public async virtual Task DeleteAsync(TKey id)
        {
            string requestUri = $"{RequestUri}/{id}";
            await DELETEAsync(requestUri);
        }

        /// <summary>
        /// Invokes a GET against the supplied Uri
        /// </summary>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual TOut GET<TOut>(string requestUri)
        {
            return _synchronousInvocationStrategy.Invoke(() => GETAsync<TOut>(requestUri));
        }

        /// <summary>
        /// Invokes a GET against the supplied Uri
        /// </summary>
        /// <typeparam name="TIn">The type serialized and sent to the service</typeparam>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="args"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual async Task<TOut> GETAsync<TOut>(string requestUri)
        {
            using (var client = BuildConfiguredClient())
            {
                var response = await client.GetAsync(requestUri).ConfigureAwait(false);
                var entity = await ParseResponseAsync<TOut>(response);
                return entity;
            }
        }

        /// <summary>
        /// Invokes a POST against the supplied Uri
        /// </summary>
        /// <typeparam name="TIn">The type serialized and sent to the service</typeparam>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="args"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual TOut POST<TIn, TOut>(TIn args, string requestUri)
        {
            return _synchronousInvocationStrategy.Invoke(() => POSTAsync<TIn, TOut>(args, requestUri));
        }

        /// <summary>
        /// Invokes a POST against the supplied Uri
        /// </summary>
        /// <typeparam name="TIn">The type serialized and sent to the service</typeparam>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="args"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual async Task<TOut> POSTAsync<TIn, TOut>(TIn args, string requestUri)
        {
            using (var client = BuildConfiguredClient())
            {
                var response = await client.PostAsync<TIn>(requestUri, args, GetMediaTypeFormatter()).ConfigureAwait(false);
                var returnValue = await ParseResponseAsync<TOut>(response);
                return returnValue;
            }
        }

        /// <summary>
        /// Invokes a PUT against the supplied Uri
        /// </summary>
        /// <typeparam name="TIn">The type serialized and sent to the service</typeparam>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="args"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual TOut PUT<TIn, TOut>(TIn args, string requestUri)
        {
            return _synchronousInvocationStrategy.Invoke(() => PUTAsync<TIn, TOut>(args, requestUri));
        }

        /// <summary>
        /// Invokes a PUT against the supplied Uri
        /// </summary>
        /// <typeparam name="TIn">The type serialized and sent to the service</typeparam>
        /// <typeparam name="TOut">The type that will be deserialized from the service response and returned</typeparam>
        /// <param name="args"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual async Task<TOut> PUTAsync<TIn, TOut>(TIn args, string requestUri)
        {
            using (var client = BuildConfiguredClient())
            {
                var response = await client.PutAsync<TIn>(requestUri, args, GetMediaTypeFormatter()).ConfigureAwait(false);
                var returnValue = await ParseResponseAsync<TOut>(response);
                return returnValue;
            }
        }

        /// <summary>
        /// Invokes a DELETE synchronously against the supplied Uri
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual void DELETE(string requestUri)
        {
            _synchronousInvocationStrategy.Invoke(() => DELETEAsync(requestUri)); 
        }

        /// <summary>
        /// Invokes a DELETE asynchronously against the supplied Uri
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        protected virtual async Task DELETEAsync(string requestUri)
        {
            using (var client = BuildConfiguredClient())
            {
                var response = await client.DeleteAsync(requestUri).ConfigureAwait(false);
                EnsureSuccessStatusCode(response);
            }
        }

        protected virtual HttpClient BuildConfiguredClient()
        {
            return new HttpClient();
        }

        protected virtual MediaTypeFormatter GetMediaTypeFormatter()
        {
            return new JsonMediaTypeFormatter();
        }

        protected virtual async Task<TOut> ParseResponseAsync<TOut>(HttpResponseMessage result)
        {
            EnsureSuccessStatusCode(result);
            var entity = await result.Content.ReadAsAsync<TOut>(new[] { GetMediaTypeFormatter() });
            OnParseResponse(result, entity);
            return entity;
        }

        protected virtual void OnParseResponse<TOut>(HttpResponseMessage response, TOut entity)
        {
        }

        /// <summary>
        /// Throws an exception if the System.Net.Http.HttpResponseMessage.IsSuccessStatusCode property for the HTTP response is false.
        /// </summary>
        /// <returns>
        /// Returns System.Net.Http.HttpResponseMessage.The HTTP response message if the call is successful.
        /// </returns>
        protected virtual HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            string message = string.Empty;
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    message = OnFormatServerError(response.Content.ReadAsStringAsync().Result.ToString());
                    throw new ServiceException(message);

                case HttpStatusCode.Conflict:
                    message = OnFormatServerError(response.Content.ReadAsStringAsync().Result.ToString());
                    throw new ConcurrencyException(message);

                case HttpStatusCode.NotFound:
                    message = OnFormatServerError(response.Content.ReadAsStringAsync().Result.ToString());
                    throw new DomainObjectNotFoundException(message);

                case HttpStatusCode.NotImplemented:
                    message = OnFormatServerError(response.Content.ReadAsStringAsync().Result.ToString());
                    throw new NotImplementedException(message);
            }
            return response.EnsureSuccessStatusCode();
        }

        protected virtual string OnFormatServerError(string message)
        {
            return message;
        }

        public virtual bool IsLatencyProne
        {
            get { return true; }
        }

        public virtual bool SupportsTransactions
        {
            get { return false; }
        }
    }
}