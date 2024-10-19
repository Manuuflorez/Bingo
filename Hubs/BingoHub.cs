using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bingoo.Models;

public class BingoHub : Hub
{
    private readonly BingoContext _context;
    private static Dictionary<string, List<string>> rooms = new Dictionary<string, List<string>>();

    // Constructor que inyecta el contexto de la base de datos
    public BingoHub(BingoContext context)
    {
        _context = context;
    }

    public async Task JoinRoom(string roomId, string userName)
    {
        if (!rooms.ContainsKey(roomId))
        {
            rooms[roomId] = new List<string>();
        }

        var room = rooms[roomId];

        // Permitir que los jugadores que ya estuvieron en la sala vuelvan a unirse
        if (!room.Contains(userName))
        {
            room.Add(userName);
        }

        // Unir al grupo de SignalR para la sala
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Notificar a todos los usuarios en la sala que un nuevo usuario se ha unido
        await Clients.Group(roomId).SendAsync("UserJoined", userName);
        await Clients.Group(roomId).SendAsync("UpdateUserList", room);
    }
    public async Task BroadcastBall(string roomId, int ballNumber)
    {
        await Clients.Group(roomId).SendAsync("ReceiveBall", ballNumber);
    }

    public async Task AnnounceWinner(string roomId, string winnerName)
    {
        // Notificar a todos los jugadores de la sala quién ha ganado
        await Clients.Group(roomId).SendAsync("GameWon", winnerName);
    }

    public async Task LeaveRoom(string roomId, string userName)
    {
        if (rooms.ContainsKey(roomId))
        {
            rooms[roomId].Remove(userName);
            await Clients.Group(roomId).SendAsync("UpdateUserList", rooms[roomId]);

            if (rooms[roomId].Count == 0)
            {
                rooms.Remove(roomId);
            }
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var room in rooms)
        {
            if (room.Value.Contains(Context.ConnectionId))
            {
                room.Value.Remove(Context.ConnectionId);
                await Clients.Group(room.Key).SendAsync("UpdateUserList", room.Value);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
     public async Task PauseGame(string roomId)
    {
        await Clients.Group(roomId).SendAsync("PauseGame");
    }

    public async Task ResumeGame(string roomId)
    {
        await Clients.Group(roomId).SendAsync("ResumeGame");
    }
}
