using Newtonsoft.Json.Linq;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class DestinationTests : IDisposable
    {
        private RestClient client;
        private string token;
        private object destinationId;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_GetAllDestinations()
        {
            var request = new RestRequest("destination", Method.Get); 
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            Console.WriteLine("Request URL: " + client.BuildUri(request)); 
            Console.WriteLine("Response Content: " + response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK, but got {response.StatusCode}. Response content: {response.Content}"); 
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            var jsonResponse = JArray.Parse(response.Content); 
            Assert.That(jsonResponse, Is.TypeOf<JArray>(), "Response content should be a JSON array"); 
            Assert.That(jsonResponse.Count, Is.GreaterThan(0), "JSON array should contain at least one destination");

            foreach (var destination in jsonResponse) 
            { 
             Assert.That(destination["name"].ToString(), Is.Not.Null.And.Not.Empty, "Each destination's name should not be null or empty"); 
             Assert.That(destination["location"].ToString(), Is.Not.Null.And.Not.Empty, "Each destination's location should not be null or empty"); 
                Assert.That(destination["description"].ToString(), Is.Not.Null.Or.Empty, "Each destination's description should not be null or empty"); 
                Assert.That(destination["category"].ToString(), Is.Not.Null.And.Not.Empty, "Each destination's category should not be null or empty"); 
                Assert.That(destination["attractions"], Is.TypeOf<JArray>(), "Each destination’s attractions should be a JSON array"); 
                Assert.That(destination["bestTimeToVisit"].ToString(), Is.Not.Null.And.Not.Empty, "Each destination’s bestTimeToVisit should not be null or empty"); }
        }

        [Test]
        public void Test_GetDestinationByName()
        {
     
            string destinationName = "New York City";

            // Step 1: Get All Destinations
            var getAllDestinationsRequest = new RestRequest("destination", Method.Get);
            getAllDestinationsRequest.AddHeader("Authorization", $"Bearer {token}");
            var getAllDestinationsResponse = client.Execute(getAllDestinationsRequest);

            // Step 2: Response Assertions for Get All Destinations
            Assert.That(getAllDestinationsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {getAllDestinationsResponse.StatusCode}. Response content: {getAllDestinationsResponse.Content}");
            Assert.That(getAllDestinationsResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            // Step 3: Parse JSON Response
            var destinations = JArray.Parse(getAllDestinationsResponse.Content);

            // Step 4: Find Destination by Name
            var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == destinationName);
            Assert.That(destination, Is.Not.Null, "Destination with the name 'New York City' should exist in the response");

            // Step 5: Data Assertions
            Assert.That(destination["name"]?.ToString(), Is.EqualTo(destinationName), "Destination name should match the searched name");

            // Step 6: Destination Fields Assertions
            var location = destination["location"]?.ToString();
            var description = destination["description"]?.ToString();

            Assert.That(location, Is.EqualTo("New York, USA"), "Location of the destination should be 'New York, USA'");
            Assert.That(description, Is.EqualTo("The largest city in the USA, known for its skyscrapers, culture, and entertainment."),
                "Description of the destination should match the given description");
        }

        

        [Test]
        public void Test_AddDestination()
        {
            // Step 1: Get all categories
            var getCategoriesRequest = new RestRequest("category", Method.Get);
            getCategoriesRequest.AddHeader("Authorization", $"Bearer {token}");
            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            Assert.That(getCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {getCategoriesResponse.StatusCode}. Response content: {getCategoriesResponse.Content}");
            Assert.That(getCategoriesResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            var categories = JArray.Parse(getCategoriesResponse.Content);
            var category = categories.FirstOrDefault();
            Assert.That(category, Is.Not.Null, "At least one category should be returned");

            string categoryId = category["_id"]?.ToString();
            Assert.That(categoryId, Is.Not.Null.And.Not.Empty, "Category ID should not be null or empty");

            // Step 2: Create a new Destination
            var newDestination = new
            {
                name = "Sample Destination",
                location = "Sample Location",
                description = "Sample Description",
                bestTimeToVisit = "Sample Best Time",
                attractions = new string[] { "Attraction 1", "Attraction 2" },
                category = categoryId
            };

            var addDestinationRequest = new RestRequest("destination", Method.Post);
            addDestinationRequest.AddHeader("Authorization", $"Bearer {token}");
            addDestinationRequest.AddJsonBody(newDestination);
            var addDestinationResponse = client.Execute(addDestinationRequest);

            // Step 3: Response Assertions for Add Destination
            Assert.That(addDestinationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {addDestinationResponse.StatusCode}. Response content: {addDestinationResponse.Content}");
            Assert.That(addDestinationResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            var createdDestination = JObject.Parse(addDestinationResponse.Content);
            string createdDestinationId = createdDestination["_id"]?.ToString();
            Assert.That(createdDestinationId, Is.Not.Null.And.Not.Empty, "Created destination ID should not be null or empty");

            // Step 5: Get the details of the created Destination
            var getDestinationRequest = new RestRequest($"destination/{createdDestinationId}", Method.Get);
            getDestinationRequest.AddHeader("Authorization", $"Bearer {token}");
            var getDestinationResponse = client.Execute(getDestinationRequest);

            // Step 6: Response Assertions for Get Destination
            Assert.That(getDestinationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {getDestinationResponse.StatusCode}. Response content: {getDestinationResponse.Content}");
            Assert.That(getDestinationResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            var retrievedDestination = JObject.Parse(getDestinationResponse.Content);

            // Step 7: Destination Fields Assertions
            Assert.That(retrievedDestination["name"]?.ToString(), Is.EqualTo(newDestination.name), "Destination name should match the input value");
            Assert.That(retrievedDestination["location"]?.ToString(), Is.EqualTo(newDestination.location), "Destination location should match the input value");
            Assert.That(retrievedDestination["description"]?.ToString(), Is.EqualTo(newDestination.description), "Destination description should match the input value");
            Assert.That(retrievedDestination["bestTimeToVisit"]?.ToString(), Is.EqualTo(newDestination.bestTimeToVisit), "Destination bestTimeToVisit should match the input value");

            var retrievedCategory = retrievedDestination["category"];
            Assert.That(retrievedCategory, Is.Not.Null, "Category should not be empty");
            Assert.That(retrievedCategory["_id"]?.ToString(), Is.EqualTo(categoryId), "Category ID should match the input value");

            var retrievedAttractions = retrievedDestination["attractions"] as JArray;
            Assert.That(retrievedAttractions, Is.Not.Null, "Attractions should be a JSON array");
            Assert.That(retrievedAttractions.Count, Is.EqualTo(newDestination.attractions.Length), "Attractions array should have the same number of elements as the input value");
            for (int i = 0; i < retrievedAttractions.Count; i++)
            {
                Assert.That(retrievedAttractions[i].ToString(), Is.EqualTo(newDestination.attractions[i]), "Attractions values should match the input values");
            }

        }

        [Test]
        public void Test_UpdateDestination()
        {
   
            // Step 1: Get All Destinations
            var getAllDestinationsRequest = new RestRequest("destination", Method.Get);
            getAllDestinationsRequest.AddHeader("Authorization", $"Bearer {token}");
            var getAllDestinationsResponse = client.Execute(getAllDestinationsRequest);

            // Step 2: Get Request Assertions
            Assert.That(getAllDestinationsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {getAllDestinationsResponse.StatusCode}. Response content: {getAllDestinationsResponse.Content}");
            Assert.That(getAllDestinationsResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            // Step 3: Parse JSON Response
            var destinations = JArray.Parse(getAllDestinationsResponse.Content);

            // Step 4: Find Destination by Name
            var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Machu Picchu");
            Assert.That(destination, Is.Not.Null, "Destination with the name 'Machu Picchu' should exist in the response");

            string? destinationId = destination["_id"]?.ToString();
            Assert.That(destinationId, Is.Not.Null.And.Not.Empty, "Destination ID should not be null or empty");

            // Step 5: Update the Destination
            var updatedDestination = new
            {
                name = "Machu Picchu - Updated",
                bestTimeToVisit = "Updated Best Time"
            };

            var updateDestinationRequest = new RestRequest($"destination/{destinationId}", Method.Put);
            updateDestinationRequest.AddHeader("Authorization", $"Bearer {token}");
            updateDestinationRequest.AddJsonBody(updatedDestination);
            var updateDestinationResponse = client.Execute(updateDestinationRequest);

            // Step 6: Update Response Assertions
            Assert.That(updateDestinationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {updateDestinationResponse.StatusCode}. Response content: {updateDestinationResponse.Content}");
            Assert.That(updateDestinationResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            // Step 7: Updated Fields Assertions
            var updatedDestinationResponse = JObject.Parse(updateDestinationResponse.Content);

            Assert.That(updatedDestinationResponse["name"]?.ToString(), Is.EqualTo(updatedDestination.name), "Updated name should match the input value");
            Assert.That(updatedDestinationResponse["bestTimeToVisit"]?.ToString(), Is.EqualTo(updatedDestination.bestTimeToVisit), "Updated bestTimeToVisit should match the input value");
        }



        [Test]
        public void Test_DeleteDestination()
        {
        
            // Step 1: Get All Destinations
            var getAllDestinationsRequest = new RestRequest("destination", Method.Get);
            getAllDestinationsRequest.AddHeader("Authorization", $"Bearer {token}");
            var getAllDestinationsResponse = client.Execute(getAllDestinationsRequest);

            // Step 2: Get Request Assertions
            Assert.That(getAllDestinationsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {getAllDestinationsResponse.StatusCode}. Response content: {getAllDestinationsResponse.Content}");
            Assert.That(getAllDestinationsResponse.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty");

            // Step 3: Parse JSON Response
            var destinations = JArray.Parse(getAllDestinationsResponse.Content);

            // Step 4: Find Destination by Name
            var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Yellowstone National Park");
            Assert.That(destination, Is.Not.Null, "Destination with the name 'Yellowstone National Park' should exist in the response");

            string? destinationId = destination["_id"]?.ToString();
            Assert.That(destinationId, Is.Not.Null.And.Not.Empty, "Destination ID should not be null or empty");

            // Step 5: Delete the Destination
            var deleteDestinationRequest = new RestRequest($"destination/{destinationId}", Method.Delete);
            deleteDestinationRequest.AddHeader("Authorization", $"Bearer {token}");
            var deleteDestinationResponse = client.Execute(deleteDestinationRequest);

            // Step 6: Delete Response Assertions
            Assert.That(deleteDestinationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {deleteDestinationResponse.StatusCode}. Response content: {deleteDestinationResponse.Content}");

            // Step 7: Verification Assertions
            var verifyDeleteRequest = new RestRequest($"destination/{destinationId}", Method.Get);
            verifyDeleteRequest.AddHeader("Authorization", $"Bearer {token}");
            var verifyDeleteResponse = client.Execute(verifyDeleteRequest);

            Assert.That(verifyDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected status code 200 OK, but got {verifyDeleteResponse.StatusCode}. Response content: {verifyDeleteResponse.Content}");
            Assert.That(verifyDeleteResponse.Content, Is.EqualTo("null"), "Response content for the deleted destination should be 'null'");
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
