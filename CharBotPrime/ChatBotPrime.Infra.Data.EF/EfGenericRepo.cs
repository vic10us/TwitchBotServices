using ChatBotPrime.Core.Data;
using ChatBotPrime.Core.Data.Model;
using ChatBotPrime.Core.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBotPrime.Infra.Data.EF
{
	public class EfGenericRepo : IRepository
	{
		private readonly AppDataContext _db;

		public EfGenericRepo(AppDataContext db)
		{
			_db = db;
		}

		public async Task<T> SingleAsync<T>(ISpecification<T> spec) where T : DataEntity
		{
			IQueryable<T> setWithIncludes = SetWithIncludes(spec);
			return await setWithIncludes.SingleOrDefaultAsync(spec.Criteria);
		}

		public async Task<List<T>> ListAsync<T>(ISpecification<T> spec) where T : DataEntity
		{
			return spec != null
				? await SetWithIncludes(spec).Where(spec.Criteria).ToListAsync()
				: await _db.Set<T>().ToListAsync();
		}

		public async  Task<T> CreateAsync<T>(T dataItem) where T : DataEntity
		{
			await _db.Set<T>().AddAsync(dataItem);
			await _db.SaveChangesAsync();

			return dataItem;
		}

		public async Task<T> UpdateAsync<T>(T dataItem) where T : DataEntity
		{
			_db.Set<T>().Update(dataItem);
			await _db.SaveChangesAsync();

			return dataItem;
		}

		public async Task UpdateAsync<T>(List<T> dataItemList) where T : DataEntity
		{
			_db.Set<T>().UpdateRange(dataItemList);
			await _db.SaveChangesAsync();
		}

		public async Task CreateAsync<T>(List<T> dataItemList) where T : DataEntity
		{
			await _db.Set<T>().AddRangeAsync(dataItemList);
			await _db.SaveChangesAsync();
		}

		public async Task RemoveAsync<T>(T dataItem) where T : DataEntity
		{
			_db.Set<T>().Remove(dataItem);
			await _db.SaveChangesAsync();
		}

		public async Task RemoveAsync<T>(List<T> dataItems) where T : DataEntity
		{
			_db.Set<T>().RemoveRange(dataItems);
			await _db.SaveChangesAsync();
		}

		private IQueryable<T> SetWithIncludes<T>(ISpecification<T> spec) where T : DataEntity
		{
			var withExpressionIncludes = spec?.Includes
				.Aggregate(_db.Set<T>().AsQueryable(),
					(queryable, include) => queryable.Include(include));

			var withAllIncludes = spec?.IncludeStrings
				.Aggregate(withExpressionIncludes,
					(queryable, include) => queryable.Include(include));

			return withAllIncludes;
		}
	}
}
