using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryWebService
{
    public class InventoryItem
    {

        private bool _available = true;

        public InventoryItem()
        {
        }

        /// <summary>
        /// Name of medication.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Common name?
        /// </summary>
        public string Label { get; set; }

        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Returns the type of medication.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Returns the amount in the container.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Returns the dosage size; 500 mg, 15 ml...
        /// </summary>
        public string Size { get; set; }

        public string ID { get; set; }

        /// <summary>
        /// Keeps track whether the item is available or not. 
        /// </summary>
        public bool Available
        {
            get
            {
                return _available;
            }
            set
            {
                _available = value;
            }
        }

        /// <summary>
        /// Returns whether item is available or not. If the item has expired then it will return false.
        /// </summary>
        public bool AvailabilityStatus
        {
            get
            {
                return DateTime.Compare(ExpirationDate, DateTime.Now) > 0 && _available;
            }

        }

        public override string ToString()
        {
            return Label;
        }
    }
}