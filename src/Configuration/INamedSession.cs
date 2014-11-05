namespace MicroORM.Configuration
{
    public interface INamedSession
    {
         ISession Session { get; set; }
    }
}