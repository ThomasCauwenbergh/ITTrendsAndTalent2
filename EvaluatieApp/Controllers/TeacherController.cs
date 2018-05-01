
using EvaluatieApp.Models;
using EvaluatieApp.ViewModels;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EvaluatieApp.Controllers
{


    [Authorize(Roles = "Docenten")]
    public class TeacherController : Controller
    {
        
        private string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string graphResourceID = "https://graph.windows.net";

        private EvaluatieAppDBEntities db = new EvaluatieAppDBEntities();
        public ActionResult Index(int year = 0, int month = 0)
        {
            if (year == 0 && month == 0)
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month;
            }
            ItemViewModel itemViewModel = new ItemViewModel();
            itemViewModel.Items1 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 1).ToList<Item>();
            itemViewModel.Items2 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 2).ToList<Item>();
            itemViewModel.Items3 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 3).ToList<Item>();
            itemViewModel.Items4 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 4).ToList<Item>();

            return View(itemViewModel);
        }

       

        [HttpPost]
        [ValidateInput(true)]
        public ActionResult UpdateItems(int year, int month)
        {
            ItemViewModel itemViewModel = new ItemViewModel();
            itemViewModel.Items1 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 1).ToList<Item>();
            itemViewModel.Items2 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 2).ToList<Item>();
            itemViewModel.Items3 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 3).ToList<Item>();
            itemViewModel.Items4 = db.Items.SqlQuery("SELECT * FROM dbo.Item").Where(d => d.CreatedOn.Value.Month == month && d.CreatedOn.Value.Year == year && d.Type == 4).ToList<Item>();
            itemViewModel.date = new DateTime(year, month, 1);
            return PartialView("paperView", itemViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Create(int type, DateTime date)
        {
            ViewBag.ItemType = type;
            ViewBag.Date = date;
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(true)]
        public async Task<ActionResult> Create([Bind(Include = "Title,Description")] Item item, int type, DateTime date)
        {
            if (ModelState.IsValid)
            {
                string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                try
                {
                    Uri servicePointUri = new Uri(graphResourceID);
                    Uri serviceRoot = new Uri(servicePointUri, tenantID);
                    ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                          async () => await GetTokenForApplication());

                    // use the token for querying the graph to get the user details

                    var result = await activeDirectoryClient.Users
                        .Where(u => u.ObjectId.Equals(userObjectID))
                        .ExecuteAsync();
                    IUser user = result.CurrentPage.ToList().First();

                    item.Type = type;
                    item.AuthorId = user.ObjectId;
                    item.CreatedBy = user.DisplayName;
                    item.CreatedOn = date;
                    item.Rating = 0;
                    db.Items.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (AdalException)
                {
                    // Return to error page.
                    return View("Error");
                }
                // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
                catch (Exception)
                {
                    return View("Relogin");
                }
                
            }

            return View(item);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,CreatedBy,AuthorId,CreatedOn,rating")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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