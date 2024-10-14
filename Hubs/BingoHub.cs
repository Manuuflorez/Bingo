using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BingoHub : Hub
{
    // Diccionario para llevar un registro de los usuarios conectados por sala
    private static Dictionary<string, List<string>> rooms = new Dictionary<string, List<string>>();

    // Método que se llama cuando un usuario se une a una sala
    public async Task JoinRoom(string roomId, string userName)
    {
        // Añadir el usuario a la sala
        if (!rooms.ContainsKey(roomId))
        {
            rooms[roomId] = new List<string>();
        }

        rooms[roomId].Add(userName);

        // Notificar a todos los usuarios en la sala que un nuevo usuario se ha unido
        await Clients.Group(roomId).SendAsync("UserJoined", userName);

        // Unir al grupo de SignalR para la sala
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Actualizar la lista de usuarios conectados para todos los usuarios en la sala
        await Clients.Group(roomId).SendAsync("UpdateUserList", rooms[roomId]);
    }

    // Método que se llama cuando un usuario deja la sala
    public async Task LeaveRoom(string roomId, string userName)
    {
        if (rooms.ContainsKey(roomId))
        {
            rooms[roomId].Remove(userName);

            // Notificar a todos los usuarios en la sala que un usuario se ha ido
            await Clients.Group(roomId).SendAsync("UserLeft", userName);

            // Actualizar la lista de usuarios conectados
            await Clients.Group(roomId).SendAsync("UpdateUserList", rooms[roomId]);

            // Si no quedan usuarios, eliminar la sala
            if (rooms[roomId].Count == 0)
            {
                rooms.Remove(roomId);
            }
        }

        // Salir del grupo de SignalR para la sala
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    // Método que se llama cuando el usuario se desconecta
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

}
