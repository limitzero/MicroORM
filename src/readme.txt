MicroORM : A small framework for entity persistance

Credits: 
===========
- LinqExtender (http://mehfuzh.github.com/LinqExtender/) - supporting LINQ querying ( no join support )
- Davey Brion
- LINQ-to-T-SQL Provider Framework - http://linqprovider.codeplex.com/

Entity Persistance
-------------------
The session (ISession) is the core part of entity persistance and retrieval, this works as the central 
point of coordinating all data changes and implements IDisposable for appropriate scoping. Please be aware 
that the library uses "lazy-loading" in the scope of the session to pull referenced entities and entity 
collections on the entity being manipulated. 

Ex: Saving a new instance of an entity

using(var scope = new TransactionScope())
using(var session = SessionFactory.OpenSession())
{
    var customer = new Customer();

    customer.FirstName = "joe";
    customer.LastName = "schmoe";

    // let the session figure out whether to insert or update:
    session.SaveOrUpdate(customer); 

    scope.Complete();
}


Ex: Fetching an entity and updating

using(var scope = new TransactionScope())
using(var session = SessionFactory.OpenSession())
{
    // fetch the entity by the value in the primary key column:
    var customer = session.Get<Customer>(1):

    customer.FirstName = "joe";
    customer.LastName = "schmoe";

    // let the session figure out whether to insert or update:
    session.SaveOrUpdate(customer); 

    scope.Complete();
}

Ex: Fetching an entity and updating underlying collections:

using(var scope = new TransactionScope())
using(var session = SessionFactory.OpenSession())
{
    // fetch the entity by the value in the primary key column:
    var customer = session.Get<Customer>(1):

    var order = customer.CreateOrder(); 
    order.OrderNumber = "12345";
    order.OrderDate = System.DateTime.UtcNow();

    // this will update the customer and save all children 
    // in the collection:
    session.SaveOrUpdate(customer); 

    scope.Complete();
}

Ex:  Entities

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
    public Customer Customer {get; set;}

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


Entity Interception:
---------------------
We can "intercept" calls to entities on insert, update and delete operations as a way to further extend how 
we can handle certain cross cutting concerns and apply our own logic. 

For example, Company A wants to use the micro ORM to delete entities but does not want them removed from the persistance 
storage as a matter of internal auditing purposes (i.e. "soft delete"). Instead they would like these records to be unavailable for selection 
by future inquiries. 

To do this we first create a class and derive its implementation from IDeleteInterceptor

First, let's create an interface that models the overall behavior that we want on the entity:

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

Next, let's create a plain class that holds this information:

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

For each entity that is to be peristed and audited, let's inherit from the auditable parent class

public class Customer : Auditable
{
    // other properties plus auditable....
}


Now, let's define the interceptor to handle deleting of entities:

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

Finally, in using the ORM, we set the Configuration object with the interceptor to 
use on delete operations:

MicroORM.Configuration.RegisterInterceptor<MarkAsUnavailableOnDeleteInterceptor>();

Now on all deletes, the entity will be updated but not removed...


Entity Mapping (POCO style on domain model)
--------------------------------------------------
In order to reduce the noise on the domain model and remove the declarative style of 
identifying columns, primary keys etc. on the entity, we can add external mapping 
classes. 

Ex: Creating a mapping for a simple class

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

Ex: Creating an entity with associations 

 - An insurance policy is assigned an agent (the policy can not be created without an existing agent)
 - An agent has zero or more policies to manage for existing customers

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

Unit testing our model...

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
        Assert.Equal(1, ToList(agent.Policies).Count);
    }

    private IList<T> ToList(IEnumerable<T> list) where T : class
    {
        var list = new List<T>(list);
		return list;
    }
}

public class DomainModelPersistanceTests : IDisposable
{
    private ISessionFactory factory;

    public DomainModelPersistanceTests()
    {
        // initially only supports SQL Server :(
		MicroORM.Configuration.Instance.DialectProvider<SqlServerDialectProvider>(
			new SqlServerDialectConnectionProvider("{data server name}", "{database name}")); //this uses SSPI

        // this option will pick up all mapping files and classes from the specified assemblies describing 
        // the domain model to be persisted to storage:
		this.factory = MicroORM.Configuration.Instance.BuildSessionFactory(this.GetType().Assembly);
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
            Assert.Equal(1, ToList(agent.Policies).Count); // lazy load invoked on "get"
        }
    }

    private IList<T> ToList(IEnumerable<T> list) where T : class
    {
        var list = new List<T>(list);
		return list;
    }
}

Using MicroORM with an IOC/DI framework
----------------------------------------

The most challenging part of this configuration is how to generate an 
instance of the session to have data persistance for the set of domain entities. 

(1) Factory Extensions of DI/IOC framework
==========================================
If the DI framework supports factory creation of objects, then we can use it to 
generate an concrete instance of the session when the supporting object is resolved
from the container:

Ex (based on Castle Windsor as IoC):

public class Bootstrapper
{
    private static ISessionFactory factory;

    public void Boot(IContainer container)
    {
        // set your dialect and connection information:
		MicroORM.Configuration.Instance.DialectProvider<SqlServerDialectProvider>(
			new SqlServerDialectConnectionProvider("{data server name}", "{database name}"));

        // set your mapping assemblies for the factory:
		factory = MicroORM.Configuration.Instance.BuildSessionFactory(this.GetType().Assembly);

        // now we need to define how to get the session from the container (simplistic declaration):
        container.UseFactoryFor<ISession>(()=> factory.OpenSession());
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

The line below will inject the session into the component upon retrieval from the container:

var facade = IContainer.Resolve<MyFacade>(); 
facade.DoSomeWork(); 
IContainer.Release(facade); // only if container can support it... 


Using MicroORM with multiple databases:
-------------------------------------------------------
OK, here is the "pratical" part of using the MicroORM, most applications will need to interface
with different databases as a normal part of completing business processing. In order to do this, 
all you have to do is add the following to the configuration file for accessing multiple databases. 

<configSections>
	<section name="micro.orm" type="MicroORM.Environment.DatabaseConfigurationSectionHandler, MicroORM" />
</configSections>

<micro.orm>
		<aliases>
			<alias name="some name" server="some server" database="some database" username="{user name to access database}" password="{password to access database}"/>
			<alias name="some other name" server="some server" database="some database" username="{user name to access database}" password="{password to access database}"/>
		</aliases>
</micro.orm>

and in code, you can do the following:

MicroORM.Configuration.Instance.DialectProvider<SqlServerDialect>(); // no need to specify the connection provider
var factory = MicroORM.Configuration.Instance.BuildSessionFactory("{assembly containing your entities}");

using (var session = factory.OpenSessionViaAlias("{some name}"))  // this will look up the information to connect to DB
using (var txt = session.BeginTransaction()) 
// remember this transaction will be scoped with the database mentioned in the alias...
{
    //do some work....
}

for distributed transactions, you would do the following (do not use local transaction from session!!!):

using (var session1 = factory.OpenSessionViaAlias("{some name}"))  // this will look up the information to connect to DB
using (var session2 = factory.OpenSessionViaAlias("{some other name}"))  // this will look up the information to connect to DB
using (var txn = new TransactionScope()) // here is the synchronization part....
{
	try
	{
		// do some work:
		txn.Commit();
	}
	catch(Exception someException)
	{
		// transaction rolledback here in both DB's...
		t
		throw;
	}
}

Using the DAO pattern with the session
==============================================
Some people may want to use a DAO for abstraction over the session

		public class FirstDBDAO
		{
			private IFirstDBDAOSession session;
			public class FirstDBDAO(IFirstDBDAOSession session)
			{
				this.session = session;
			}

			// some method to work with session to persist and retrieve
		}

and the definition for the session to the indicated database

		public interface IFirstDBSession : ISession
		{}

and in the IoC configuration, something like this will be created

		container.Register<FirstDBDAO>();

		var firstDBFactory = MicroORM.Configuration.Instance.BuildSessionFactory("{assembly containing your entities}");
		container.UseFactoryFor<IFirstDBSession>(()=> firstDBFactory.OpenSessionViaAlias("first db"));

and in code:

		using(var txn = new TransactionScope())
		{
				var dao = container.Resolve<FirstDBDAO>();
		}




Eat, Drink and Enjoy...


