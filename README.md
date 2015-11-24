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
            return "http://localhost:1234/api/customers";
        }
    }
}
```

In this example, we create an HTTP person repository.  The first thing to note is that we supplied ```Person``` and ```int``` as our generic types.  The Person class is a [DTO](https://github.com/peasy/Peasy.NET/wiki/Data-Transfer-Object-(DTO)) that must implement [```IDomainObject<T>```](https://github.com/peasy/Peasy.NET/blob/master/Peasy.Core/IDomainObject.cs).  The ```int``` specifies the key type that will be used for all of our arguments to the [IDataProxy](https://github.com/peasy/Peasy.NET/wiki/Data-Proxy) methods.

As part of our contractual obligation, we override ```RequestUri``` to provide the endpoint in which the data proxy will communicate with.  In this example, we hard coded the uri for brevity.  In practice, you will most likely return a value from a configuration file or similar.

By simply inheriting from HttpServiceProxyBase and overriding RequestUri, you have a full-blown HTTP data proxy that communicates with an HTTP endpoint, handling the [serialization/deserialization]() of your types, and throwing [peasy specific exceptions]() based on HTTP error response codes.

### Serialization/Deserialization

By default, HttpServiceProxyBase was designed to communicate with HTTP endpoints by sending and consuming JSON payloads.  Your strongly typed [DTOs]() will be serialized as JSON before being sent to HTTP endpoints.  In addition, the returned JSON will be deserialized into a strongly typed DTO and returned from your HTTP data proxy methods.

There might be cases when your HTTP data proxy needs to communicate with a service using a different media type, such as XML or a custom defined media type.  In this case, you can specify your own [MediaTypeFormatter]() by overriding the ```GetMediaTypeFormatter``` method.

Here is an example

```c#
public class PersonRepository : HttpServiceProxyBase<Person, int>
{
    protected override string RequestUri
    {
        get
        {
            return "http://localhost:1234/api/customers";
        }
    }
    
    protected override MediaTypeFormatter GetMediaTypeFormatter()
    {
        return new XmlMediaTypeFormatter();
    }
}
```

In this example, we simply override ```GetMediaTypeFormatter``` and return the [XmlMediaTypeFormatter]().  Doing this will serialize and deserialize our types to and from XML.

### Peasy specific exceptions

### Synchronous execution

### Parsing the response message
