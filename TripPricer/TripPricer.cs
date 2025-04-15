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
        Console.WriteLine($"GetPrice called with attractionId: {attractionId}, adults: {adults}, children: {children}, nights: {nightsStay}, rewards: {rewardsPoints}");
        List<Provider> providers = new List<Provider>();
        HashSet<string> providersUsed = new HashSet<string>();

        // Sleep to simulate some latency
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

            string provider;
            do
            {
                provider = GetProviderName(apiKey, adults);
            } while (providersUsed.Contains(provider));

            providersUsed.Add(provider);
            providers.Add(new Provider(attractionId, provider, price));
        }
        Console.WriteLine($"GetPrice generated {providers.Count} providers");
        return providers;
    }

    public string GetProviderName(string apiKey, int adults)
    {
        int multiple = ThreadLocalRandom.Current.Next(1, 20);

        return multiple switch
        {
            1 => "Holiday Travels",
            2 => "Enterprize Ventures Limited",
            3 => "Sunny Days",
            4 => "FlyAway Trips",
            5 => "United Partners Vacations",
            6 => "Dream Trips",
            7 => "Live Free",
            8 => "Dancing Waves Cruselines and Partners",
            9 => "AdventureCo",
            10 => "Global Getaways",
            11 => "Skyhigh Tours",
            12 => "Ocean Breeze Travel",
            13 => "Wanderlux Agency",
            14 => "Epic Journey",
            15 => "Amazing",
            16 => "Why Not",
            17 => "Bucket List",
            18 => "One Life",
            19 => "No Way Home",
            _ => "Cure-Your-Blues",
        };
    }
}