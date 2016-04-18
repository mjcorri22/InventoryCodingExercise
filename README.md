# InventoryCodingExercise

I am posting the entire solution to my REST API for my Inventory Tracking service. The way I deployed my site was to:
1. Copy the folder InventoryWebService to C:\inetpub\wwwroot
2. Open up IIS Manager
3. Right click Default Web Site and add a new application
4. Give the site an alias
5. Click the Select button to change the Application Pool to .Net 4.5
6. Change the Physical Path to C:\inetpub\wwwroot\InventoryWebService
7. Click OK

ADDING AN ITEM:
To add an item through the Inventory API you make a POST call and pass in the body a JSON string that contains:
  Label
  Name
  ExpirationDate
  Quantity
  Size
If the item was succesfully added then the JSON string will be returned with the ID of the value included. If the add failed then an error message will be returned.

CHECKING OUT AN ITEM:
To check out an item from the Inventry API you make a GET call and pass in the label value through the Uri. If an available item that matches the label is discovered then the item is marked an unavailable and the item information is returned to the user. If no match is found then an error message will be returned.

RETURN AN ITEM:
To put an item back in the inventory the user must use the inventory ID value and make a Put call with the ID in the URI.

Code design:
I did not implement any security features into the code. I've used tokens where I've logged into a system and when I logged in I was assigned a token value to use the the headers of my messages. My experience was with the client side. My plan is to return to this project and update it to use username validation. 

The code has users reserve items by passing in the label and then items are marked as unavailable and the user is informed of the reserved item. There should not be an issue with users trying to reserve an item and 2 users making a request at the same time reserving the same item.

The database is an in memory dictionary. Items are always added to the dictionary, they are never removed, even if an item is taken out or an item expires. The positive to this is that item IDs are easy to generate, just use the size of the dictionary. (Although using SQL would be just as easy, but I was just trying to use the fastest method.) The other benifit is that users can check an item out and then return it in case they accidenatlly checked out the item or changed their mind. The downside is that the longer the service runs the slower it will run to search through the items. It would be optimized by including an ID counter and removing items that have expired from the dictionary since it wouldn't matter if those items were returned, they couldn't be used.

Future possible work:
I added the quantity and the size of the inventory items to the DB to be kept track of. It would make getting items from the inventory easier if items could also be searched for based on the quantity, size of the pills/capsules/bottle, or use the epiration date, to grab the next item that is set to expire.


Testing:
To test the system I used a REST API tool called poster. This is a plugin for Firefox. I then tried various test cases:
Adding a new item
Checking out existing item
Adding multiple items
Try to check out an item that is not in stock
Try to check out an item that is in stock but expired
Add item that has already expired

Example test data:
={Label: 'Tylenol', Name: 'Acetaminophen', ExpirationDate: '06/01/2016', Quantity: '100', Size: '50mg'}
={Label: 'Tylenol', Name: 'Acetaminophen', ExpirationDate: '02/01/2016', Quantity: '100', Size: '50mg'}
={Label: 'Tylenol', Name: 'Acetaminophen', ExpirationDate: '06/01/2016', Quantity: '75', Size: '100mg'}
={Label: 'Tylenol', Name: 'Acetaminophen', ExpirationDate: '06/01/2016', Quantity: '200', Size: '100mg'}
={Label: 'Tylenol', Name: 'Acetaminophen', ExpirationDate: '06/01/2016', Quantity: '100', Size: '50mg'}
={Label: 'Ibuprofen', Name: 'Ibuprofen', ExpirationDate: '06/01/2016', Quantity: '100', Size: '50mg'}


Requirements
1. Add an item to the inventory:
When I add a new item to the inventory
Then inventory contains information about the newly added item, such as Label, Expiration, and Type.
 
2. Take an item from the inventory by Label:
When I took an item out from the inventory
Then the item is no longer in the inventory
 
3. Notification that an item has been taken out:
When I took an item out from the inventory
Then there is a notification that an item has been taken out.
 
4. Notification that an item has expired:
When an item expires
Then there is a notification about the expired item.

