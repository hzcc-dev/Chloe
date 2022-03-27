﻿using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    partial class Query<T> : QueryBase, IQuery<T>, IQuery
    {
        public async Task<T> FirstAsync()
        {
            var q = (Query<T>)this.Take(1);
            var iterator = q.GenerateIterator();
            return await iterator.FirstAsync();
        }
        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.Where(predicate).FirstAsync();
        }
        public async Task<T> FirstOrDefaultAsync()
        {
            var q = (Query<T>)this.Take(1);
            var iterator = q.GenerateIterator();
            return await iterator.FirstOrDefaultAsync();
        }
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.Where(predicate).FirstOrDefaultAsync();
        }
        public Task<List<T>> ToListAsync()
        {
            return this.GenerateIterator().ToListAsync().AsTask();
        }

        public async Task<bool> AnyAsync()
        {
            string v = "1";
            var q = (Query<string>)this.Select(a => v).Take(1);
            return await q.GenerateIterator().AnyAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.Where(predicate).AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await this.ExecuteAggregateQueryAsync<int>(GetCalledMethod(() => default(IQuery<T>).Count()), null, false);
        }
        public async Task<long> LongCountAsync()
        {
            return await this.ExecuteAggregateQueryAsync<long>(GetCalledMethod(() => default(IQuery<T>).LongCount()), null, false);
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<TResult>(GetCalledMethod(() => default(IQuery<T>).Max(default(Expression<Func<T, TResult>>))), selector);
        }
        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<TResult>(GetCalledMethod(() => default(IQuery<T>).Min(default(Expression<Func<T, TResult>>))), selector);
        }

        public async Task<int> SumAsync(Expression<Func<T, int>> selector)
        {
            return await this.ExecuteSumAsync<int>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int>>))), selector);
        }
        public async Task<int> SumAsync(Expression<Func<T, int?>> selector)
        {
            return await this.ExecuteSumAsync<int>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int?>>))), selector);
        }
        public async Task<long> SumAsync(Expression<Func<T, long>> selector)
        {
            return await this.ExecuteSumAsync<long>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long>>))), selector);
        }
        public async Task<long> SumAsync(Expression<Func<T, long?>> selector)
        {
            return await this.ExecuteSumAsync<long>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long?>>))), selector);
        }
        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            return await this.ExecuteSumAsync<decimal>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal>>))), selector);
        }
        public async Task<decimal> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            return await this.ExecuteSumAsync<decimal>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal?>>))), selector);
        }
        public async Task<double> SumAsync(Expression<Func<T, double>> selector)
        {
            return await this.ExecuteSumAsync<double>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double>>))), selector);
        }
        public async Task<double> SumAsync(Expression<Func<T, double?>> selector)
        {
            return await this.ExecuteSumAsync<double>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double?>>))), selector);
        }
        public async Task<float> SumAsync(Expression<Func<T, float>> selector)
        {
            return await this.ExecuteSumAsync<float>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float>>))), selector);
        }
        public async Task<float> SumAsync(Expression<Func<T, float?>> selector)
        {
            return await this.ExecuteSumAsync<float>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float?>>))), selector);
        }
        async Task<TResult> ExecuteSumAsync<TResult>(MethodInfo method, Expression argument, bool checkArgument = true) where TResult : struct
        {
            return await this.ExecuteAggregateQueryAsync<TResult?>(method, argument) ?? default(TResult);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, int>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int>>))), selector);
        }
        public async Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int?>>))), selector);
        }
        public async Task<double?> AverageAsync(Expression<Func<T, long>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long>>))), selector);
        }
        public async Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long?>>))), selector);
        }
        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal>>))), selector);
        }
        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal?>>))), selector);
        }
        public async Task<double?> AverageAsync(Expression<Func<T, double>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double>>))), selector);
        }
        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double?>>))), selector);
        }
        public async Task<float?> AverageAsync(Expression<Func<T, float>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float>>))), selector);
        }
        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float?>>))), selector);
        }
    }
}
