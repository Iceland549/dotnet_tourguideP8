﻿using GpsUtil.Location;
using Microsoft.Extensions.Logging;
using RewardCentral;
using System.Diagnostics;
using System.Globalization;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services;

public class TourGuideService : ITourGuideService
{
    private readonly ILogger _logger;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardsService _rewardsService;
    private readonly TripPricer.TripPricer _tripPricer;
    public Tracker Tracker { get; private set; }
    private readonly Dictionary<string, User> _internalUserMap = new();
    private const string TripPricerApiKey = "test-server-api-key";
    private bool _testMode = true;

    public TourGuideService(ILogger<TourGuideService> logger, IGpsUtil gpsUtil, IRewardsService rewardsService, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _tripPricer = new();
        _gpsUtil = gpsUtil;
        _rewardsService = rewardsService;

        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        if (_testMode)
        {
            _logger.LogInformation("TestMode enabled");
            _logger.LogDebug("Initializing users");
            InitializeInternalUsers();
            _logger.LogDebug("Finished initializing users");
        }

        var trackerLogger = loggerFactory.CreateLogger<Tracker>();

        Tracker = new Tracker(this, trackerLogger);
        AddShutDownHook();
    }

    public List<UserReward> GetUserRewards(User user)
    {
        return user.UserRewards;
    }

    public async Task<VisitedLocation> GetUserLocationAsync(User user)
    {
        return user.VisitedLocations.Any() ? user.GetLastVisitedLocation() : await TrackUserLocationAsync(user);
    }

    public User? GetUser(string userName)
    {
        return _internalUserMap.ContainsKey(userName) ? _internalUserMap[userName] : null;
    }

    public List<User> GetAllUsers()
    {
        return _internalUserMap.Values.ToList();
    }

    public void AddUser(User user)
    {
        if (!_internalUserMap.ContainsKey(user.UserName))
        {
            _internalUserMap.Add(user.UserName, user);
        }
    }

    public List<Provider> GetTripDeals(User user)
    {
        int cumulativeRewardPoints = user.UserRewards.Sum(i => i.RewardPoints);
        List<Provider> providers = _tripPricer.GetPrice(TripPricerApiKey, user.UserId,
            user.UserPreferences.NumberOfAdults, user.UserPreferences.NumberOfChildren,
            user.UserPreferences.TripDuration, cumulativeRewardPoints);
        user.TripDeals = providers;
        return providers;
    }

    public async Task<VisitedLocation> TrackUserLocationAsync(User user)
    {
        VisitedLocation visitedLocation = await _gpsUtil.GetUserLocationAsync(user.UserId);
        user.AddToVisitedLocations(visitedLocation);
        await _rewardsService.CalculateRewardsAsync(user);
        return visitedLocation;
    }

    public async Task<List<object>> GetNearByAttractionsAsync(VisitedLocation visitedLocation, User user)
    {
        var userLocation = visitedLocation.Location;
        var allAttractions = await _gpsUtil.GetAttractionsAsync();

        var nearbyAttractionsTasks = allAttractions
            .Select(async attraction => new
            {
                attractionName = attraction.AttractionName,
                attractionLatitude = attraction.Latitude,
                attractionLongitude = attraction.Longitude,
                userLatitude = userLocation.Latitude,
                userLongitude = userLocation.Longitude,

                distanceInMiles = _rewardsService.GetDistance(userLocation, new Locations(attraction.Latitude, attraction.Longitude)),
                rewardPoints = await _rewardsService.GetRewardPointsAsync(attraction, user)
            });

        var nearbyAttractions = (await Task.WhenAll(nearbyAttractionsTasks))
            .OrderBy(x => x.distanceInMiles)
            .Take(5)
            .Cast<object>()
            .ToList();

        return nearbyAttractions;
    }

    public async Task<List<object>> GetRewardAttractionAsync(VisitedLocation visitedLocation, User user)
    {
        var userLocation = visitedLocation.Location;
        var allAttractions = await _gpsUtil.GetAttractionsAsync();

        var rewardAttractionTasks = allAttractions
            .Select(async attraction => new
            {
                visitedLocation = new
                {
                    userId = visitedLocation.UserId,
                    location = new
                    {
                        longitude = userLocation.Longitude,
                        latitude = userLocation.Latitude
                    },
                    timeVisited = visitedLocation.TimeVisited
                },
                attraction = new
                {
                    longitude = attraction.Longitude,
                    latitude = attraction.Latitude,
                    attractionName = attraction.AttractionName,
                    city = attraction.City,
                    state = attraction.State,
                    attractionId = attraction.AttractionId
                },
                distanceInMiles = _rewardsService.GetDistance(userLocation, new Locations(attraction.Latitude, attraction.Longitude)),
                rewardPoints = await _rewardsService.GetRewardPointsAsync(attraction, user)
            });

        var closestAttraction = (await Task.WhenAll(rewardAttractionTasks))
            .OrderBy(x => x.distanceInMiles)
            .Take(1)
            .Cast<object>()
            .ToList();

        return closestAttraction;
    }


    private void AddShutDownHook()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.StopTracking();
    }

    public async Task<List<Attraction>> GetAllAttractionsAsync()
    {
        return await _gpsUtil.GetAttractionsAsync();
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        return _rewardsService.GetDistance(loc1, loc2);
    }

    public async Task<int> GetRewardPointsAsync(Attraction attraction, User user)
    {
        return await _rewardsService.GetRewardPointsAsync(attraction, user);
    }

    /**********************************************************************************
    * 
    * Methods Below: For Internal Testing
    * 
    **********************************************************************************/

    private void InitializeInternalUsers()
    {
        for (int i = 0; i < InternalTestHelper.GetInternalUserNumber(); i++)
        {
            var userName = $"internalUser{i}";
            var user = new User(Guid.NewGuid(), userName, "000", $"{userName}@tourGuide.com");
            GenerateUserLocationHistory(user);
            _internalUserMap.Add(userName, user);
        }

        _logger.LogDebug($"Created {InternalTestHelper.GetInternalUserNumber()} internal test users.");
    }

    private void GenerateUserLocationHistory(User user)
    {
        for (int i = 0; i < 3; i++)
        {
            var visitedLocation = new VisitedLocation(user.UserId, new Locations(GenerateRandomLatitude(), GenerateRandomLongitude()), GetRandomTime());
            user.AddToVisitedLocations(visitedLocation);
        }
    }

    private static readonly Random random = new Random();

    private double GenerateRandomLongitude()
    {
        return new Random().NextDouble() * (180 - (-180)) + (-180);
    }

    private double GenerateRandomLatitude()
    {
        return new Random().NextDouble() * (90 - (-90)) + (-90);
    }

    private DateTime GetRandomTime()
    {
        return DateTime.UtcNow.AddDays(-new Random().Next(30));
    }
}
