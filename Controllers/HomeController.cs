using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Hotel.Check_In.Management.Filters;
using Hotel.Check_In.Management.Models;
using Hotel.Check_In.Management.Dal;
using Hotel.Check_In.Management.ErrorHandler;
namespace Hotel.Check_In.Management.Controllers
{
    [AiHandleError]
    public class HomeController : Controller
    {
        HotelCheckInDataAccessLayer dal = new HotelCheckInDataAccessLayer();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            // Dictionary<string,string> Secrets =  Task.Run(() => data.Secrets()).Result;
            string UserName = "User";
            string Password = "User@123";
            if (form["username"] == UserName && form["password"] == Password)
            {
                string role = "admin";
                FormsAuthentication.SetAuthCookie("login", false);
                var ticket = new FormsAuthenticationTicket(1, "login", DateTime.Now, DateTime.Now.AddMinutes(12), false, role);
                var authticket = FormsAuthentication.Encrypt(ticket);
                var authcookie = new HttpCookie(FormsAuthentication.FormsCookieName, authticket);
                HttpContext.Response.Cookies.Add(authcookie);
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid Username/Password";
            }
            return View();

        }

        [CustomAuthorize(Roles = "admin")]
        public ActionResult Home()
        {
            
            List<string> Details = dal.HomeDetails().ToList();
            ViewBag.AvailableRooms = Details[0];
            ViewBag.OccupiedRooms = Details[1];
            ViewBag.ReservedRooms = Details[2];
            ViewBag.CheckIns = Details[3];
            ViewBag.CheckOuts = Details[4];
            Dictionary<string, List<Rooms>> Vrooms = dal.ViewRooms();
            ViewBag.AvailableRoomsList = Vrooms["AvailableRooms"];
            ViewBag.OccupiedRoomsList = Vrooms["OccupiedRooms"];
            ViewBag.ReservedRoomsList = Vrooms["ReservedRooms"];
            return View();
        }

        [CustomAuthorize(Roles = "admin")]
        public ActionResult AddBooking()
        {
            ViewBag.RoomNumbers = dal.GetRoomsList();
            ViewBag.TotalAmountToBePaid = "100";
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        [HttpPost]
        public ActionResult AddBooking(HttpPostedFileBase photo, Booking deatils)
        {
            deatils.IdProof = "";
            string ImagePath = dal.UploadFile(photo);
            int result = dal.RoomBooking(deatils);
            return RedirectToAction("ViewBookings");
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult ViewBookings()
        {
            ViewBag.ViewRoomBookings = dal.ViewRoomBookings();
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        [HttpPost]
        public ActionResult ViewBookings(string command, string CheckInId, string CustomerId)
        {

            if (command == "Edit")
            {
                TempData["CheckInId"] = CheckInId;
                TempData["CustomerId"] = CustomerId;
                return RedirectToAction("UpdateBookings");
            }
            else if (command == "Delete")
            {
                int result = dal.DeleteRoomBooking(int.Parse(CheckInId), int.Parse(CustomerId));
                return RedirectToAction("ViewBookings", "Home");
            }
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult UpdateBookings()
        {
            int CheckInId = int.Parse(TempData["CheckInId"].ToString());
            int CustomerId = int.Parse(TempData["CustomerId"].ToString());
            Booking details = (from BK in dal.ViewRoomBookings() where BK.CheckInId == CheckInId && BK.CustomerId == CustomerId select BK).FirstOrDefault();
            ViewBag.RoomNumbers = dal.GetRoomsList();
            return View(details);
        }
        [CustomAuthorize(Roles = "admin")]
        [HttpPost]
        public ActionResult UpdateBookings(string command, Booking details)
        {
            if (command == "Update")
            {
                int result = dal.UpdateRoomBooking(details);
            }
            return RedirectToAction("ViewBookings", "Home");
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult AddRooms()
        {
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        [HttpPost]
        public ActionResult AddRooms(Rooms roomdata)
        {
            int result = dal.InsertRooms(roomdata);
            Rooms Rdata = (from R in dal.GetRoomsList() orderby R.RoomId descending select R).FirstOrDefault();
            int CosmosResult = dal.CosmsoInsertRooms(Rdata);
            return RedirectToAction("ViewRooms", "Home");
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult ViewRooms()
        {
            ViewBag.RoomsList = dal.GetRoomsList();
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        [HttpPost]
        public ActionResult ViewRooms(string command, int? ViewRoomId, Rooms roomdata)
        {
            if (command == "Edit")
            {
                Rooms Rdata = (from R in dal.GetRoomsList() where R.RoomId == ViewRoomId select R).FirstOrDefault();
                ViewBag.RoomType = Rdata.RoomType;
                ViewBag.Price = Rdata.Price;
                ViewBag.RoomNo = Rdata.RoomNo;
                ViewBag.Edit = true;
                return View();
            }
            else if (command == "Update")
            {
                int result = dal.UpdateRooms(roomdata);
            }
            else if (command == "Delete")
            {
                int result = dal.DeleteRooms(ViewRoomId);
            }
            return RedirectToAction("ViewRooms", "Home"); ;
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult ViewCheckIns()
        {
            Dictionary<string, List<Booking>> VCheckIns = dal.ViewCheckIns();
            ViewBag.OverallCheckIns = VCheckIns["OverallCheckIns"];
            ViewBag.TodayCheckIns = VCheckIns["TodayCheckIns"];
            return View();
        }
        [CustomAuthorize(Roles = "admin")]
        public ActionResult ViewCheckOuts()
        {
            Dictionary<string, List<Booking>> VCheckOuts = dal.ViewCheckOuts();
            ViewBag.OverallCheckOuts = VCheckOuts["OverallCheckOuts"];
            ViewBag.TodayCheckOuts = VCheckOuts["TodayCheckOuts"];
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
