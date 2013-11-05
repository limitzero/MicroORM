using System;
using System.Text;

namespace MicroORM.DataAccess.Querying.Impl
{
	public class StringWriterReader : ITextWriter, ITextReader
	{
		private StringBuilder builder;

		public StringWriterReader(StringBuilder builder)
		{
			this.builder = builder;
		}

		public void Write(object value)
		{
			builder.Append(value);
		}

		public string Read()
		{
			return builder.ToString();
		}
	}
}