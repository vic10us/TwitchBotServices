using ChatBotPrime.Core.Data.Model;
using ChatBotPrime.Core.Data.Specifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotPrime.Core.Data
{
    public interface IRepository
    {
        Task<T> SingleAsync<T>(ISpecification<T> spec) where T : DataEntity;
        Task<List<T>> ListAsync<T>(ISpecification<T> spec = null) where T : DataEntity;
        Task<T> CreateAsync<T>(T dataItem) where T : DataEntity;
        Task<T> UpdateAsync<T>(T dataItem) where T : DataEntity;
        Task UpdateAsync<T>(List<T> dataItemList) where T : DataEntity;
        Task CreateAsync<T>(List<T> dataItemList) where T : DataEntity;
        Task RemoveAsync<T>(T dataItem) where T : DataEntity;
        Task RemoveAsync<T>(List<T> dataItems) where T : DataEntity;
    }
}
