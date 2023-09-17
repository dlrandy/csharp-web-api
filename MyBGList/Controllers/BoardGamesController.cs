﻿using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;
using MyBGList.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace MyBGList.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardGameController : ControllerBase
{

    private readonly ApplicationDbContext _context;

    private readonly ILogger<BoardGameController> _logger;

    public BoardGameController(
        ApplicationDbContext context,
        ILogger<BoardGameController> logger)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet(Name = "GetBoardGames")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    public async Task<RestDTO<BoardGame[]>> Get(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = "Name",
        string? sortOrder = "ASC",
        string? filterQuery = null
        )
    {

        var query = _context.BoardGames.AsQueryable();
        if (!string.IsNullOrEmpty(filterQuery)) {
            query = query.Where(b => b.Name.Contains(filterQuery));
        }
        var recordCount = await query.CountAsync();
                //.OrderBy(b => b.Name)
        query = query.OrderBy($"{sortColumn} {sortOrder}")
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
        var data = await query.ToArrayAsync();
        return new RestDTO<BoardGame[]>() {
            Data = data,
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = recordCount,
            Links = new List<LinkDTO>() {
               new LinkDTO(
                   Url.Action(null, "BoardGames", new { pageIndex, pageSize }, Request.Scheme)!,
                   "self",
                   "GET"
                   )
           }
        };
    }


    [HttpPost(Name = "UpdateBoardGame")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO model) {
        var boardgame = await _context.BoardGames
            .Where(b => b.Id == model.Id)
            .FirstOrDefaultAsync();
        if (boardgame != null) {
            if (!string.IsNullOrEmpty(model.Name))
                boardgame.Name = model.Name;
            if (model.Year.HasValue && model.Year.Value > 0)
                boardgame.Year = model.Year.Value;
            if (model.MinPlayers.HasValue && model.MinPlayers.Value > 0)
                boardgame.MinPlayers = model.MinPlayers.Value;
            if (model.MaxPlayers.HasValue && model.MaxPlayers.Value > 0)
                boardgame.MaxPlayers = model.MaxPlayers.Value;
            if (model.PlayTime.HasValue && model.PlayTime.Value > 0)
                boardgame.PlayTime = model.PlayTime.Value;
            if (model.MinAge.HasValue && model.MinAge.Value > 0)
                boardgame.MinAge = model.MinAge.Value;

            boardgame.LastModifiedDate = DateTime.Now;
            _context.BoardGames.Update(boardgame);
            await _context.SaveChangesAsync();
        };
        return new RestDTO<BoardGame?>() {
            Data = boardgame,
            Links = new List<LinkDTO> {
                new LinkDTO(Url.Action(null, "BoardGames", model, Request.Scheme)!, "self", "POST"), }

        };

    }

    [HttpDelete(Name = "DeleteBoardGame")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<BoardGame[]?>> Delete(string ids)
    {
        var idArray = ids.Split(',').Select(x => int.Parse(x));
        var deletedBGList = new List<BoardGame>();

        foreach (int id in idArray) {

          var boardgame = await _context.BoardGames
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();
            if (boardgame != null)
            {
                deletedBGList.Add(boardgame);
                _context.BoardGames.Remove(boardgame);
                await _context.SaveChangesAsync();
            };

        }

        return new RestDTO<BoardGame[]?>()
        {
            Data = deletedBGList.Count > 0 ? deletedBGList.ToArray():null,
            Links = new List<LinkDTO>
                {
                    new LinkDTO(
                            Url.Action(
                                null,
                                "BoardGames",
                                ids,
                                Request.Scheme)!,
                            "self",
                            "DELETE"),
                }
        };
    }


}

