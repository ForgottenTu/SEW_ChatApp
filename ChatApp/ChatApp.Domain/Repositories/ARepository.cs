using ChatApp.Domain.Interfaces;
using ChatApp.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ARepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ChatAppContext Context;
    protected readonly DbSet<TEntity> Table;

    protected ARepository(ChatAppContext context)
    {
        this.Context = context;
        Table = context.Set<TEntity>();
    }
    
    public async Task<IEnumerable<TEntity>?> GetAllAsync()
    {
        return await Table.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await Table.FindAsync(id);
    }

    public async Task<TEntity?> AddAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var keyProperty = Context.Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties[0];
        var keyValue = keyProperty.PropertyInfo!.GetValue(entity);
        
        var existingEntity = await Table.FindAsync(keyValue);

        if (existingEntity != null)
            return null;
        
        await Table.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<TEntity>?> CreateRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityType = Context.Model.FindEntityType(typeof(TEntity));
        var keyProperty = entityType.FindPrimaryKey().Properties.First();

        var addedEntities = new List<TEntity>();

        foreach (var entity in entities)
        {
            var keyValue = keyProperty.PropertyInfo.GetValue(entity);

            var existingEntity = await Table.FindAsync(keyValue);

            if (existingEntity != null) continue;
            await Table.AddAsync(entity);
            addedEntities.Add(entity);
        }

        if (addedEntities.Count == 0)
        {
            return null;
        }

        await Context.SaveChangesAsync();

        return addedEntities;
    }


    public async Task<TEntity> UpSertAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var keyProperty = Context.Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties
            .First();
        var keyValue = keyProperty.PropertyInfo!.GetValue(entity);
        
        var existingEntity = await Table.FindAsync(keyValue);

        if (existingEntity == null)
            await Table.AddAsync(entity);
        else
            Context.Entry(existingEntity).CurrentValues.SetValues(entity);

        await Context.SaveChangesAsync();

        return entity;
    }

    public async Task<IEnumerable<TEntity>> UpSertRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        
        var entityType = Context.Model.FindEntityType(typeof(TEntity));
        var keyProperty = entityType!.FindPrimaryKey()!.Properties[0];

        foreach (var entity in entities)
        {
            var keyValue = keyProperty.PropertyInfo!.GetValue(entity);

            var existingEntity = await Table.FindAsync(keyValue);

            if (existingEntity == null)
                await Table.AddAsync(entity);
            else
                Context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        await Context.SaveChangesAsync();

        return entities;
    }

    public async Task<TEntity?> DeleteAsyncById(int id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var entity = await Table.FindAsync(id);
        if (entity is null) return null;
        
        Table.Remove(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<TEntity?> DeleteAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entityType = Context.Model.FindEntityType(typeof(TEntity));
        var keyProperty = entityType!.FindPrimaryKey()!.Properties[0];

        var keyValue = keyProperty.PropertyInfo!.GetValue(entity);
        var existingEntity = await Table.FindAsync(keyValue);

        if (existingEntity == null) return null;
        
        Table.Remove(existingEntity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<TEntity>?> DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityType = Context.Model.FindEntityType(typeof(TEntity));
        var keyProperty = entityType!.FindPrimaryKey()!.Properties[0];
        var deletedEntities = new List<TEntity>();

        foreach (var entity in entities)
        {
            var keyValue = keyProperty.PropertyInfo!.GetValue(entity);
            var existingEntity = await Table.FindAsync(keyValue);
            
            if (existingEntity == null) continue;
            
            deletedEntities.Add(existingEntity);
            Table.Remove(existingEntity);
        }
        await Context.SaveChangesAsync();

        return deletedEntities;
    }
    
    public async Task<IEnumerable<TEntity>?> GetPagedAsync(int page, int pageSize)
    {
        return await Table.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    
}