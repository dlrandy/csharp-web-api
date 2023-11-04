using System;
using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using MyBGList.gRPC;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using MyBGList.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Grpc.Net.Client.Web;

namespace MyBGList.Controllers
{
	[Route("[controller]/[action]")]
	[ApiController]
	public class GrpcController : ControllerBase
	{
		[HttpGet("{id}")]
		public async Task<BoardGameResponse> GetBoardGame(int id) {

            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var handler = new HttpClientHandler() {
                
            };
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using var channel = GrpcChannel.ForAddress("https://localhost:5001",
                new GrpcChannelOptions { HttpHandler =  new GrpcWebHandler(handler) });
     
			var client = new gRPC.Grpc.GrpcClient(channel);
			var response = await client.GetBoardGameAsync(new BoardGameRequest { Id=id});
			return response;


        }
        [HttpPost]
        public async Task<BoardGameResponse> UpdateBoardGame(
             string token,
             int id,
             string name)
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            using var channel = GrpcChannel
                .ForAddress("https://localhost:40443");
            var client = new gRPC.Grpc.GrpcClient(channel);
            var response = await client.UpdateBoardGameAsync(
                                new UpdateBoardGameRequest
                                {
                                    Id = id,
                                    Name = name
                                },
                                headers);
            return response;
        }


    }
}

