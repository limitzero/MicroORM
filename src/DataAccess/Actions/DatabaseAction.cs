using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using MicroORM.Configuration;
using MicroORM.DataAccess.Hydrator;
using MicroORM.DataAccess.Internals;
using MicroORM.DataAccess.Internals.Impl;
using MicroORM.Dialects;

namespace MicroORM.DataAccess.Actions
{
    public abstract class DatabaseAction
    {
        protected IMetadataStore MetadataStore { get; private set; }
        protected IHydrator Hydrator { get; private set; }
        protected IDbConnection Connection { get; private set; }
        public IEnvironmentSettings Environment { get; private set; }

        protected DatabaseAction(IMetadataStore metadataStore,
            IHydrator hydrator,
            IDbConnection connection, 
            IEnvironmentSettings environment)
        {
            this.MetadataStore = metadataStore;
            this.Hydrator = hydrator;
            Connection = connection;
            Environment = environment;
        }

        protected IDbCommand CreateCommand()
        {
            var command = this.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            return command;
        }

        protected void DisplayCommand(IDbCommand command)
        {
#if DEBUG
            var builder = new StringBuilder();
            var parameters = new List<string>();

            builder.Append(command.CommandText).AppendLine(";");

            for ( int index = 0; index < command.Parameters.Count; index++ )
            {
                var parameter = command.Parameters[index] as DbParameter;
                parameters.Add(string.Format("[{0} = {1}]",
                    string.Concat("@", parameter.ParameterName), parameter.Value));
            }

            builder.Append(string.Concat("Parameters : ", string.Join(",", parameters)));

            this.Environment.Logger.Debug(builder.ToString());
#endif
        }
    }

    public abstract class DatabaseAction<TEntity>
    {
        private readonly IDbConnection connection;
        protected IMetadataStore MetadataStore { get; private set; }
        protected TEntity Entity { get; set; }
        protected IDialect Dialect { get; private set; }
        public IEnvironmentSettings Environment { get; private set; }

        protected DatabaseAction(IMetadataStore metadataStore,
            TEntity entity, IDbConnection connection,
            IDialect dialect,
            IEnvironmentSettings environment)
        {
            this.connection = connection;
            this.MetadataStore = metadataStore;
            Entity = entity;
            Dialect = dialect;
            Environment = environment;
        }

        protected IDbCommand CreateCommand()
        {
            var command = this.connection.CreateCommand();
            command.CommandType = CommandType.Text;
            return command;
        }

        protected void DisplayCommand(IDbCommand command)
        {
#if DEBUG
            var builder = new StringBuilder();
            var parameters = new List<string>();

            builder.Append(command.CommandText).AppendLine(";");

            for ( int index = 0; index < command.Parameters.Count; index++ )
            {
                var parameter = command.Parameters[index] as DbParameter;
                parameters.Add(string.Format("[{0} = {1}]",
                    string.Concat("@", parameter.ParameterName), parameter.Value));
            }

            builder.Append(string.Concat("Parameters : ", string.Join(",", parameters)));

            this.Environment.Logger.Debug(builder.ToString());
#endif
        }

        protected string GetParametersWithValueAssigmment(object entity)
        {
            var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();
            var builder = new StringBuilder();

            foreach ( var columnInfo in tableinfo.Columns )
            {
                if ( columnInfo is PrimaryKeyInfo )
                    continue;
                {
                    if ( columnInfo.Column.CanRead && !columnInfo.Column.PropertyType.IsGenericType )
                    {
                        var data = this.CoalesceValue(columnInfo.Column.GetValue(entity, null));
                        builder.Append(string.Format("@{0} = {1},", columnInfo.DataColumnName, data));
                    }
                }
            }

            return builder.ToString().TrimEnd(",".ToCharArray());
        }

        protected string GetColumnsWithParameterAssignment(object enity)
        {
            var tableinfo = this.MetadataStore.GetTableInfo<TEntity>();
            var builder = new StringBuilder();

            foreach ( var columnInfo in tableinfo.Columns )
            {
                if ( columnInfo is PrimaryKeyInfo )
                    continue;
                {
                    if ( columnInfo.Column.CanRead && !columnInfo.Column.PropertyType.IsGenericType )
                    {
                        builder.Append(string.Format("[{0}] = @{1},", columnInfo.DataColumnName, columnInfo.DataColumnName));
                    }
                }
            }

            return builder.ToString().TrimEnd(",".ToCharArray());
        }

        protected string CoalesceValue(object value)
        {
            string newValue = "is null";
            DateTime dateTime;
            var isDateParsed = DateTime.TryParse(value.ToString(), out dateTime);

            if ( typeof(string).IsAssignableFrom(value.GetType()) )
            {
                newValue = string.Format("'{0}'", value.ToString());
            }
            else if ( typeof(DateTime).IsAssignableFrom(value.GetType()) ||
                     isDateParsed == true )
            {
                if ( isDateParsed == false )
                {
                    dateTime = (DateTime)value;
                }

                newValue = string.Format("'{0}'", dateTime.ToShortDateString());
            }
            else if ( value == null )
            {
                newValue = "null";
            }
            else
            {
                newValue = value.ToString();
            }
            return newValue;
        }
    }
}