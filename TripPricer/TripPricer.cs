using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPricer.Helpers;

namespace TripPricer;

public class TripPricer
{
    public List<Provider> GetPrice(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardsPoints)
    {
        List<Provider> providers = new List<Provider>();

        // Liste statique de noms uniques
        var availableProviders = new List<string>
    {
        "Holiday Travels",
        "Enterprize Ventures Limited",
        "Sunny Days",
        "FlyAway Trips",
        "United Partners Vacations",
        "Dream Trips",
        "Live Free",
        "Dancing Waves Cruselines and Partners",
        "AdventureCo",
        "Cure-Your-Blues",
        "TravelJoy",
        "GlobalEscapes",
        "Wanderlust",
        "SkyHigh",
        "OceanBreeze",
        "ExploreMore",
        "VoyageElite",
        "SunsetTours",
        "MountainQuest",
        "RiverCruise"
    };

        Thread.Sleep(ThreadLocalRandom.Current.Next(1, 50));

        for (int i = 0; i < 10; i++)
        {
            int multiple = ThreadLocalRandom.Current.Next(100, 700);
            double childrenDiscount = children / 3.0;
            double price = multiple * adults + multiple * childrenDiscount * nightsStay + 0.99 - rewardsPoints;

            if (price < 0.0)
            {
                price = 0.0;
            }

            string provider = availableProviders[i];
            providers.Add(new Provider(attractionId, provider, price));
        }
        return providers;
    }
}
