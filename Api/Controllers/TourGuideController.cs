﻿using GpsUtil.Location;
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
    public async Task<ActionResult<VisitedLocation>> GetLocationAsync([FromQuery] string userName)
    {
        var location = await _tourGuideService.GetUserLocationAsync(GetUser(userName));
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
    public async Task<ActionResult> GetNearbyAttractionsAsync([FromQuery] string userName)
    {
        var user = GetUser(userName);
        var visitedLocation = await _tourGuideService.GetUserLocationAsync(user);
        var nearbyAttractions = await _tourGuideService.GetNearByAttractionsAsync(visitedLocation, user);
        return Ok(nearbyAttractions);
    }

    [HttpGet("getRewards")]
    public async Task<ActionResult<List<object>>> GetRewards([FromQuery] string userName)
    {
        var user = GetUser(userName);
        var visitedLocation = await _tourGuideService.GetUserLocationAsync(user);
        var closestAttraction = await _tourGuideService.GetRewardAttractionAsync(visitedLocation, user);
        return Ok(closestAttraction);
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
