using System;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MyBGList.Constants;
using MyBGList.DTO;
using MyBGList.Models;

namespace MyBGList.GraphQL
{
	public class Mutation
	{
		[Serial]
		[Authorize(Roles = new[] { RoleNames.Moderator})]
		public async Task<BoardGame?> UpdateBoardGame(
			[Service]
			ApplicationDbContext context,
			BoardGameDTO boardGameDTO
			) {
			var boardGame = await context.BoardGames
				.Where(b => b.Id == boardGameDTO.Id)
				.FirstOrDefaultAsync();
			if (boardGame != null)
			{
				if (!string.IsNullOrEmpty(boardGameDTO.Name))
				{
					boardGame.Name = boardGameDTO.Name;
				}
				if (boardGameDTO.Year.HasValue && boardGameDTO.Year.Value > 0)
				{
					boardGame.Year = boardGameDTO.Year.Value;
				}
				boardGame.LastModifiedDate = DateTime.Now;
				context.BoardGames.Update(boardGame);
				await context.SaveChangesAsync();
			}
			return boardGame;
		}

		[Serial]
		[Authorize(Roles = new[] { RoleNames.Administrator })]
		public async Task DeleteBoardGame([Service] ApplicationDbContext context, int id) {

			var boardGame = await context.BoardGames
				.Where(b => b.Id == id)
				.FirstOrDefaultAsync();
			if (boardGame != null)
			{
				context.BoardGames.Remove(boardGame);
				await context.SaveChangesAsync();
			}

		}

        [Serial]
        [Authorize(Roles = new[] { RoleNames.Moderator })]
        public async Task<Domain?> UpdateDomain(
         [Service] ApplicationDbContext context, DomainDTO model)
        {
            var domain = await context.Domains
                .Where(d => d.Id == model.Id)
                .FirstOrDefaultAsync();
            if (domain != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    domain.Name = model.Name;
                domain.LastModifiedDate = DateTime.Now;
                context.Domains.Update(domain);
                await context.SaveChangesAsync();
            }
            return domain;
        }

        [Serial]
        [Authorize(Roles = new[] { RoleNames.Administrator })]
        public async Task DeleteDomain(
            [Service] ApplicationDbContext context, int id)
        {
            var domain = await context.Domains
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
            if (domain != null)
            {
                context.Domains.Remove(domain);
                await context.SaveChangesAsync();
            }
        }

        [Serial]
        [Authorize(Roles = new[] { RoleNames.Moderator })]
        public async Task<Mechanic?> UpdateMechanic(
            [Service] ApplicationDbContext context, MechanicDTO model)
        {
            var mechanic = await context.Mechanics
                .Where(m => m.Id == model.Id)
                .FirstOrDefaultAsync();
            if (mechanic != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    mechanic.Name = model.Name;
                mechanic.LastModifiedDate = DateTime.Now;

                context.Mechanics.Update(mechanic);
                await context.SaveChangesAsync();
            }
            return mechanic;
        }

        [Serial]
        [Authorize(Roles = new[] { RoleNames.Administrator })]
        public async Task DeleteMechanic(
            [Service] ApplicationDbContext context, int id)
        {
            var mechanic = await context.Mechanics
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
            if (mechanic != null)
            {
                context.Mechanics.Remove(mechanic);
                await context.SaveChangesAsync();
            }
        }
    }
}

