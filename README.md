![peasy](https://www.dropbox.com/s/2yajr2x9yevvzbm/peasy3.png?dl=0&raw=1)

### Peasy.DataProxy.Http

Peasy.DataProxy.Http provides the [HttpServiceProxyBase](https://github.com/peasy/Peasy.DataProxy.Http/blob/master/Peasy.DataProxy.Http/HttpServiceProxyBase.cs) class.  HttpServiceProxyBase is an abstract class that implements [IDataProxy](https://github.com/ahanusa/Peasy.NET/wiki/Data-Proxy), and can be used to very quickly and easily provide a data proxy that communicates with HTTP services via the GET, POST, PUT, and DELETE verbs.

###Where can I get it?

First, install NuGet. Then create a project for your HTTP class implementations to live.  Finally, install Peasy.DataProxy.Http from the package manager console:

``` PM> Install-Package Peasy.DataProxy.Http ```

You can also download and add the Peasy.DataProxy.Http project to your solution and set references where applicable

### Creating a concrete HTTP data proxy

To create an HTTP repository, you must inherit from [HttpServiceProxyBase](https://github.com/peasy/Peasy.DataProxy.Http/blob/master/Peasy.DataProxy.Http/HttpServiceProxyBase.cs).  There is one contractual obligation to fullfill.

1.) Override [RequestUri](https://github.com/peasy/Peasy.DataProxy.Http/blob/master/Peasy.DataProxy.Http/HttpServiceProxyBase.cs#L18) - this property represents the endpoint your proxy implementation will communicate with

Here is a sample implementation

```c#
public class PersonRepository : HttpServiceProxyBase<Person, int>
{
    protected override string RequestUri
    {
        get
        {
            return "http://localhost:1234/api/people";
        }
    }
}
```

In this example, we create an HTTP person repository.  The first thing to note is that we supplied ```Person``` and ```int``` as our generic types.  The Person class is a [DTO](https://github.com/peasy/Peasy.NET/wiki/Data-Transfer-Object-(DTO)) that must implement [```IDomainObject<T>```](https://github.com/peasy/Peasy.NET/blob/master/Peasy.Core/IDomainObject.cs).  The ```int``` specifies the key type that will be used for all of our arguments to the [IDataProxy](https://github.com/peasy/Peasy.NET/wiki/Data-Proxy) methods.

As part of our contractual obligation, we override ```RequestUri``` to provide the endpoint in which the data proxy will communicate with.  In this example, we hard coded the uri for brevity.  In practice, you will most likely return a value from a configuration file or similar.

By simply inheriting from HttpServiceProxyBase and overriding RequestUri, you have a full-blown HTTP data proxy that communicates with an HTTP endpoint, handling the [serialization/deserialization](https://github.com/peasy/Peasy.DataProxy.Http#serializationdeserialization) of your types, and throwing [peasy specific exceptions](https://github.com/peasy/Peasy.DataProxy.Http#peasy-specific-exceptions) based on HTTP error response codes.

### Serialization/Deserialization

By default, HttpServiceProxyBase was designed to communicate with HTTP endpoints by sending and consuming JSON payloads.  Your strongly typed [DTOs](https://github.com/peasy/Peasy.NET/wiki/Data-Transfer-Object-(DTO)) will be serialized as JSON before being sent to HTTP endpoints.  In addition, returned JSON from HTTP endpoints will be deserialized into strongly typed DTOs and returned from your HTTP data proxy methods.

There might be cases when your HTTP data proxy needs to communicate with a service using a different media type, such as XML or a custom defined media type.  In this case, you can specify your own [MediaTypeFormatter](https://msdn.microsoft.com/en-us/library/system.net.http.formatting.mediatypeformatter(v=vs.118).aspx) by overriding the ```GetMediaTypeFormatter``` method.

Here is an example

```c#
public class PersonRepository : HttpServiceProxyBase<Person, int>
{
    protected override string RequestUri
    {
        get
        {
            return "http://localhost:1234/api/people";
        }
    }
    
    protected override MediaTypeFormatter GetMediaTypeFormatter()
    {
        return new XmlMediaTypeFormatter();
    }
}
```

In this example, we simply override ```GetMediaTypeFormatter``` and return the [XmlMediaTypeFormatter](https://msdn.microsoft.com/en-us/library/system.net.http.formatting.xmlmediatypeformatter(v=vs.118).aspx).  Doing this will serialize and deserialize our types to and from XML.

### Peasy specific exceptions

Your HTTP data proxy implementations serve as a data layer abstraction to your peasy [service classes](https://github.com/peasy/Peasy.NET/wiki/ServiceBase).  As such, consumers of your data proxies should know how to handle certain exceptions that can happen when being consumed.  HttpServiceProxyBase handles a few specific HTTP error response codes and throws specific peasy exceptions so that you can handle accordingly.

**400 Bad Request** - This error is typically returned when a business or validation rule is broken during an HTTP request.  As a result, HttpServiceProxyBase throws a [Peasy.Core.ServiceException](https://github.com/peasy/Peasy.NET/blob/master/Peasy.Core/ServiceException.cs).  The [command](https://github.com/peasy/Peasy.NET/wiki/Command) class catches exceptions of this type, and adds errors to the [ExecutionResult](https://github.com/peasy/Peasy.NET/wiki/ExecutionResult).```Errors``` list on your behalf.

**404 Not Found** - This error is typically returned when an item cannot be found during an HTTP request.  As a result, HttpServiceProxyBase throws a [Peasy.DomainObjectNotFoundExeption](https://github.com/peasy/Peasy.NET/blob/master/Peasy/Exception/DomainObjectNotFoundException.cs).

**409 Conflict** - This error is typically returned when a concurrency issue occurs during an HTTP request.  As a result, HttpServiceProxyBase throws a [Peasy.ConcurrencyException](https://github.com/peasy/Peasy.NET/blob/master/Peasy/Exception/ConcurrencyException.cs).

**501 Not Implemented** - This error is typically returned when a requested resource provides no implementation for an HTTP request.  As a result, HttpServiceProxyBase throws a [System.NotImplementedException](https://msdn.microsoft.com/en-us/library/system.notimplementedexception(v=vs.110).aspx).



### Synchronous execution

### Parsing the response message
