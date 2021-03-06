﻿#Welcome to MicroORM

MicroORM is a **_versatile_** micro-orm (i.e Dapper.Net, PetaPoco, Massive, etc) for .NET 4+. Currently the project is not licensed but that wil be remedied shortly :)

If you are wondering "Why is this guy creating yet another micro-orm for data persistence" it was created solely as a learning project about how NHibernate works 
and what would it take to get a bare-bones ORM off the ground to accomodate simple persistence.  This sits in the middle-space of micro-orm and full-blown orms like NH and EF allowing you to expand your data persistence needs up or down as your application grows.

## Why should you use it
The unqiue part of  why you would want to use this library is an entry-level option into NHibernate without all of the nuances that NHibernate brings for simple persistence. If at any time you need the full power of NHibernate, switching to its native 
API should be relatively painless but some effort will be required. 

The main reason that this library was designed is that most orms treat applications as if only one database will be involved for all data persistence and retreival.  MicroORM understands that the majority of applications will retrieve and store data to differing databases
and has built-in configuration extensions and markers to allow for this. 

MicroORM currently supports
* SqlServer 2005+
* SqlServerCE 4 
* Sqlite 

##User Friendly
Intuitive usage and native LINQ provider for accessing information via the model entities to your data store similar to EF and NH. 

## Usage
 
###Entity Persistence

The session (ISession) is the core part of entity persistance and retrieval, this works as the central 
point of coordinating all data changes and implements IDisposable for appropriate scoping. Please be aware 
that the library uses "lazy-loading" in the scope of the session to pull referenced entities and entity 
collections on the entity being manipulated. 

```csharp

// saving an entity:
using(var session = SessionFactory.OpenSession())
using(var txn = session.BeginTransaction())
{
    var customer = new Customer();

    customer.FirstName = "joe";
    customer.LastName = "schmoe";

    // let the session figure out whether to insert or update:
    session.SaveOrUpdate(customer); 
    txn.Commit();
	
    var fromDB = session.Get<Customer>(customer.Id);
    Assert.Equal(customer.Id, fromDB.Id);
}
```

```csharp

// fetching an entity and updating
using(var session = SessionFactory.OpenSession())
using(var txn = session.BeginTransaction())
{
    // fetch the entity by the value in the primary key column:
    var customer = session.Get<Customer>(1):

    customer.FirstName = "joe";
    customer.LastName = "schmoe";

    // let the session figure out whether to insert or update:
    session.SaveOrUpdate(customer); 
	
    txn.Commit();
}
```

```csharp
// fetching an entity and updating underlying collections:

using (var session = SessionFactory.OpenSession())
using (var txn = session.BeginTransaction())
{
    // fetch the entity by the value in the primary key column:
    var customer = session.Get<Customer>(1):

    var order = customer.CreateOrder(); 
    order.OrderNumber = "12345";
    order.OrderDate = System.DateTime.UtcNow();

    // this will update the customer and save all children 
    // in the collection:
    session.SaveOrUpdate(customer); 

    txn.Commit();
}
```

```csharp
// entities

[Table()]
public class Customer
{
    [PrimaryKey("customerId")]
    public virtual int Id {get; set;}

    [Column("firstName")]
    public virtual string FirstName {get; set;}

    [Column("lastName")]
    public virtual string LastName {get; set;}

    private IList<Order> orders = new List<Order>();
    public virtual IEnumerable<Order> Orders 
    {
        get {return orders;}
    }

    public virtual Order CreateOrder()
    {
        var order = new Order(this);

        // control uniqueness on parent for children:
        if(this.orders.Contains(order) == false)
        {
            this.orders.Add(order);
        }

        return order;
    }
}

[Table()]
public class Order
{
    // this is a reference to the customer that created the order:
    public virtual Customer Customer {get; set;}

    [PrimaryKey("orderId")]
    public virtual int Id {get; set;}

    [Column("orderNumber")]
    public virtual string OrderNumber {get; set;}

    [Column("orderDate")]
    public virtual DateTime? OrderDate {get; set;}

    // needed for entity hydration...
    private Order() {}

    // an order can not exist without a customer...
    public Order(Customer customer)
    {
        this.Customer = customer;
    }
}
```

###Entity Interception
We can "intercept" calls to entities on insert, update and delete operations as a way to further extend how 
we can handle certain cross cutting concerns and apply our own logic. 

For example, Company A wants to use the micro ORM to delete entities but does not want them removed from the persistance 
storage as a matter of internal auditing purposes (i.e. "soft delete"). Instead they would like these records to be unavailable for selection 
by future inquiries. 

To do this we first create a class and derive its implementation from IDeleteInterceptor

First, let's create an interface that models the overall behavior that we want on the entity:

```csharp
public interface IAuditable
{
    // for updates:
    int ModifiedBy {get; set;}
    Datetime? ModifiedDate {get; set:}

    // for inserts:
    int CreatedBy {get; set;}
    Datetime? CreationDate {get; set:}

    // for deletes:
    int DeletedBy {get; set;}
    Datetime? DeletionDate {get; set;}
}
```

Next, let's create a plain class that holds this information:

```csharp
public class Auditable : IAuditable
{
    [Column("createdById")]
    public int CreatedBy {get; set;}

    [Column("creationDate")]
    public Datetime? CreationDate {get; set:}

    [Column("modifiedById")
    public int ModifiedBy {get; set;}

    [Column("modifiedDate")
    public Datetime? ModifiedDate {get; set:}

    [Column("deletedById")]
    public int DeletedBy {get; set;}

    [Column("deletionDate")]
    public Datetime? DeletionDate {get; set:}
}
```

For each entity that is to be peristed and audited, let's inherit from the auditable parent class

```csharp
public class Customer : Auditable
{
    // other properties plus auditable....
}
```

Now, let's define the interceptor to handle deleting of entities:

```csharp
public class MarkAsUnavailableOnDeleteInterceptor : IDeleteInterceptor
{
    public bool OnPreDelete(IDataInvocation invocation)
    {
        if(typeof(IAuditable).IsAssignableFrom(invocation.Entity.GetType())
        {
            var auditable = (IAuditable)invocation.Entity;

            auditable.DeletedById = {some identifier of the user}
            auditable.DeletionDate = System.DateTime.UtcNow();

            // let's save the updated entity back to the persistance store instead:
            invocation.Session.Save(invocation.Entity);
        }

        // return "false" to tell the session not to invoke the delete action...
        return false;
    }

    public void OnPostDelete(IDataInvocation invocation)
    {
        // not invoked if OnPreDelete returns "false"...
    }

}
```

Finally, in using the ORM, we set the Configuration object with the interceptor to 
use on delete operations:

Ex: 

```csharp
var configuration = new Configuration();

// you must register the interceptors before building the factory...
configuration.Environment.RegisterInterceptor<MarkAsUnavailableOnDeleteInterceptor>(); 

var factory = configuration.BuildFactory(this.GetType().Assembly); 

using(var session = factory.OpenSession("your connection"))
using (var txn = session.BeginTransaction())
{
    var customer = session.Get<Customer>(1);
    session.Delete(customer);
    txn.Commit();	
}
```

Now on all deletes, the entity will be updated but not removed...


### Entity Mapping (POCO style on domain model)
In order to reduce the noise on the domain model and remove the declarative style of 
identifying columns, primary keys etc. on the entity, we can add external mapping 
classes. 

Ex: Creating a mapping for a simple class

```csharp
public class Invoice 
{
    public virtual int Id {get; set;}
    public virtual string InvoiceNumber {get; set;}
}

public class InvoiceMap : EntityMap<Invoice>
{
    public InvoiceMap()
    {
        TableName = "Invoices"; // name of the data table
        HasPrimaryKey(pk => pk.Id, "invoiceId"); // define the primary key column on the entity
        HasColumn( c=>c.InvoiceNumber, "invoiceNumber"); // define the data column to map to the class property
    }
}
```

Ex: Creating an entity with associations 

 - An insurance policy is assigned an agent (the policy can not be created without an existing agent)
 - An agent has zero or more policies to manage for existing customers

 ```csharp
public class Agent
{
    public virtual int Id {get; set;}   
    public virtual string AgentNumber {get; set;}

    private IList<Policy> policies = new List<Policy>();
    public virtual IEnumerable<Policy> Policies
    {
        get {return this.polices;}
        private set {this.policies = value;}
    }

    // an agent can create a policy (single responsibility)...
    public virtual Policy CreatePolicy()
    {
        var policy = new Policy(this); 

        // manage the collection internally:
        if(this.policies.Contains(policy) == false)
        {
            this.policies.Add(policy);
        }
        
        return policy;
    }
}

public class Policy
{
    public virtual Agent Agent {get; set;}
    public virtual int Id {get; set;}
    public virtual string PolicyNumber {get; set;}

    // need at least one parameter-less constructor for object construction:
    private Policy() {}

    // here is where the business rule is enforced that a policy must have an agent..
    public Policy(Agent agent)
    {
        this.Agent = agent;
    }
}

public class AgentEntityMap : EntityMap<Agent>
{
    public AgentEntityMap()
    {
        TableName = "Agents";
        HasPrimaryKey(pk =>pk.Id, "agentId");
        HasColumn (c=>c.AgentNumber, "agentNumber");
        HasCollection (c=>c.Policies); // agent has zero or more policies.
    }
}

public class PolicyEntityMap : EntityMap<Policy>
{
    public PolicyEntityMap()
    {
        TableName = "Policies";
        HasPrimaryKey(pk => pk.Id, "policyId");
        HasColumn(c=>c.PolicyNumber, "policyNumber");
        HasReference(r=>r.Agent); // a policy is assigned to an agent
    }
}
```

Unit testing our model, we have something like this...

```csharp
public class DomainModelBehaviorTests
{
    [Fact]
    public void can_assign_policy_to_agent()
    {
        var agent = new Agent();
        var policy = agent.CreatePolicy();

        // make sure of the parent to child relationship:
        Assert.True(ReferenceEquals(agent, policy.Agent));

        // make sure policy was added to the collection:
        Assert.Equal(1, agent.Policies.Count());
    }
}

public class DomainModelPersistanceTests : IDisposable
{
    private ISessionFactory factory;
    private const string Connection = "Data Source=.\SQLExpress;Initial Catalog=testdb;";

    public DomainModelPersistanceTests()
    {
        var configuration = new Configuration()

        this.factory = configuration.
            .BuildSessionFactory(Connection, this.GetType().Assembly);
    }

    public void Dispose()
    {
        this.factory = null;
    }

    [Fact]
    public void can_assign_policy_to_agent()
    {
        using (var session = factory.OpenSession())
        {
            var agent = new Agent();
            var policy = agent.CreatePolicy();

            session.Save(agent); 

            var fromDB = session.Get<Agent>(1);

            Assert.True(agent.Id, fromDB.Id);
            Assert.Equal(1, agent.Policies.Count()); // lazy load invoked on "get"
        }
    }
}
```

##Using MicroORM with an IOC/DI framework

###Factory Extensions of DI/IOC framework for generation of the session
If the DI framework supports factory creation of objects, then we can use it to 
generate an concrete instance of the session when the supporting object is resolved
from the container:

Example: (loosely based on Castle Windsor as IoC):

```csharp
public class Bootstrapper
{
    private static ISessionFactory factory;

    public void Boot(IContainer container)
    {
		var configuration = new Configuration();

		var factory = configuration
			.BuildSessionFactory(this.GetType().Assembly);

		container.Register(Component.For<ISessionFactory>()
			.Instance(factory));
			
		// now we need to define how to get the session from the container (simplistic declaration):
		container.Register(Component.For<ISession>()
			.UsingFactory<ISessionFactory, ISession>( factory => factory.OpenSession("your connection string"))
			.LifeStyleTransient());
    }
}

public class MyFacade
{
    private ISession session;

    public MyFacade(ISession session)
    {
        this.session = session;
    }

    public void DoSomeWork()
    {
        session.Save(...); 
        var entity = session.CreateQuery<..>();
    }
}
```

The line below will inject the session into the component upon retrieval from the container:

```csharp
var facade = IContainer.Resolve<MyFacade>(); 
facade.DoSomeWork(); 
IContainer.Release(facade); // only if container can support it... 
```


##Using MicroORM with multiple databases:
OK, here is the "pratical" part of using the MicroORM...most applications will need to interface
with different databases as a normal part of completing business processing. In order to do this, 
all you have to do is add your connection strings to the settings file and create "named" sessions
via the connection string and implementation. 

For example, if we have an application configuration file with the following settings:

<configuration>
	<appSettings>
		<add key="Contoso" value="Data Source=.\SQLExpress;Initial Catalog=Contoso;" />
		<add key="AdventureWorks" value="Data Source=.\SQLExpress;Initial Catalog=AdventureWorks;" />
	</appSettings>
</configuration>

and in code, you can do the following:

```csharp
public class Bootstrapper
{
    public void Boot(IContainer container)
    {
		IConfiguration configuration = new Configuration();

		var contoso = ConfigurationManager.AppSettings.Get("Contoso"); 
		var adventureWorks = ConfigurationManager.AppSettings.Get("AdventureWorks"); 

		configuration
			.RegisterNamedSession<IContosoNamedSession, ContosoNamedSession>(contoso, "assembly to entities for contoso")
			.RegisterNamedSession<IAdventureWorksNamedSession, AdventureWorksNamedSession>(adventureWorks,
             "assembly to entities for adventure works");  
		
		container.Register(Component.For<IConfiguration>()
			.Instance(configuration));
			
		container.Register(Component.For<IContosoNamedSession>()
			.UsingFactory<IConfiguration, IContosoNamedSession>( cfg=> cfg.NamedSessionContainer.Resolve<IContosoNamedSession>()))
			.LifeStyleTransient());

		container.Register(Component.For<IAdventureWorksNamedSession>()
			.UsingFactory<IConfiguration, IAdventureWorksNamedSession>( cfg=> cfg.NamedSessionContainer.Resolve<IAdventureWorksNamedSession>()))
			.LifeStyleTransient());

		container.Register(Component.For<MyContosoComponent>()
			.ImplementedBy<MyContosoComponent>()
			.LifeStyleTransient());

		container.Register(Component.For<MyAdventureWorksComponent>()
			.ImplementedBy<MyAdventureWorksComponent>()
			.LifeStyleTransient());
    }
}
```

and somewhere in code you will have....

```csharp
// this is the named session instance marker to the Constoso database
public interface IContosoNamedSession : INamedSession
{}

// this is the named session instance marker to the AdventureWorks database
public interface IAdventureWorksNamedSession : INamedSession
{}

public sealed class ContosoNamedSession : IContosoNamedSession
{
    public ISession Session {get; set;}
}

public sealed class AdventureWorksNamedSession : INamedSession
{
    public ISession Session {get; set;}
}

public class MyConstosoComponent : IDisposable
{
	private  IContosoNamedSession namedSession;

	public MyContosoComponent (IContosoNamedSession namedSession)
	{
		this.namedSession = namedSession;
	}

	public void Disposable()
	{
		if(namedSession != null && namedSession.Session != null)
		{
			namedSession.Session.Dispose();
		}
	}
	
	public void DoWork()
	{
		using(var txn = _namedSession.Session.BeginTransaction())
		{
			try
			{	
				// do some work....
				_namedSession.Session.Save(....);
				txn.Commit();
			}
			catch
			{
				txn.Rollback();
				throw;
			}
		}
	}
}

public class MyAdventureWorksComponent : IDisposable
{
	private IAdventureWorksNamedSession namedSession;

	public MyAdventureWorksComponent(IAdventureWorksNamedSession namedSession)
	{
		this.namedSession = namedSession;
	}

	public void Disposable()
	{
		if(namedSession != null && namedSession.Session != null)
		{
			namedSession.Session.Dispose();
		}
	}
	
	public void DoWork()
	{
		using(var txn = _namedSession.Session.BeginTransaction())
		{
			try
			{	
				// do some work....
				_namedSession.Session.Save(....);
				txn.Commit();
			}
			catch
			{
				txn.Rollback();
			}
		}
	}
}
```

##Using the DAO/Repository pattern with the session
Some people may want to use a DAO/Repository for abstraction over the session or named session instance for data access encapsulation in their code bases. We can accomplish the "spirit" of this as follows:

```csharp
public class ConstosoRepository : IContosoRepository, IDisposable
{
	private  IContosoNamedSession namedSession;

	public ConstosoRepository (IContosoNamedSession namedSession)
	{
		this.namedSession = namedSession;
	}	
	
	public void Disposable()
	{
		if(namedSession != null && namedSession.Session != null)
		{
			namedSession.Session.Dispose();
		}
	}
	
	// your use cases for data retreival .....
}
```

and 

```csharp
public class AdventureWorksRepository : IAdventureWorksRepository, IDisposable
{
	private  IAdventureWorksNamedSession namedSession;

	public AdventureWorksRepository (IAdventureWorksNamedSession  namedSession)
	{
		this.namedSession = namedSession;
	}	
	
	public void Disposable()
	{
		if(namedSession != null && namedSession.Session != null)
		{
			namedSession.Session.Dispose();
		}
	}
	
	// your use cases for data retreival .....
}
```

with registrations like previous use case for components....

##Querying using custom provider 

Querying over entities is relatively straight forward with LINQ support behind the scenes:

```csharp

using ( var session = _factory.OpenSession("your connection string") )
using ( var txn = session.BeginTransaction())
{
	var department = new Department
	{
		Description = "Test",
		Name = "Test",
		Number = "101"
	};

	var instructor = department.CreateInstructor();
	instructor.Name.Change("Test", "Test");

	session.Save(department);
	txn.Commit();

	var result = session.QueryOver<Department>()
		.Join<Instructor>((d) => d.Id, (i) => i.Department.Id)
		.Where<Instructor>((i) => i.Name.FirstName == "Test")
		.And<Department>((i) => i.Name == "Test")
		.Select().FirstOrDefault();
}
```

TODO: Multi-map queries for projections into custom models for viewing


Eat, Drink and Enjoy...


