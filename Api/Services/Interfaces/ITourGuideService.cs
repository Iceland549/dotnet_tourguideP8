using GpsUtil.Location;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        Task<List<object>> GetNearByAttractionsAsync(VisitedLocation visitedLocation, User user);
        List<Provider> GetTripDeals(User user);
        User? GetUser(string userName);
        Task<VisitedLocation> GetUserLocationAsync(User user);
        List<UserReward> GetUserRewards(User user);
        Task<VisitedLocation> TrackUserLocationAsync(User user);
        Task<List<Attraction>> GetAllAttractionsAsync();
        double GetDistance(Locations loc1, Locations loc2);
        Task<int> GetRewardPointsAsync(Attraction attraction, User user);
    }
}