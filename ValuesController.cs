using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InventoryWebService.Controllers
{
    public class ValuesController : ApiController
    {
        private static InventoryDB myDb = new InventoryDB();
        // GET inventoryApi/values?value=1
        public string Get([FromUri]string value)
        {
            return myDb.CheckOutItemByLabel(value);
        }

        // POST inventoryApi/values
        public string Post([FromBody]string value)
        {
            return myDb.ParsePostMsg(value);
        }

        // PUT inventoryApi/values?value=1
        public void Put([FromUri]string value)
        {
            // Put item that was checked out back.
            myDb.ReturnItemToInventory(value);
        }

    }
}