using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using Hotel.Check_In.Management.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
namespace Hotel.Check_In.Management.Dal
{
  
    public class HotelCheckInDataAccessLayer
    {
        SqlConnection cn = new SqlConnection(@WebConfigurationManager.AppSettings["ConnectionString"]);
        private DocumentClient client;
        public List<string> HomeDetails()
        {

            List<string> HDetails = new List<string>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("HomeDetailsScreen", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cn.Close();
            if (dt.Rows.Count > 0)
            {
                HDetails.Add(dt.Rows[0]["AvailableRooms"].ToString());
                HDetails.Add(dt.Rows[0]["OccupiedRooms"].ToString());
                HDetails.Add(dt.Rows[0]["ReservedRooms"].ToString());
                HDetails.Add(dt.Rows[0]["CheckIns"].ToString());
                HDetails.Add(dt.Rows[0]["CheckOuts"].ToString());
            }
            return HDetails;

        }

        public Dictionary<string, List<Rooms>> ViewRooms()
        {
            List<Rooms> Arooms = new List<Rooms>();
            List<Rooms> Orooms = new List<Rooms>();
            List<Rooms> Rrooms = new List<Rooms>();

            Dictionary<string, List<Rooms>> VRooms = new Dictionary<string, List<Rooms>>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("ViewRooms", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            cn.Close();
            if (ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Rooms obj = new Rooms();
                    obj.RoomId = int.Parse(row["RoomId"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.RoomType = row["RoomType"].ToString();
                    obj.Price = int.Parse(row["Price"].ToString());
                    Arooms.Add(obj);
                }

            }
            if (ds.Tables[1].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Rooms obj = new Rooms();
                    obj.RoomId = int.Parse(row["RoomId"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.RoomType = row["RoomType"].ToString();
                    obj.Price = int.Parse(row["Price"].ToString());
                    Orooms.Add(obj);
                }

            }
            if (ds.Tables[2].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[2].Rows)
                {
                    Rooms obj = new Rooms();
                    obj.RoomId = int.Parse(row["RoomId"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.RoomType = row["RoomType"].ToString();
                    obj.Price = int.Parse(row["Price"].ToString());
                    Rrooms.Add(obj);
                }
            }

            VRooms.Add("AvailableRooms", Arooms);
            VRooms.Add("OccupiedRooms", Orooms);
            VRooms.Add("ReservedRooms", Rrooms);


            return VRooms;

        }

        public int RoomBooking(Booking details)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("RoomBooking", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RoomNo", details.RoomNo);
            cmd.Parameters.AddWithValue("@CheckInDate", details.CheckInDate);
            cmd.Parameters.AddWithValue("@CheckOutDate", details.CheckOutDate);
            cmd.Parameters.AddWithValue("@CheckInStatus", details.CheckInStatus);
            cmd.Parameters.AddWithValue("@PaymentStatus", details.PaymentStatus);
            cmd.Parameters.AddWithValue("@AdvancePaid", details.AdvancePaid);
            cmd.Parameters.AddWithValue("@TotalAmountPaid", details.TotalAmountPaid);
            cmd.Parameters.AddWithValue("@TotalAmountToBePaid", details.TotalAmountToBePaid);
            cmd.Parameters.AddWithValue("@Name", details.Name);
            cmd.Parameters.AddWithValue("@Address", details.Address);
            cmd.Parameters.AddWithValue("@Gender", details.Gender);
            cmd.Parameters.AddWithValue("@IdProof", details.IdProof);
            int result = cmd.ExecuteNonQuery();
            cn.Close();
            return result;
        }

        public int UpdateRoomBooking(Booking details)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("UpdateRoomBooking", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CheckInId", details.CheckInId);
            cmd.Parameters.AddWithValue("@CustomerId", details.CustomerId);
            cmd.Parameters.AddWithValue("@CheckOutDate", details.CheckOutDate);
            cmd.Parameters.AddWithValue("@CheckInStatus", details.CheckInStatus);
            cmd.Parameters.AddWithValue("@PaymentStatus", details.PaymentStatus);
            cmd.Parameters.AddWithValue("@AdvancePaid", details.AdvancePaid);
            cmd.Parameters.AddWithValue("@TotalAmountPaid", details.TotalAmountPaid);
            cmd.Parameters.AddWithValue("@TotalAmountToBePaid", details.TotalAmountToBePaid);
            cmd.Parameters.AddWithValue("@Name", details.Name);
            cmd.Parameters.AddWithValue("@Address", details.Address);
            int result = cmd.ExecuteNonQuery();
            cn.Close();
            return result;
        }

        public int DeleteRoomBooking(int CheckInId, int CustomerId)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("DeleteRoomBooking", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CheckInId", CheckInId);
            cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            int result = cmd.ExecuteNonQuery();
            cn.Close();
            return result;
        }
        public int InsertRooms(Rooms details)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("InsertRooms", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RoomNo", details.RoomNo);
            cmd.Parameters.AddWithValue("@RoomType", details.RoomType);
            cmd.Parameters.AddWithValue("@Price", details.Price);
            int result = cmd.ExecuteNonQuery();
            cn.Close();
            return result;
        }

        /*Adding rooms data to Azure Cosmos Database*/
        public int CosmsoInsertRooms(Rooms details)
        {
            string EndPointUrl = WebConfigurationManager.AppSettings["EndPointUrl"];
            String PrimaryKey = WebConfigurationManager.AppSettings["PrimaryKey"];
            client = new DocumentClient(new Uri(EndPointUrl), PrimaryKey);
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = "project" });
            client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("project"),
            new DocumentCollection { Id = "Rooms" });
            client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("project", "Rooms"), details);
            return 1;
        }
        public int UpdateRooms(Rooms details)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("UpdateRooms", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RoomNo", details.RoomNo);
            cmd.Parameters.AddWithValue("@RoomType", details.RoomType);
            cmd.Parameters.AddWithValue("@Price", details.Price);
            int result = cmd.ExecuteNonQuery();
            return result;
        }
        public int DeleteRooms(int? RoomId)
        {
            cn.Open();
            SqlCommand cmd = new SqlCommand("DeleteRooms", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RoomId", RoomId);
            int result = cmd.ExecuteNonQuery();
            cn.Close();
            return result;
        }

        public List<Rooms> GetRoomsList()
        {
            List<Rooms> GRooms = new List<Rooms>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("GetRoomsList", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cn.Close();
            if (dt.Rows.Count > 0)
            {

                foreach (DataRow row in dt.Rows)
                {
                    Rooms obj = new Rooms();
                    obj.RoomId = int.Parse(row["RoomId"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.RoomType = row["RoomType"].ToString();
                    obj.Price = int.Parse(row["Price"].ToString());
                    GRooms.Add(obj);
                }

            }
            return GRooms;

        }

        public List<Booking> ViewRoomBookings()
        {
            List<Booking> VRBookings = new List<Booking>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("ViewRoomBookings", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cn.Close();
            if (dt.Rows.Count > 0)
            {

                foreach (DataRow row in dt.Rows)
                {
                    Booking obj = new Booking();
                    obj.CheckInId = int.Parse(row["CheckInId"].ToString());
                    obj.CustomerId = int.Parse(row["CustomerId"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.CheckInDate = DateTime.Parse(row["CheckInDate"].ToString());
                    obj.CheckOutDate = DateTime.Parse(row["CheckOutDate"].ToString());
                    obj.CheckInStatus = row["CheckInStatus"].ToString();
                    obj.PaymentStatus = row["PaymentStatus"].ToString();
                    obj.AdvancePaid = int.Parse(row["AdvancePaid"].ToString());
                    obj.TotalAmountPaid = int.Parse(row["TotalAmountPaid"].ToString());
                    obj.TotalAmountToBePaid = int.Parse(row["TotalAmountToBePaid"].ToString());
                    obj.Name = row["Name"].ToString();
                    obj.Address = row["Address"].ToString();
                    obj.Gender = row["Gender"].ToString();
                    obj.IdProof = row["IdProof"].ToString();
                    VRBookings.Add(obj);
                }

            }
            return VRBookings;

        }

       
        public string UploadFile(HttpPostedFileBase photo)
        {
            string ImagePath = null;
            if (photo == null || photo.ContentLength == 0)
            {
                return null;
            }
            CloudStorageAccount CStorageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            CloudBlobClient BlobClient = CStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer BlobContainer = BlobClient.GetContainerReference("sayrcontainer");
            CloudBlockBlob BlockBlob = BlobContainer.GetBlockBlobReference(photo.FileName);
            using (photo.InputStream)
            {
                BlockBlob.UploadFromStream(photo.InputStream);
            }
            ImagePath = BlockBlob.Uri.ToString();
            return ImagePath;
        }

        public Dictionary<string, List<Booking>> ViewCheckIns()
        {
            List<Booking> OCheckIns = new List<Booking>();
            List<Booking> TCheckIns = new List<Booking>();
            Dictionary<string, List<Booking>> VCheckIns = new Dictionary<string, List<Booking>>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("ViewCheckIns", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            cn.Close();
            if (ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Booking obj = new Booking();
                    obj.Name = row["Name"].ToString();
                    obj.CheckInDate = DateTime.Parse(row["CheckInDate"].ToString());
                    obj.CheckOutDate = DateTime.Parse(row["CheckOutDate"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.TotalAmountPaid = int.Parse(row["TotalAmountPaid"].ToString());
                    obj.TotalAmountToBePaid = int.Parse(row["TotalAmountToBePaid"].ToString());

                    OCheckIns.Add(obj);
                }

            }
            if (ds.Tables[1].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Booking obj = new Booking();
                    obj.Name = row["Name"].ToString();
                    obj.CheckInDate = DateTime.Parse(row["CheckInDate"].ToString());
                    obj.CheckOutDate = DateTime.Parse(row["CheckOutDate"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.TotalAmountPaid = int.Parse(row["TotalAmountPaid"].ToString());
                    obj.TotalAmountToBePaid = int.Parse(row["TotalAmountToBePaid"].ToString());
                    TCheckIns.Add(obj);
                }

            }
            VCheckIns.Add("OverallCheckIns", OCheckIns);
            VCheckIns.Add("TodayCheckIns", TCheckIns);
            return VCheckIns;

        }

        public Dictionary<string, List<Booking>> ViewCheckOuts()
        {
            List<Booking> OCheckOuts = new List<Booking>();
            List<Booking> TCheckOuts = new List<Booking>();
            Dictionary<string, List<Booking>> VCheckOuts = new Dictionary<string, List<Booking>>();
            cn.Open();
            SqlCommand cmd = new SqlCommand("ViewCheckOuts", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            cn.Close();
            if (ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Booking obj = new Booking();
                    obj.Name = row["Name"].ToString();
                    obj.CheckInDate = DateTime.Parse(row["CheckInDate"].ToString());
                    obj.CheckOutDate = DateTime.Parse(row["CheckOutDate"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.TotalAmountPaid = int.Parse(row["TotalAmountPaid"].ToString());
                    obj.TotalAmountToBePaid = int.Parse(row["TotalAmountToBePaid"].ToString());

                    OCheckOuts.Add(obj);
                }

            }
            if (ds.Tables[1].Rows.Count > 0)
            {

                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Booking obj = new Booking();
                    obj.Name = row["Name"].ToString();
                    obj.CheckInDate = DateTime.Parse(row["CheckInDate"].ToString());
                    obj.CheckOutDate = DateTime.Parse(row["CheckOutDate"].ToString());
                    obj.RoomNo = int.Parse(row["RoomNo"].ToString());
                    obj.TotalAmountPaid = int.Parse(row["TotalAmountPaid"].ToString());
                    obj.TotalAmountToBePaid = int.Parse(row["TotalAmountToBePaid"].ToString());
                    TCheckOuts.Add(obj);
                }

            }
            VCheckOuts.Add("OverallCheckOuts", OCheckOuts);
            VCheckOuts.Add("TodayCheckOuts", TCheckOuts);
            return VCheckOuts;

        }
    }

}
