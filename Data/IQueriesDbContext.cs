namespace AspNetCore.Specification.Data
{
    //With clean architecture at outer layer can reference anything from an inner layer.
    //IApplicationDbContext : IApplicationQueriesDbContext. can be stored within application layer.
    //IApplicationQueriesDbContext : IQueriesDbContext and Specifications (Include, Filter, OrderBy) are stored within the Domain so all layers have access.
    //modelBuilder.Entity<Query>().HasNoKey().ToView("ViewName");
    public interface IQueriesDbContext
    {
        SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>() where TEntity : class;
    }
}
