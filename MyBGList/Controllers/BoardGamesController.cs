using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;
using MyBGList.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;
using MyBGList.Constants;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;

namespace MyBGList.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardGameController : ControllerBase
{

    private readonly ApplicationDbContext _context;

    private readonly ILogger<BoardGameController> _logger;

    private readonly IMemoryCache _memoryCache;

    public BoardGameController(
        ApplicationDbContext context,
        ILogger<BoardGameController> logger,
        IMemoryCache memoryCache
        )
    {
        _logger = logger;
        _context = context;
        _memoryCache = memoryCache;
    }

    [HttpGet(Name = "GetBoardGames")]
    //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    //[ResponseCache(CacheProfileName = "Any-60")]
    [ResponseCache(CacheProfileName = "Client-120")]
    public async Task<RestDTO<BoardGame[]>> Get(
        //int pageIndex = 0,
        //[Range(1, 100)] int pageSize = 10,
        ////[RegularExpression("ASC|DESC")]string? sortOrder = "ASC",
        //[SortOrderValidator(
        //    ErrorMessage = "Value should be one the following: {0}.",
        //    AllowedValues = new[] {"ASC", "DESC","OtherString"}),
        //] string? sortOrder = "ASC",
        ////string? sortColumn = "Name",
        //[
        //    SortColumnValidator(typeof(BoardGameDTO))
        //] string? sortColumn = "Name",
        //string? filterQuery = null
        [FromQuery] RequestDTO<BoardGameDTO> input
        )
    {
        _logger.LogInformation(CustomLogEvents.BoardGamesController_Get,"Get method started! [{MachineName}] [{ThreadId}].",
            Environment.MachineName,
            Environment.CurrentManagedThreadId
            );
        var query = _context.BoardGames.AsQueryable();
        if (!string.IsNullOrEmpty(input.FilterQuery)) {
            query = query.Where(b => b.Name.Contains(input.FilterQuery));
        }
        (int recordCount, BoardGame[]? result) dataTuple = (0,null);
        //.OrderBy(b => b.Name)

        //BoardGame[]? result = null;
        var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";
        Console.WriteLine("----{0}",cacheKey);
        if (!_memoryCache.TryGetValue<(int recordCount, BoardGame[]? result)/*BoardGame[]*/>(cacheKey, out dataTuple/*result*/))
        {

            // TODO: 使用all优化
        dataTuple.recordCount = await query.CountAsync();
        query = query.OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);
          //result = await query.ToArrayAsync();
          dataTuple.result = await query.ToArrayAsync();
            _memoryCache.Set(cacheKey, dataTuple/*result*/, new TimeSpan(0,0,30));
        }
        return new RestDTO<BoardGame[]>() {
            Data = dataTuple.result,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            RecordCount = dataTuple.recordCount,
            Links = new List<LinkDTO>() {
               new LinkDTO(
                   Url.Action(null, "BoardGames", new { input.PageIndex, input.PageSize }, Request.Scheme)!,
                   "self",
                   "GET"
                   )
           }
        };
    }

    [Authorize(Roles = RoleNames.Moderator)]
    [HttpPost(Name = "UpdateBoardGame")]
    //[ResponseCache(NoStore = true)]
    [ResponseCache(CacheProfileName = "NoCache")]
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
    [Authorize(Roles = RoleNames.Administrator)]
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

    [HttpGet("{id}")]
    [ResponseCache(CacheProfileName = "Any-60")]
    public async Task<RestDTO<BoardGame?>> GetBoardGame(int id)
    {
        _logger.LogInformation(CustomLogEvents.BoardGamesController_Get,"GetBoardGame method started.");
        BoardGame? result = null;
        var cachekey = $"GetBoardGame-{id}";
        if (!_memoryCache.TryGetValue<BoardGame>(cachekey, out result))
        {
            result = await _context.BoardGames
                // eager loading
                //.Include(it=>it.BoardGames_Domains)
                //.Include(it=>it.BoardGames_Mechanics)
                .FirstOrDefaultAsync(bg => bg.Id == id);
            //var options = new JsonSerializerOptions
            //{
            //    //ReferenceHandler = ReferenceHandler.Preserve,
            //    MaxDepth = 64
            //};
            //Console.WriteLine(result);
            //var json = JsonSerializer.Serialize(result, options);

            _memoryCache.Set(cachekey, result, new TimeSpan(0,0,30));
        }

        return new RestDTO<BoardGame?>() {
            Data = result,
            PageIndex = 0,
            PageSize = 1,
            RecordCount = result != null ? 1 : 0,
            Links = new List<LinkDTO> {
                new LinkDTO (
                    Url.Action(null, "BoardGames", new { id}, Request.Scheme)!,
                    "self",
                    "GET"
                    )
            }

        };
    }


}

