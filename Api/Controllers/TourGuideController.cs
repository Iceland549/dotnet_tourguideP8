using GpsUtil.Location;
using Microsoft.AspNetCore.Mvc;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TripPricer;

namespace TourGuide.Controllers;

[ApiController]
[Route("[controller]")]
public class TourGuideController : ControllerBase
{
    private readonly ITourGuideService _tourGuideService;

    public TourGuideController(ITourGuideService tourGuideService)
    {
        _tourGuideService = tourGuideService;
    }

    [HttpGet("getLocation")]
    public ActionResult<VisitedLocation> GetLocation([FromQuery] string userName)
    {
        var location = _tourGuideService.GetUserLocation(GetUser(userName));
        return Ok(location);
    }

    // TODO: Change this method to no longer return a List of Attractions.
    // Instead: Get the closest five tourist attractions to the user - no matter how far away they are.
    // Return a new JSON object that contains:
    // Name of Tourist attraction, 
    // Tourist attractions lat/long, 
    // The user's location lat/long, 
    // The distance in miles between the user's location and each of the attractions.
    // The reward points for visiting each Attraction.
    //    Note: Attraction reward points can be gathered from RewardsCentral
    [HttpGet("getNearbyAttractions")]
    public ActionResult GetNearbyAttractions([FromQuery] string userName)
    {
        var visitedLocation = _tourGuideService.GetUserLocation(GetUser(userName));
        var userLocation = visitedLocation.Location;
        var user = GetUser(userName);
        var attractions = _tourGuideService.GetAllAttractions()
            .Select(attraction => new
            {
                attractionName = attraction.AttractionName,
                attractionLatitude = attraction.Latitude,
                attractionLongitude = attraction.Longitude,
                userLatitude = userLocation.Latitude,
                userLongitude = userLocation.Longitude,
                distanceInMiles = _tourGuideService.GetDistance(userLocation, new Locations(attraction.Latitude, attraction.Longitude)),
                rewardPoints = _tourGuideService.GetRewardPoints(attraction, user)
            })
            .OrderBy(x => x.distanceInMiles)
            .Take(5)
            .ToList(); 
        return Ok(attractions);
    }

    [HttpGet("getRewards")]
    public ActionResult<List<UserReward>> GetRewards([FromQuery] string userName)
    {
        var rewards = _tourGuideService.GetUserRewards(GetUser(userName));
        return Ok(rewards);
    }

    [HttpGet("getTripDeals")]
    public ActionResult<List<Provider>> GetTripDeals([FromQuery] string userName)
    {
        var deals = _tourGuideService.GetTripDeals(GetUser(userName));
        return Ok(deals);
    }

    private User GetUser(string userName)
    {
        return _tourGuideService.GetUser(userName);
    }

}
