using EvaluatieApp.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EvaluatieApp.Services
{
    
    public class GraphServices
    {
        private string graphResourceID = "https://graph.microsoft.com";
        private HttpClient httpClient = new HttpClient();

        private async Task<T> GetjsonAsync<T>(string Resource, string accesstoken) where T : class
        {
            HttpResponseMessage response;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, graphResourceID + Resource);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accesstoken);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("The response was not succesfull");
                }

                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<UserListModel> GetUsersFromGroup(string accesstoken)
        {
            var response = await GetjsonAsync<UserListModel>("/v1.0/groups/1cadfa28-f1bc-4958-af9c-4b3130555bc9/members", accesstoken);
            return response;
        }

        public async Task<User> GetUser(string accesstoken, string id)
        {
            var response = await GetjsonAsync<User>("/v1.0/users/" + id, accesstoken);
            return response;
        }

    }
}