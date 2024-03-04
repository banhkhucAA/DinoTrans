using DinoTrans.Shared.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TenderOffersHub : Hub
    {
        public async Task SendNewBids(List<TenderBid> newBids)
        {
            await Clients.All.SendAsync("ReceiveNewBids", newBids);
        }
    }
}
