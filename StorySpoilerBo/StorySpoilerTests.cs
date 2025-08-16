using NUnit.Framework.Internal;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerBo.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoilerBo
{
    public class StorySpoilerBoTests
    {
        private static RestClient? _client;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private const string userName = "bospoinerstory123";
        private const string password = "987654321";
        private static string _token;

        private static string? _storyId;

        [SetUp]
        public void Setup()
        {
            _client = new RestClient(BaseUrl);
          
            _token = GetAuthToken();

            if (string.IsNullOrEmpty(_token))
            {
                throw new Exception("Authentication failed, token is null or empty.");
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(_token)
            };

            _client = new RestClient(options);
        }
        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [Test, Order(1)]
        public void Create_StorySpoiler_with_RequiredFields()
        {
            //Create a test to send a POST request to add a new story.
            //Assert that the response status code is Created(201).
            //Assert that the StoryId is returned in the response.
            //Assert that the response message indicates the story was "Successfully created!".
            //Store the StoryId as a static member of the static member of the test class to maintain its value between test runs

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(new StoryDTO
            {
                Title = "Test Story",
                Description = "Description2234",
                Url = ""
            });
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
            Assert.That(json.Msg, Is.EqualTo("Successfully created!"));
            Assert.That(json.StoryId, Is.Not.Null);
            _storyId = json.StoryId;
        }
        //Create a test that sends a PUT request to edit the story using the StoryId from the story creation test as a path variable.
        //Assert that the response status code is OK(200).
        //Assert that the response message indicates the story was "Successfully edited".

        [Test, Order(2)]
        public void Edit_Story_Spoiler()
        {
            var request = new RestRequest($"/api/Story/Edit/{_storyId}", Method.Put);
            request.AddJsonBody(new StoryDTO
            {
                Title = "Test Story Edited",
                Description = "Description2234",
                Url = ""
            });
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
        }

        //Create a test to send a GET request to list all stories.
        //Assert that the response status code is OK(200).
        //Assert that the response contains a non - empty array.

        [Test, Order(3)]
        public void GetAll_StorySpoilers ()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(json.GetArrayLength(), Is.GreaterThan(0));
        }

        //Create test that sends a DELETE request using the StoryId from the created story.
        //Assert that the response status code is OK(200).
        //Assert that the response message is "Deleted successfully!".

        [Test, Order(4)]
        public void Delete_StorySpoiler() 
        {
            var request = new RestRequest($"/api/Story/Delete/{_storyId}", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
            Assert.That(json.Msg, Is.EqualTo("Deleted successfully!"));
        }

        //Write a test that attempts to create a story with missing required fields (Title, Description).
        //Send the POST request with the incomplete data.
        //Assert that the response status code is BadRequest (400).

        [Test, Order(5)]
        public void StorySpoiler_Without_RequiredFields()
        {
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(new
            {
                Title = string.Empty,
                Description = string.Empty
            });
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        //Write a test to send a PUT request to edit a story with a StoryId that does not exist.
        //Assert that the response status code is NotFound(404).
        //Assert that the response message indicates "No spoilers...".

        [Test, Order(6)]
        public void Edit_Non_existingStorySpoiler()
        {
            var nonExistingStoryId = "non-existing-id";
            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(new StoryDTO
            {
                Title = "Test Story with non existing Id",
                Description = "Description",
                Url = ""
            });
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
            Assert.That(json.Msg, Is.EqualTo("No spoilers..."));
        }

        //Write a test to send a DELETE request to edit a story with a StoryId that does not exist.
        //Assert that the response status code is Bad request(400).
        //Assert that the response message indicates "Unable to delete this story spoiler!".

        [Test, Order(7)]
        public void Delete_NonExistingStorySpoiler()
        {
            var nonExistingStoryId = "non-existing-id";
            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var content = response.Content ?? string.Empty;
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(content);
            Assert.That(json.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }

        private static string? GetAuthToken()
        {
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });
            var response = _client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Content))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                if (json.TryGetProperty("accessToken", out var tokenElement))
                {
                    return tokenElement.GetString();
                }
            }

            return null;
        }
    }
}