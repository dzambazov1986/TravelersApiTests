using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection.Metadata;

namespace ApiTests
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;
        private string createdCategoryId;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category

            var createRequest = new RestRequest("category", Method.Post); 
            createRequest.AddHeader("Authorization", $"Bearer {token}"); 
            createRequest.AddJsonBody(new { name = "Test Category" }); 
            var createResponse = client.Execute(createRequest); 
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK)); 
            var createContent = JObject.Parse(createResponse.Content); 
            createdCategoryId = createContent["_id"].ToString(); 
            Assert.That(createdCategoryId, Is.Not.Null.Or.Empty, "Category ID should not be null or empty"); 

            // Step 2: Get all categories

            var getAllRequest = new RestRequest("category", Method.Get); 
            getAllRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var getAllResponse = client.Execute(getAllRequest); 
            Assert.That(getAllResponse.StatusCode, 
                Is.EqualTo(HttpStatusCode.OK)); 
            Assert.That(getAllResponse.Content, Is.Not.Empty); 
            var getAllContent = JArray.Parse(getAllResponse.Content); 
            Assert.That(getAllContent, Is.InstanceOf<JArray>());
            Assert.That(getAllContent.Count, Is.GreaterThanOrEqualTo(1));

            // Step 3: Get category by ID

            var getByIdRequest = new RestRequest($"category/{createdCategoryId}", Method.Get); 
            getByIdRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var getByIdResponse = client.Execute(getByIdRequest); 
            Assert.That(getByIdResponse.StatusCode, 
            Is.EqualTo(HttpStatusCode.OK)); 
            Assert.That(getByIdResponse.Content, Is.Not.Empty); 
            var getByIdContent = JObject.Parse(getByIdResponse.Content); 
            Assert.That(getByIdContent["_id"].ToString(), Is.EqualTo(createdCategoryId)); 
            Assert.That(getByIdContent["name"].ToString(), Is.EqualTo("Test Category")); 

            // Step 4: Edit the category

            var editRequest = new RestRequest($"category/{createdCategoryId}", Method.Put); 
            editRequest.AddHeader("Authorization", $"Bearer {token}"); 
            editRequest.AddJsonBody(new { name = "Updated Test Category" }); 
            var editResponse = client.Execute(editRequest); 
            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK)); 

                // Step 5: Verify Edit

            var verifyEditRequest = new RestRequest($"category/{createdCategoryId}", Method.Get); 
            verifyEditRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var verifyEditResponse = client.Execute(verifyEditRequest); 
            Assert.That(verifyEditResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK)); 
            Assert.That(verifyEditResponse.Content, Is.Not.Empty); 
            var verifyEditContent = JObject.Parse(verifyEditResponse.Content); 
            Assert.That(verifyEditContent["name"].ToString(), Is.EqualTo("Updated Test Category")); 

            // Step 6: Delete the category

            var deleteRequest = new RestRequest($"category/{createdCategoryId}", Method.Delete); 
            deleteRequest.AddHeader("Authorization", $"Bearer {token}"); 
            var deleteResponse = client.Execute(deleteRequest); 
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Step 7: Verify that the deleted category cannot be found

            var verifyDeleteRequest = new RestRequest($"category/{createdCategoryId}", Method.Get);
            verifyDeleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var verifyDeleteResponse = client.Execute(verifyDeleteRequest);
            Assert.That(verifyDeleteResponse.Content, Is.EqualTo("null").Or.Null, "Expected response content to be null or \"null\"");
     
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
