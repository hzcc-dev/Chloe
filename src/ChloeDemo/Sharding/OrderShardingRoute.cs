﻿using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using Chloe.Sharding.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo.Sharding
{
    public class OrderRouteTable : RouteTable
    {
        public OrderRouteTable(int month)
        {
            this.Month = month;
            this.Name = ShardingTest.BuildTableName(month);
        }

        public int Month { get; set; }
    }

    public class OrderRouteDataSource : RouteDataSource
    {
        public OrderRouteDataSource(int year)
        {
            this.Year = year;
            this.Name = year.ToString();
        }

        public int Year { get; set; }
    }

    /// <summary>
    /// Order 表路由
    /// </summary>
    public class OrderShardingRoute : IShardingRoute
    {
        ShardingTest _shardingTest;
        Dictionary<string, IRoutingStrategy> _routingStrategies = new Dictionary<string, IRoutingStrategy>();

        public OrderShardingRoute(ShardingTest shardingTest, List<int> years)
        {
            this._shardingTest = shardingTest;

            //添加路由规则(支持一个或多个字段联合分片)。ps：分片字段的路由规则必须要添加以外，也可以添加非分片字段路由规则作为辅助，以便缩小表范围，提高查询效率。

            //CreateTime 是分片字段，分片字段的路由规则必须要添加
            this._routingStrategies.Add(nameof(Order.CreateTime), new OrderCreateTimeRoutingStrategy(this));

            //非分片字段路由规则，根据实际情况可选择性添加
            this._routingStrategies.Add(nameof(Order.CreateDate), new OrderCreateDateRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateYear), new OrderCreateYearRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateMonth), new OrderCreateMonthRoutingStrategy(this));

            //所有分表
            this.AllTables = years.SelectMany(a => GetTablesByYear(a)).Reverse().ToList();

            //this.AllTables = GetRouteTablesByYear(2020).ToList();
            //this.AllTables = GetRouteTablesByYear(2020).Concat(GetRouteTablesByYear(2021)).Take(23).Reverse().ToList();
            //this.AllTables = GetTablesByYear(2020).Concat(GetTablesByYear(2021)).Reverse().ToList();
            //this.AllTables = this.AllTables.Take(2).ToList();
        }
        List<RouteTable> AllTables { get; set; }

        /// <summary>
        /// 创建所有分表对象，并设置数据源
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        IEnumerable<RouteTable> GetTablesByYear(int year)
        {
            for (int month = 1; month <= 12; month++)
            {
                var dbContextProviderFactory = new OrderDbContextProviderFactory(this._shardingTest, year);
                RouteTable table = new OrderRouteTable(month) { DataSource = new OrderRouteDataSource(year) { DbContextProviderFactory = dbContextProviderFactory } };

                yield return table;
            }
        }

        static IEnumerable<DateTime> GetDates(int year)
        {
            DateTime date = new DateTime(year, 1, 1).AddDays(-1);
            DateTime lastDate = new DateTime(year, 12, 31);

            while (true)
            {
                date = date.AddDays(1);
                if (date > lastDate)
                    break;

                yield return date;
            }
        }

        /// <summary>
        /// 获取所有的分片表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RouteTable> GetTables()
        {
            return this.AllTables;
        }

        /// <summary>
        /// 根据实体属性获取相应的路由规则
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public IRoutingStrategy GetStrategy(MemberInfo member)
        {
            this._routingStrategies.TryGetValue(member.Name, out var routingStrategy);
            return routingStrategy;
        }

        /// <summary>
        /// 根据排序字段对路由表重排。
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="orderings"></param>
        /// <returns></returns>
        public SortResult SortTables(List<RouteTable> tables, List<Ordering> orderings)
        {
            var firstOrdering = orderings.FirstOrDefault();

            MemberInfo member = null;
            if (firstOrdering.KeySelector.Body is System.Linq.Expressions.MemberExpression memberExp)
            {
                member = memberExp.Member;
            }

            if (member != null)
            {
                if (member == typeof(Order).GetProperty(nameof(Order.CreateTime)))
                {
                    //因为按照日期分表，在根据日期排序的情况下，我们对路由到的表进行一个排序，对分页有查询优化。
                    if (firstOrdering.Ascending)
                    {
                        return new SortResult() { IsOrdered = true, Tables = tables.OrderBy(a => (a.DataSource as OrderRouteDataSource).Year).ThenBy(a => (a as OrderRouteTable).Month).ToList() };
                    }

                    return new SortResult() { IsOrdered = true, Tables = tables.OrderByDescending(a => (a.DataSource as OrderRouteDataSource).Year).ThenByDescending(a => (a as OrderRouteTable).Month).ToList() };
                }
            }

            return new SortResult() { IsOrdered = false, Tables = tables };
        }
    }

    /// <summary>
    /// CreateTime 字段路由规则
    /// </summary>
    public class OrderCreateTimeRoutingStrategy : RoutingStrategy<DateTime>
    {
        public OrderCreateTimeRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month == createTime.Month);
        }

        public override IEnumerable<RouteTable> ForNotEqual(DateTime createTime)
        {
            return base.ForNotEqual(createTime);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month >= createTime.Month));
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(DateTime createTime)
        {
            return this.ForGreaterThan(createTime);
        }

        public override IEnumerable<RouteTable> ForLessThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month <= createTime.Month));
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(DateTime createTime)
        {
            return this.ForLessThan(createTime);
        }
    }
    /// <summary>
    /// CreateDate 字段路由规则
    /// </summary>
    public class OrderCreateDateRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateDateRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        int ParseCreateMonth(int createDate)
        {
            int month = int.Parse(createDate.ToString().Substring(4, 2));
            return month;
        }
        int GetCreateYear(int createDate)
        {
            int year = int.Parse(createDate.ToString().Substring(0, 4));
            return year;
        }

        public override IEnumerable<RouteTable> ForEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == this.GetCreateYear(createDate) && (a as OrderRouteTable).Month == this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createDate)
        {
            return base.ForNotEqual(createDate);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month >= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month >= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForLessThan(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month <= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month <= this.ParseCreateMonth(createDate));
        }
    }
    /// <summary>
    /// CreateYear 字段路由规则
    /// </summary>
    public class OrderCreateYearRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateYearRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == createYear);
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year != createYear);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createYear);
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= createYear);
        }

        public override IEnumerable<RouteTable> ForLessThan(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createYear);
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= createYear);
        }
    }
    /// <summary>
    /// CreateMonth 字段路由规则
    /// </summary>
    public class OrderCreateMonthRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateMonthRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month == createMonth);
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month != createMonth);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month > createMonth);
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month >= createMonth);
        }

        public override IEnumerable<RouteTable> ForLessThan(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month < createMonth);
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month <= createMonth);
        }
    }
}
