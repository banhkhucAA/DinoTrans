using DinoTrans.Shared.DTOs.TendersActive;
using DinoTrans.Shared.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TenderOffersHub : Hub
    {
        public async Task SendNewBid(TenderBid tenderBid)
        {
            await Clients.All.SendAsync("ReceiveNewBid", tenderBid);
        }
    }
}
