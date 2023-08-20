using Microsoft.AspNetCore.Mvc;

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

    [HttpGet(Name = "GetBoardGame")]
    public IEnumerable<BoardGame> Get()
    {
        return new[] {
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
        };
    }
}

