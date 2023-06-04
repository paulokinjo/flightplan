using Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Data
{
    public class MongoDbDatabase : IFlightPlanDatabase<FlightPlan>
    {
        public async Task<List<FlightPlan>> GetAll()
        {
            var collection = GetCollection("local", "flightplans");
            var documents = collection.Find(_ => true).ToListAsync();
            var flightPlanList = new List<FlightPlan>();
            if (documents == null)
            {
                return flightPlanList;
            }

            foreach (var document in await documents)
            {
                flightPlanList.Add(ConvertBsonToFlightPlan(document));
            }

            return flightPlanList;
        }

        public async Task<FlightPlan> GetById(string id)
        {
            var collection = GetCollection("local", "flightplans");
            var flightPlanCursor = await collection.FindAsync(
                Builders<BsonDocument>.Filter.Eq("flight_plan_id", id));
            var document = flightPlanCursor.FirstOrDefault();

            var flightPlan = ConvertBsonToFlightPlan(document);
            if (flightPlan == null)
            {
                return new FlightPlan();
            }

            return flightPlan;
        }

        public async Task<TransactionResult> FileFlightPlan(FlightPlan flightPlan)
        {
            var collection = GetCollection("local", "flightplans");
            var document = new BsonDocument
            {
                { "flight_plan_id", Guid.NewGuid().ToString("N") },
                { "altitude", flightPlan.Altitude },
                { "airspeed", flightPlan.Airspeed },
                { "aircraft_identification", flightPlan.AircraftId },
                { "aircraft_type", flightPlan.AircraftType },
                { "arrival_airport", flightPlan.ArrivalAirport },
                { "flight_type", flightPlan.FlightType },
                { "departing_airport", flightPlan.DepartureAirport },
                { "departure_time", flightPlan.DepartureTime },
                { "estimated_arrival_time", flightPlan.ArrivalTime },
                { "route", flightPlan.Route },
                { "remarks", flightPlan.Remarks },
                { "fuel_hours", flightPlan.FuelHours },
                { "fuel_minutes", flightPlan.FuelMinutes },
                { "number_onboard", flightPlan.NumberOnBoard }
            };

            try
            {
                await collection.InsertOneAsync(document);
                if (document["_id"].IsObjectId)
                {
                    return TransactionResult.Success;
                }

                return TransactionResult.BadRequest;
            }
            catch (Exception)
            {
                return TransactionResult.ServerError;
            }
        }

        public async Task<TransactionResult> UpdateById(string id, FlightPlan data)
        {
            var collection = GetCollection("local", "flightplans");
            var filter = Builders<BsonDocument>.Filter.Eq("flight_plan_id", id);
            var update = Builders<BsonDocument>.Update
                .Set("altitude", data.Altitude)
                .Set("airspeed", data.Airspeed)
                .Set("aircraft_identification", data.AircraftId)
                .Set("aircraft_type", data.AircraftType)
                .Set("arrival_airport", data.ArrivalAirport)
                .Set("flight_type", data.FlightType)
                .Set("departing_airport", data.DepartureAirport)
                .Set("depature_time", data.DepartureTime)
                .Set("estimated_arrival_time", data.ArrivalTime)
                .Set("route", data.Route)
                .Set("remarks", data.Remarks)
                .Set("fuel_hours", data.FuelHours)
                .Set("fuel_minutes", data.FuelMinutes)
                .Set("numberOnBoard", data.NumberOnBoard);

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0 ? TransactionResult.Success :
                result.ModifiedCount == 0 ? TransactionResult.NotFound :
                TransactionResult.ServerError;
        }

        public async Task<bool> DeleteById(string id)
        {
            var collection = GetCollection("local", "flightplans");
            var result = await collection.DeleteOneAsync(
                Builders<BsonDocument>.Filter.Eq("flight_plan_id", id));

            return result.DeletedCount > 0;
        }

        private IMongoCollection<BsonDocument> GetCollection(string databaseName, string collectionName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        private FlightPlan? ConvertBsonToFlightPlan(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new FlightPlan
            {
                Id = document["flight_plan_id"].AsString,
                Altitude = document["altitude"].AsInt32,
                Airspeed = document["airspeed"].AsInt32,
                AircraftId = document["aircraft_identification"].AsString,
                AircraftType = document["aircraft_type"].AsString,
                ArrivalAirport = document["arrival_airport"].AsString,
                FlightType = document["flight_type"].AsString,
                DepartureAirport = document["departing_airport"].AsString,
                DepartureTime = DateTime.Parse(document["departure_time"]["$date"].AsBsonValue.AsString),
                ArrivalTime = DateTime.Parse(document["estimated_arrival_time"]["$date"].AsBsonValue.AsString),
                Route = document["route"].AsString,
                Remarks = document["remarks"].AsString,
                FuelHours = document["fuel_hours"].AsInt32,
                FuelMinutes = document["fuel_minutes"].AsInt32,
                NumberOnBoard = document["number_onboard"].AsInt32
            };
        }
    }
}
