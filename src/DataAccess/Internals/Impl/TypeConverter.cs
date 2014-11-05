using System;
using System.Collections.Generic;
using System.Data;

namespace MicroORM.DataAccess.Internals.Impl
{
	public static class TypeConverter
	{
		private static readonly Dictionary<Type, DbType> dotNetTypeToDbType
			= new Dictionary<Type, DbType>
			  	{
			  		{typeof (string), DbType.String},
			  		{typeof (DateTime), DbType.DateTime},
			  		{typeof (DateTime?), DbType.DateTime},
                    {typeof (short), DbType.Int16},
			  		{typeof (short?), DbType.Int16},
			  		{typeof (int), DbType.Int32},
			  		{typeof (int?), DbType.Int32},
			  		{typeof (long), DbType.Int64},
			  		{typeof (long?), DbType.Int64},
			  		{typeof (bool), DbType.Boolean},
			  		{typeof (bool?), DbType.Boolean},
			  		{typeof (byte[]), DbType.Binary},
                    {typeof (byte?[]), DbType.Binary},
			  		{typeof (decimal), DbType.Decimal},
			  		{typeof (decimal?), DbType.Decimal},
			  		{typeof (double), DbType.Double},
			  		{typeof (double?), DbType.Double},
			  		{typeof (float), DbType.Single},
			  		{typeof (float?), DbType.Single},
			  		{typeof (Guid), DbType.Guid},
			  		{typeof (Guid?), DbType.Guid},
			  		{typeof (Enum), DbType.Int32},
			  	};

		public static DbType ConvertToDbType(Type dotNetType)
		{
			return dotNetTypeToDbType[dotNetType];
		}

		public static string CoaleaseNull(object value)
		{
			string nullExpression = "is not null";

			if (value == null)
			{
				nullExpression = "is null";
			}

			return nullExpression;
		}

		public static string CoaleseBool(object value)
		{
			bool boolean;

			if (!Boolean.TryParse(value.ToString(), out boolean))
			{
				return CoaleaseNull(null);
			}

			return boolean ? "1" : "0";
		}

		public static byte[] CoaleseByteArray(object value)
		{
			byte[] byteArray = null;

			if (typeof (byte[]).IsAssignableFrom(value.GetType()))
			{
				return (byte[]) value;
			}

			return byteArray;
		}

		public static decimal? CoaleseDecimal(object value)
		{
			decimal @decimal = Decimal.Zero;

			if (!Decimal.TryParse(value.ToString(), out @decimal))
			{
				return null;
			}

			return @decimal;
		}

		public static int? CoaleseInt(object value)
		{
			int integer;

			if (!Int32.TryParse(value.ToString(), out integer))
			{
				return null;
			}

			return integer;
		}

		public static DateTime? CoaleseDate<T>(object value)
		{
			DateTime dateTime;

			if (!DateTime.TryParse(value.ToString(), out dateTime))
			{
				return null;
			}

			return dateTime;
		}
	}
}