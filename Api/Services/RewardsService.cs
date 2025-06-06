﻿using GpsUtil.Location;
using System.Collections.Concurrent;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 200;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    public async Task CalculateRewardsAsync(User user)
    {
        Interlocked.Increment(ref count);
        List<VisitedLocation> userLocations = user.VisitedLocations.ToList();
        List<Attraction> attractions = await _gpsUtil.GetAttractionsAsync();
        var tempRewards = new ConcurrentBag<UserReward>();

        var locationTasks = userLocations.Select(async visitedLocation =>
        {
            var attractionTasks = attractions.Select(async attraction =>
            {
                if (NearAttraction(visitedLocation, attraction))
                {
                    int points = await GetRewardPointsAsync(attraction, user);
                    tempRewards.Add(new UserReward(visitedLocation, attraction, points));
                }
            }).ToList();

            await Task.WhenAll(attractionTasks);
        }).ToList();

        await Task.WhenAll(locationTasks);


            foreach (var reward in tempRewards)
            {
                if (!user.UserRewards.Any(r => r.Attraction.AttractionName == reward.Attraction.AttractionName))
                {
                    user.AddUserReward(reward);
                }
            }
    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= _attractionProximityRange;
    }

    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    public async Task<int> GetRewardPointsAsync(Attraction attraction, User user)
    {
        return await _rewardsCentral.GetAttractionRewardPointsAsync(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}
