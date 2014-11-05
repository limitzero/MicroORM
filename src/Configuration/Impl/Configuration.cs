using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Dialects;
using MicroORM.Dialects.Impl.SQLServer;
using MicroORM.Environment;
using MicroORM.Interception;
using MicroORM.Mapping;

namespace MicroORM.Configuration.Impl
{
	public class Configuration
	{
		private static Configuration instance;

		/// <summary>
		/// Gets the set of entity interceptors:
		/// </summary>
		public HashSet<IInterceptor> Interceptors { get; private set; }

		/// <summary>
		/// Gets the current persistance store dialect provider for data changes.
		/// </summary>
		public IDialectProvider Dialect { get; private set; }

		/// <summary>
		/// Gets the current connection provider to access the data store.
		/// </summary>
		public IDialectConnectionProvider ConnectionProvider { get; private set; }

		/// <summary>
		/// Gets or sets the configured set of database instances from the configuration:
		/// </summary>
		public ICollection<DatabaseSettings> ExternalDatabaseSettings { get; private set; }

		private Configuration()
		{
			this.Interceptors = new HashSet<IInterceptor>();
			this.Dialect = new SQLServerDialectProvider();
			this.ReadConfigurationSectionForAliases();
		}


		public static Configuration Instance
		{
			get { return instance ?? (instance = Factory.Create()); }
		}

		private class Factory
		{
			public static Configuration Create()
			{
				return new Configuration();
			}
		}

		/// <summary>
		/// This will register an interceptor with the ORM to broker calls for insert, updates and deletes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void RegisterInterceptor<T>() where T : class, IInterceptor, new()
		{
			var interceptor = new T();
			if (this.Interceptors.Contains(interceptor)) return;
			this.Interceptors.Add(interceptor);
		}

		/// <summary>
		/// This will set the dialect for creating sql statements for multiple database interaction.
		/// </summary>
		/// <typeparam name="TDialect"></typeparam>
		public void DialectProvider<TDialect>() where TDialect : class, IDialectProvider, new()
		{
			this.Dialect = new TDialect();
		}

		/// <summary>
		/// This will set the dialect for creating sql statemtents and the configuation information to create a connection to the data source.
		/// </summary>
		/// <typeparam name="TDialect"></typeparam>
		/// <param name="connectionProvider"></param>
		public void DialectProvider<TDialect>(IDialectConnectionProvider connectionProvider)
			where TDialect : class, IDialectProvider, new()
		{
			this.Dialect = new TDialect();
			this.ConnectionProvider = connectionProvider;
		}

	    public ISessionContainer BuildSessionContainer()
	    {
            return new SessionContainer();
	    }

	    public ISessionFactory BuildSessionFactory()
		{
			return new SessionFactory(new MetadataStore());
		}

		public ISessionFactory BuildSessionFactory(params Assembly[] assemblies)
		{
			var metadatastore = new MetadataStore();

			foreach (var assembly in assemblies)
			{
				var maps = (from match in assembly.GetExportedTypes()
				            where typeof (IEntityMap).IsAssignableFrom(match)
				                  && match.IsClass == true && match.IsAbstract == false
				            select Activator.CreateInstance(match) as IEntityMap)
					.ToList().Distinct();

				if (maps.Count() > 0)
				{
					foreach (var entityMap in maps)
					{
						TableInfo tableinfo = null;
						if (TryCreateTableInfo(entityMap, metadatastore, out tableinfo))
						{
							metadatastore.AddEntity(tableinfo.Entity);
							metadatastore.SetTableInfo(tableinfo);
						}
					}
				}
			}

			return new SessionFactory(metadatastore);
		}

		private static bool TryCreateTableInfo(IEntityMap entityMap,
		                                       IMetadataStore metadataStore,
		                                       out TableInfo tableInfo)
		{
			bool success = false;
			tableInfo = null;

			try
			{
				tableInfo = entityMap.Build(metadataStore);

				if (tableInfo != null)
				{
					success = true;
				}
			}
			catch
			{
			}

			return success;
		}

		private void ReadConfigurationSectionForAliases()
		{
			var section =
				ConfigurationManager.GetSection(DatabaseConfigurationSectionHandler.SectionName) as
				DatabaseConfigurationSectionHandler;

			if (section == null) return;

			var aliases = new List<DatabaseSettings>();

			for (int index = 0; index < section.AliasElements.Count; index++)
			{
				var element = section.AliasElements[index];
				aliases.Add(new DatabaseSettings(element.Name, element.Server, element.Database, element.UserName, element.Password));
			}

			this.ExternalDatabaseSettings = aliases;
		}
	}
}