using Microsoft.AspNetCore.SignalR;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TendernHub : Hub<ITenderClient>
    {
        // No implementation needed here
    }
    public interface IClock
    {
        Task ShowTime(DateTime currentTime);
    }
    public interface ITenderClient
    {
        Task UpdateAuctionStatus(int TenderId);
    }
}
