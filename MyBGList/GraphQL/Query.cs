using System;
using MyBGList.Models;

namespace MyBGList.GraphQL
{
	public class Query
	{
		[Serial] // 也可以使用AddDbContextFactory的方式注册Context以保证线程安全，graphql的查询可以是parallel
		[UsePaging]
		[UseProjection]
		[UseFiltering]
		[UseSorting]
		public IQueryable<BoardGame> GetBoardGames([Service] ApplicationDbContext context) => context.BoardGames;

        [Serial]
        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Domain> GetDomains([Service] ApplicationDbContext context) => context.Domains;

        [Serial]
        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Mechanic> GetMechanics([Service] ApplicationDbContext context) => context.Mechanics;
    }
}

