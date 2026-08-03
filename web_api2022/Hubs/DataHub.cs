using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace web_api2022.Hubs
{
    public class DataHub : Hub
    {
        // 这里可以定义客户端与服务器之间交互的方法

        // 例如，广播消息给所有连接的客户端
        public async Task SendDataUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }

}