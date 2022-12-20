using ChatDemo.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatDemo.Hubs
{
    public class ChatHub:Hub
    {

        private readonly IDictionary<string, UserConnection> _connection;


        public ChatHub(IDictionary<string, UserConnection> connection)
        {
            _connection = connection;
        }



        public async Task  JoinChat(string user,string room)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.GroupExcept(room,Context.ConnectionId).SendAsync("join",user, $"{user} has joined {room}");

            _connection[Context.ConnectionId] = new UserConnection { Room = room, UserName = user };

          

            await SendUsersConnected(room);

        }



        public Task SendUsersConnected(string room)
        {

            var users = _connection.Values.Where(c => c.Room == room)
                 .Select(c => c.UserName);


            return Clients.Group(room).SendAsync("UsersInRoom", users);


        }



        public async Task SendMessage(string Message)
        {

            var userConnection = _connection.Where(c => c.Key == Context.ConnectionId).Select(c=>c.Value).First();

            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.UserName,Message);


        }



        public async override Task OnDisconnectedAsync(Exception? exception)
        {

            if (_connection.Any(c => c.Key == Context.ConnectionId))
            {
                var userConnection = _connection.Where(c => c.Key == Context.ConnectionId).Select(c => c.Value).First();
                await Clients.GroupExcept(userConnection.Room, Context.ConnectionId).SendAsync("ReceiveMessage","MyCHat", $"{userConnection.UserName} has left");
                _connection.Remove(Context.ConnectionId);
                await SendUsersConnected(userConnection.Room);

            }
            await  base.OnDisconnectedAsync(exception);
        }
       







    }
}
