using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO;

namespace MyBGList.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardGameController : ControllerBase
{
 

    private readonly ILogger<BoardGameController> _logger;

    public BoardGameController(ILogger<BoardGameController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetBoardGames")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    public RestDTO<BoardGame[]> Get()
    {
        return new RestDTO<BoardGame[]>() {
           Data = new[] {
            new BoardGame(){
            Id = 1,
            Name = "Axis & Allies",
            Year = 1981,
            MinPlayers = 2,
                MaxPlayers = 5
            },
             new BoardGame(){
            Id = 2,
            Name = "Citadels",
            Year = 2000,
            MinPlayers = 1,
            MaxPlayers = 3
            },
              new BoardGame(){
            Id = 3,
            Name = "Terraforming Mars",
            Year = 2016,
            MinPlayers = 2,
            MaxPlayers = 6
            },
           },
           Links = new List<LinkDTO>() {
               new LinkDTO(
                   Url.Action(null, "BoardGames", null, Request.Scheme)!,
                   "self",
                   "GET"
                   )
           }
        };
    }
}

