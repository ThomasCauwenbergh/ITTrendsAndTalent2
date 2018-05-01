using EvaluatieApp.Models;
using EvaluatieApp.Services;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EvaluatieApp.Controllers
{
    [Authorize(Roles = "EvaluatieManager")]
    public class EvaluatorController : Controller
    {
        private EvaluatieAppDBEntities db = new EvaluatieAppDBEntities();
        private string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string graphResourceID = "https://graph.microsoft.com";
        private GraphServices graphServices = new GraphServices();

        // GET: Evaluator
        public async Task<ActionResult> Index()
        {
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string groupId = "1cadfa28-f1bc-4958-af9c-4b3130555bc9";
            try
            {
                string accesstoken = await GetTokenForApplication();

                var result = await graphServices.GetUsersFromGroup(accesstoken);
                var users = result.users;

                return View(users);
            }
           
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception)
            {
                return View("Relogin");
            }

        }

        public async Task<ActionResult> EvaluationList(string id)
        {
            EvaluationListModel evaluationListModel = new EvaluationListModel();
            List<Item> items = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(a => a.AuthorId == id).ToList<Item>();
            string accesstoken = await GetTokenForApplication();
            evaluationListModel.user = await graphServices.GetUser(accesstoken, id);
            evaluationListModel.items = items;
            return View(evaluationListModel);
        }

        
        public ActionResult Rating_Changed(string userId)
        {

            return RedirectToAction("EvaluationList", "Evaluator", userId);
        }

        [HttpPost]
        public ActionResult Rating_Changed(string userId, int ItemId, double rating)
        {
            var result = db.Items.SingleOrDefault(i => i.Id == ItemId);
                if (result != null)
                {
                    result.Rating = rating;
                    db.SaveChanges();
                    
                }
            return RedirectToAction("EvaluationList", "Evaluator", userId);

        }

        // GET: Evaluator/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: Evaluator/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Evaluator/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Evaluator/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Evaluator/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Evaluator/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Evaluator/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public void RefreshSession()
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties { RedirectUri = "/UserProfile" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public async Task<string> GetTokenForApplication()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential clientcred = new ClientCredential(clientId, appKey);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            AuthenticationContext authenticationContext = new AuthenticationContext(aadInstance + tenantID, new ADALTokenCache(signedInUserID));
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return authenticationResult.AccessToken;
        }
    }
}
