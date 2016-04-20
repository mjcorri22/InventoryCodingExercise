using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Script.Serialization;

namespace InventoryWebService
{
    public class GlobalVars
    {
        public const string NAME = "Name";
        public const string LABEL = "Label";
        public const string TYPE = "Type";
        public const string QUANTITY = "Quantity";
        public const string SIZE = "Size";
        public const string ID = "ID";
        public const string AVAILABLE = "Available";
        public const string EXPIRATION_DATE = "ExpirationDate";
        public const string ACTION = "Action";
        public const string JSON = "JSON";
        public const string EMAIL = "Email";
    }
    public class InventoryDB
    {
        private static Dictionary<string, InventoryItem> _myDb = new Dictionary<string, InventoryItem>();
        private static JavaScriptSerializer _jss = new JavaScriptSerializer();
        private static string _email = string.Empty;

        public InventoryDB()
        {
        }

        public string ParsePostMsg(string json)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                Dictionary<string, object> jsonObj = _jss.DeserializeObject(json) as Dictionary<string, object>;
                string action = jsonObj.ContainsKey(GlobalVars.ACTION) ? jsonObj[GlobalVars.ACTION].ToString() : string.Empty;
                Dictionary<string, object> msgObj = jsonObj.ContainsKey(GlobalVars.JSON) ? jsonObj[GlobalVars.JSON] as Dictionary<string, object> : new Dictionary<string, object>();

                switch (action)
                {
                    case "AddItem":
                        return AddItem(msgObj);
                    case "RegisterEmail":
                        return RegisterEmail(msgObj);
                    case "CheckInventory":
                        return CheckInventory();
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Add("Error", "Failed to parse message: " + json);
                result.Add("Message", ex.Message);
                return _jss.Serialize(result);
            }

            result.Add("Error", "Failed to parse message: " + json);
            return _jss.Serialize(result);
        }

        /// <summary>
        /// Register the email of the user who gets sent the notifications that an item in the inventory has expired.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string RegisterEmail(Dictionary<string, object> jsonObj)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                // Parse the message for the email.
                _email = jsonObj.ContainsKey(GlobalVars.EMAIL) ? jsonObj[GlobalVars.EMAIL].ToString() : string.Empty;
                result.Add("Result", "Successfully added email " + _email);
                return _jss.Serialize(result);
            }
            catch (Exception e)
            {
                result.Add("Error", "Failed to parse RegisterEmail message: " + _jss.Serialize(jsonObj));
                result.Add("Message", e.Message);
                return _jss.Serialize(result);
            }
        }

        /// <summary>
        /// Itterate through current inventory and compile an email with the items that have expired.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CheckInventory()
        {
            List<string> items = new List<string>();

            foreach (var pair in _myDb)
            {
                // If item is still said to be available and has expired then add the item to the list so a notification can be sent out.
                if (pair.Value.Available && pair.Value.ExpirationDate.CompareTo(DateTime.Today) < 0)
                {
                    items.Add(ConvertItemToJson(pair.Value));
                    pair.Value.Available = false;
                }
            }

            if (items.Count > 0)
            {
                // Found items, send an email then.
                // Could spend more time to make message in a nicer format but skipping due to time constraints.
                string message = string.Format("The following items have expired:\r\n{0}", string.Join("\r\n", items));
                SendEmail(_email, _email,"Expired items in inventory", message);
                return string.Format("Found {0} expired item(s).", items.Count); ;
            }
            return "Did not find any expired items.";
        }

        #region Adding item to DB.
        /// <summary>
        /// Takes the json string and parses to create an inventory item then adds the item to the in memory database.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string AddItem(Dictionary<string, object> jsonObj)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string id = "";
            try
            {
                id = (_myDb.Count + 1).ToString();

                InventoryItem ii = new InventoryItem();

                ii.Label = jsonObj.ContainsKey(GlobalVars.LABEL) ? jsonObj[GlobalVars.LABEL].ToString() : string.Empty;
                ii.Name = jsonObj.ContainsKey(GlobalVars.NAME) ? jsonObj[GlobalVars.NAME].ToString() : string.Empty;
                ii.ExpirationDate = jsonObj.ContainsKey(GlobalVars.EXPIRATION_DATE) ? DateTime.Parse(jsonObj[GlobalVars.EXPIRATION_DATE].ToString())  : DateTime.Now.AddDays(7.0);
                ii.Quantity = jsonObj.ContainsKey(GlobalVars.QUANTITY) ? jsonObj[GlobalVars.QUANTITY].ToString() : string.Empty;
                ii.Size = jsonObj.ContainsKey(GlobalVars.SIZE) ? jsonObj[GlobalVars.SIZE].ToString() : string.Empty;
                ii.Type = jsonObj.ContainsKey(GlobalVars.TYPE) ? jsonObj[GlobalVars.TYPE].ToString() : string.Empty;
                ii.Available = true;
                ii.ID = id;

                _myDb.Add(id, ii);
            }
            catch (Exception ex)
            {
                result.Add("Error", "Failed to add item: " + _jss.Serialize(jsonObj));
                result.Add("Message", ex.Message);
                return _jss.Serialize(result);
            }

            result.Add("ID", id);
            return _jss.Serialize(result);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Currently not using this feature but goal would be to expand the functionality and allow users to make Get requests to look at
        /// what is available in the inventory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string LookAtItemItem(string id)
        {
            if (_myDb.ContainsKey(id))
                return ConvertItemToJson(_myDb[id]);

            Dictionary<string, object> error = new Dictionary<string, object>();
            error.Add("ErrorMessage", string.Format("Could not find item with specified ID: {0}", id));

            return _jss.Serialize(error);
        }

        /// <summary>
        /// A method originally used for development and initial testing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CheckOutItem(string id)
        {
            // Check to see if item exists and is available.
            if (_myDb.ContainsKey(id) && _myDb[id].AvailabilityStatus)
            {
                _myDb[id].Available = false; // Mark the item as being no longer available and return the information on the item.
                string mainbody = string.Format("Item {0} has been checked out.", _myDb[id].Label);
                SendEmail(_email, _email, "Item checked out of inventory", mainbody);
                return ConvertItemToJson(_myDb[id]);
            }

            Dictionary<string, object> error = new Dictionary<string,object>();
            error.Add("ErrorMessage", string.Format("Could not find item with specified ID: {0}", id));

            return _jss.Serialize(error);
        }

        /// <summary>
        /// Helper method that converts the InventoryItem object into a JSON string.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string ConvertItemToJson(InventoryItem item)
        {
            if (item == null)
                return string.Empty;

            Dictionary<string, object> json = new Dictionary<string, object>();

            json.Add(GlobalVars.NAME, item.Name);
            json.Add(GlobalVars.LABEL, item.Label);
            json.Add(GlobalVars.TYPE, item.Type);
            json.Add(GlobalVars.QUANTITY, item.Quantity);
            json.Add(GlobalVars.SIZE, item.Size);
            json.Add(GlobalVars.AVAILABLE, item.Available);
            json.Add(GlobalVars.EXPIRATION_DATE, item.ExpirationDate.ToShortDateString());
            json.Add(GlobalVars.ID, item.ID);

            return _jss.Serialize(json);
        }

        /// <summary>
        /// A method that was used in testing and could see it being useful in future API.
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string GetAllItemsByLabel(string label)
        {
            List<string> output = new List<string>();

            // Find all items in the DP that match the label.
            foreach (var pair in _myDb)
            {
                if (pair.Value.Label == label)
                    output.Add(ConvertItemToJson(pair.Value));
            }

            if (output.Count > 0)
            {
                // If at least one item was found, return the list of the results.
                return _jss.Serialize(output);
            }
            else
            {
                // No items were found.
                Dictionary<string, object> error = new Dictionary<string, object>();
                error.Add("ErrorMessage", string.Format("Could not find items with specified label: {0}", label));

                return _jss.Serialize(error);
            }
        }

#endregion


        /// <summary>
        /// Finds the first available item with a matching label. Returns an ErrorMessage if no item could be found or no items were available
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string CheckOutItemByLabel(string label)
        {
            Dictionary<string, object> error = new Dictionary<string, object>();

            foreach (var pair in _myDb)
            {
                if (pair.Value.Label == label && pair.Value.AvailabilityStatus)
                {
                    // Check out the item and send email to user.
                    return CheckOutItem(pair.Key);
                }                
            }
            
            error.Add("ErrorMessage", string.Format("Could not check out item with specified label: {0}", label));

            return _jss.Serialize(error);
        }

        /// <summary>
        /// Allowing users to return an item to inventory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string ReturnItemToInventory(string id)
        {
            if (_myDb.ContainsKey(id))
            {
                _myDb[id].Available = true;
                return ConvertItemToJson(_myDb[id]);
            }

            Dictionary<string, object> error = new Dictionary<string, object>();
            error.Add("ErrorMessage", string.Format("Could not return item to inventory: {0}", id));

            return _jss.Serialize(error);
        }

        private void SendEmail(string from, string to, string subject, string body)
        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            // 
            smtpServer.Credentials = new System.Net.NetworkCredential("UserName", "Password123");
            smtpServer.Port = 587; // Gmail works on this port

            mail.From = new MailAddress(from);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;

            smtpServer.Send(mail);

        }
    }
}
