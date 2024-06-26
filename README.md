The "Table-Booking-System" is an intuitive and 
efficient reservation software designed to streamline the 
process of managing your bookings digitally.
It enables online reservations and includes an 
administrative dashboard for managing various aspects such as floors, 
tables, food, bookings, orders, staff, 
customers, system settings, and more.



The software is developed using ASP.NET MVC and C# programming language, 
ensuring a reliable and efficient performance. 

To implement this application, please follow the deployment steps provided below:

**NOTE: Make sure you have MSSQL Server and SQL Server Management Studio  Installed.
        Also, if you prefer to use Visual Studio IDE, make sure you have the 2015 version or above.



1.      Download or clone the repo 

2.	Extract the zip file to your localhost directory if you are running MS IIS, 
        or your web directory if deployed on a web server.
3.	Open Your SQL Server Management Studio and Create a New Database
4.	Open the "SQL.sql" file, and copy all of its contents
5.	Select your newly created database, and click on "New Query" from the menu bar
6.	Paste the contents you have copied from the "SQL.sql" file, into the query window
7.	Then Click the "Execute" button to create the database tables
8.	Go to your project directory and open "Web.config" file located in the "BookingTable.Web" folder 

9.	Locate the "connectionStrings" tag and find this line of code below:

            add name="BookingTableEntities" connectionString="data source=YOUR SERVER NAME;initial catalog=YOUR DATABASE NAME;persist security info=True;user id=YOUR USERNAME;password=YOUR PASSWORD;MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SqlClient"
           

10.    Replace the following placeholders with your credentials:

                             "YOUR SERVER NAME"     (Replace this with your server name);

                             "YOUR DATABASE NAME"   (Replace this with your database name);

                             "YOUR USERNAME"        (Replace this with your databse Username);

                             "YOUR PASSWORD"        (Replace this with your databse password);
           


11.	Finally, open your web browser and navigate to your site for example:    
        "http://localhost/" or "
http://www.yoursite.com/

"


TO LOGIN AS SAMPLE CUSTOMER:

	use the following credentials:
                                      USERNAME:  customer1
                                      PASSWORD:  password


TO LOGIN AS ADMIN:

        First navigate to your site and add "/admin/home" to your url, for example: 
        "http://localhost/admin/home"  or  "
http://www.yoursite.com/admin/home

"

	  use the following credentials to login:
                                             USERNAME:  admin
                                             PASSWORD:  password






If you are having any trouble deploying or using this application, 
please feel free to contact me at :

Email:     harbmathew@yahoo.com

Whatsapp:  +220 7425159

Tel:       00220 7425159

                                                                            




 
