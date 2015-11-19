![peasy](https://www.dropbox.com/s/2yajr2x9yevvzbm/peasy3.png?dl=0&raw=1)

### Peasy.DataProxy.InMemory

Peasy.DataProxy.InMemory provides the [InMemoryDataProxyBase](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs) class.  InMemoryDataProxyBase is an abstract class that implements [IDataProxy](https://github.com/ahanusa/Peasy.NET/wiki/Data-Proxy), and can be used to very quickly and easily provide an in-memory implementation of your data layer.

Creating an in-memory implementation of your data layer can help developing state-based unit tests as well as serving as a fake repository for rapid development of your [service classes](https://github.com/ahanusa/Peasy.NET/wiki/ServiceBase).  InMemoryDataProxy was designed to be simple to use and thread-safe.

###Where can I get it?

First, install NuGet. Then create a project for your in-memory class implementations to live.  Finally, install Peasy.DataProxy.InMemory from the package manager console:

``` PM> Install-Package Peasy.DataProxy.InMemory ```

You can also download and add the Peasy.DataProxy.InMemory project to your solution and set references where applicable

### Creating a concrete in-memory data proxy

To create an in-memory repository, you must inherit from [InMemoryDataProxyBase](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs).  There is one contractual obligation to fullfill.

1.) Override [GetNextID](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs#L45) - this method is invoked by [InMemoryDataProxyBase.```Insert```](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs#L65) and [InMemoryDataProxyBase.```InsertAsync```](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs#L113) and is used to obtain an ID to assign to a newly created object.  You can use any algorithm you want to calculate the next sequential ID that makes sense to you given your specified data type.

Here is a sample implementation

```c#
public class PersonRepository : InMemoryDataProxyBase<Person, int>
{
    protected override int GetNextID()
    {
        if (Data.Values.Any())
            return Data.Values.Max(c => c.ID) + 1;

        return 1;
    }
}
```

In this example, we create an in-memory person repository.  The first thing to note is that we supplied ```Person``` and ```int``` as our generic types.  The Person class is a [DTO](https://github.com/ahanusa/Peasy.NET/wiki/Data-Transfer-Object-(DTO)) that must implement [```IDomainObject<T>```](https://github.com/ahanusa/Peasy.NET/blob/master/Peasy.Core/IDomainObject.cs).  The ```int``` specifies the key type that will be used for all of our arguments to the [IDataProxy](https://github.com/ahanusa/Peasy.NET/wiki/Data-Proxy) methods.

As part of our contractual obligation, we override ```GetNextID``` to provide logic that is responsible for serving up a new ID when invoked by InMemoryDataProxyBase.```Insert``` or InMemoryDataProxyBase.```InsertAsync```.  It should be noted that in the event that Insert or InsertAsync receives a duplicate ID, an exception of type [System.ArgumentException](https://msdn.microsoft.com/en-us/library/system.argumentexception(v=vs.110).aspx) will be thrown.

By simply inheriting from InMemoryDataProxyBase and overriding GetNextID, you have a full-blown thread-safe in-memory data proxy that can be consumed by your unit tests and service classes.

### Providing seed data

Many times you'll want an instantiation of an in-memory data proxy to contain default data.  Providing default data is often times referred to as _seeding_ and can be accomplished by overriding [```SeedDataProxy```](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs#L40).

Here is an example

```c#
public class PersonRepository : InMemoryDataProxyBase<Person, int>
{
    protected override IEnumerable<Person> SeedDataProxy()
    {
        yield return new Person { ID = 1, Name = "Jimi Hendrix" };
        yield return new Person { ID = 2, Name = "Django Reinhardt" };
    }

    protected override int GetNextID()
    {
        if (Data.Values.Any())
            return Data.Values.Max(c => c.ID) + 1;

        return 1;
    }
}
```

In this example, we simply override ```SeedDataProxy``` and return our default data.
