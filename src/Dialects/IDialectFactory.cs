namespace MicroORM.Dialects
{
    /// <summary>
    /// Contract for creating instances of communication to the underlying database engine 
    /// and adapting certain features to the that engine to interface with the data retreival and 
    /// persistence session.
    /// </summary>
    public interface IDialectFactory
    {
        IDialect Create(string connectionString);
    }
}