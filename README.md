# Part #1
# Introduction to ASP.NET MVC Web Application

In this chapter you'll create a new MVC project to start the Nashville dog walking application, DogGo.

## Getting Started

1. Create new project in Visual Studio
1. Choose the _ASP.NET Core Web Application_
1. Specify project name of _DogGo_
1. Click _Ok_
1. Choose _Web Application (Model-View-Controller)_
1. Click _Ok_
1. Add the Nuget package for `Microsoft.Data.SqlClient`
   (Tab into Doggo, where Program.cs and Startup.cs are visable within the folder)
   ```sh
   dotnet add package Microsoft.Data.SqlClient
   dotnet restore
   ```

Take a look around at the project files that come out of the box with a new ASP.NET MVC project. It already has folders for Models, Views, and Controllers. It has a `wwwroot` folder which contains some static assets like javascript and css files. It has a `Startup.cs` file where we can configure certain things about our web application if we choose.

## Database

Run the [dog walker sql script](https://github.com/nashville-software-school/bangazon-inc/blob/cohort-43/book-2-mvc/chapters/assets/DogWalker.sql) to create database. Take a moment and look through the tables that get created.

## Configuration

Open the `appsettings.json` file and add your connection string. The file should look like this

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DogWalkerMVC;Trusted_Connection=True;"
  }
}
```

## Models

Create a `Neighborhood.cs` and `Walker.cs` file in the Models folder and add the following code

> Neighborhood.cs
```csharp
namespace DogGo.Models
{
    public class Neighborhood
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
```

> Walker.cs
```csharp
namespace DogGo.Models
{
    public class Walker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NeighborhoodId { get; set; }
        public string ImageUrl { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
```

Let's also create a repository for walkers. For now we'll just give it methods for getting all walkers and getting a single walker by their Id.

Create a new folder at root of the project called Repositories and create a `WalkerRepository.cs` file inside it. Add the following code

```csharp
using DogGo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace DogGo.Repositories
{
    public class WalkerRepository
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public WalkerRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public List<Walker> GetAllWalkers()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], ImageUrl, NeighborhoodId
                        FROM Walker
                    ";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Walker> walkers = new List<Walker>();
                    while (reader.Read())
                    {
                        Walker walker = new Walker
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        walkers.Add(walker);
                    }

                    reader.Close();

                    return walkers;
                }
            }
        }

        public Walker GetWalkerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], ImageUrl, NeighborhoodId
                        FROM Walker
                        WHERE Id = @id
                    ";

                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Walker walker = new Walker
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        reader.Close();
                        return walker;
                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }
                }
            }
        }
    }
}
```

## Controller

We can use Visual Studio to scaffold us the skeleton of a controller. Right click on the Controllers folder in Solution Explorer and click Add > Controller > MVC Controller with Read/Write actions. Give it the name `WalkersController`

Visual Studio kindly just created a whole bunch of code for us.

Add a private field for `WalkerRepository` and a constructor

```csharp
private readonly WalkerRepository _walkerRepo;

// The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
public WalkersController(IConfiguration config)
{
    _walkerRepo = new WalkerRepository(config);
}
```

### The power of ASP<span>.NET</span> Controllers

In the context of ASP<span>.NET</span>, each of the public methods in the controllers is considered an **Action**. When our application receives incoming HTTP requests, The ASP<span>.NET</span> framework is smart enough to know which controller Action to invoke.  

How does it do this? Take a look at the bottom of the `Startup.cs` class

```csharp
endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

ASPNET will inspect the parts of the url route. If a request comes in at `localhost:5001/Walkers/Index`, the framework will look for an `Index` action on the `Walker` controller and invoke it. If a request comes in at `localhost:5001/Walkers/Details/5`, The framework will look for a `Details` action in the `Walkers` controller and invoke it while passing in the parameter `5`. You'll also notice in the code above that some defaults have been set up in the routes. If the url of the request does not contain an action, the framework will invoke the Index action by default--meaning `localhost:5001/Walkers` would still trigger the `Index` action in the `Walkers` controller. Likewise, if the url doesn't contain a controller, i.e. `localhost:5001/`, the framework will assume the `Home` controller and the `Index` action. You are of course welcome to change these defaults.

### Get All Walkers

When a user is on `localhost:5001/Walkers`, we want to show them a view that contains a list of all the walkers in our system. Update the `Index` method to look like the following

```csharp
// GET: Walkers
public ActionResult Index()
{
    List<Walker> walkers = _walkerRepo.GetAllWalkers();

    return View(walkers);
}
```

This code will get all the walkers in the Walker table, convert it to a List and pass it off to the view. 



### Viewing the list of walkers

Currently, we're passing data into a view that doesn't exist. Let's fix that. Right click the method name `Index` in your controller and click "Add View". In the dialog box that appears, leave the view name "Index", for template select "List", and for Model class select "Walker". Then click the Add button. 

The generated view creates an html table and iterates over each walker in the list and creates a new row for each one.

##### Razor Templates

You'll notice a couple things about the code in the view. For one, it's not in an html file--it's in a cshtml file. This is called a _razor template_. With razor we can write a mix of C# and html code. It's similar to JSX in that it can dynamically create html. Once data gets passed into the view, the razor engine will convert it to an html page that can be returned to the browser. Here's an example of what razor code might look like

```html+razor
<h1>@Model.Name</h1>
```

And here is what the dynamically outputted html might look like

```html
<h1>Mo Silvera<h1>
```

We can also do things in our razor templates like make `if` statements or `foreach` loops to dynamically create html. Notice that any C# code that we want evaluated in the views starts with the `@` symbol. Also notice that the `Model` keyword is a reference to whatever object that the view receives from the controller. Assume in the example below that a controller has just passed the view a `List<Walker>`

```html+razor
<ul>
    @foreach (Walker walker in Model)
    {
        <li>@walker.Name</li>
    }
</ul>
```

Run the application and go to `/walkers/index`. You should see your data driven page.

The view that Visual Studio scaffolded for us is a decent start, but it has a number of flaws with it. For now, lets take care of the image urls. Instead of seeing the actual url, lets replace that with an actual image. Replace the code that say `@Html.DisplayFor(modelItem => item.ImageUrl)` with the following

```html
<img src="@item.ImageUrl" alt="avatar" />
```

Finally, uncomment the the code at the bottom of the view, and instead of using `item.PrimaryKey`, change the code to say `item.Id` on each of the action links.

```html+razor
<td>
    @Html.ActionLink("Edit", "Edit", new { id=item.Id }) |
    @Html.ActionLink("Details", "Details", new { id=item.Id }) |
    @Html.ActionLink("Delete", "Delete", new { id=item.Id })
</td>
```

These action links will generate `<a>` tags at runtime. The first one, for example, is saying that we want an `<a>` tag whose text content says the word "Edit", and we also want it to link to the `Edit` action in the controller. Lastly, it's saying that we want to include whatever `item.Id` is as a route parameter. The genereated anchor tag would look something like this

```html
<a href="/Walkers/Edit/5">Edit</a>
```

### Getting A single walker

When our users go to `/walkers/details/3` we want to take them to a page that has the details of the walker with the ID 3. To do this, we need to implement the `Details` action in the `Walkers` controller.

```csharp
// GET: Walkers/Details/5
public ActionResult Details(int id)
{
    Walker walker = _walkerRepo.GetWalkerById(id);

    if (walker == null)
    {
        return NotFound();
    }

    return View(walker);
}
```

Notice that this method accepts an `id` parameter. When the ASP<span>.NET</span> framework invokes this method for us, it will take whatever value is in the url and pass it to the `Details` method. For example, if the url is `walkers/details/2`, the framework will invoke the Details method and pass in the value `2`. The code looks in the database for a walker with the id of 2. If it finds one, it will return it to the view. If it doesn't the user will be given a 404 Not Found page.

Right click the Details method and select Add View. Keep the name "Details", select "Details" for the Template dropdown, and select "Walker" for the model class. Make the same changes in the view as before and replace the image url with the image tag

```html
<img class="bg-info" src="@Model.ImageUrl" alt="avatar" />
```

Run the application and go to `/walkers/details/1`. Then go to `/walkers/details/999` to see that we get a 404 response back.

## ERD
![ERD](https://i.imgur.com/WXwtNiI.png)


## Exercise

1. Create an `OwnerRepository` and an `OwnersController` file and implement the `Index` and `Details` methods.
1. Go into the `Shared` folder in the `_Layout.cshtml` file. Add links for "Walkers" and "Owners" in the navbar. If you finish, try changing the views and the styling to your liking.
1. **Challenge**: When viewing the details page of an owner, list all the dogs for that owner as well.



# ************************************************************************
# ************************************************************************
# ************************************************************************
# ************************************************************************

# Part #2

# Adding and Updating Data with MVC

In this chapter you'll continue to implement CRUD for the DogGo application by adding Create, Edit, and Delete routes for our dog owners.

<hr/>

**QUICK NOTE**

Did you get tired of stopping and restarting your server every time you make a change? There's a helpful package for you so that when you're only making changes to your view, you don't have to manually stop and restart your server.

To install it right click on the project name and select Manage Nuget Packages. Search for and install
```Microsoft.AspNetCore.MVC.Razor.RuntimeCompilation```

Then go to `Startup.cs` and change this line `services.AddControllersWithViews();` to this

```csharp
services.AddControllersWithViews().AddRazorRuntimeCompilation();
```

And you're good to go. To see changes made in your view, just save your cshtml file are refresh your browser. It'd be nice if we could also automatically refresh when we make changes to our controllers and repositories.... but for now this will have to do....

<hr/>

As part of the exercises in the previous chapter, you should have already created an OwnerRepository that has a method for getting all owners and getting a single owner by Id. We'll need additional CRUD functionality in the repository for this chapter, so update OwnerRepository to have the following code

```csharp
using DogGo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogGo.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public OwnerRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public Owner GetOwnerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Email, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        reader.Close();
                        return owner;
                    }

                    reader.Close();
                    return null;
                }
            }
        }

        public Owner GetOwnerByEmail(string email)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Email, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Email = @email";

                    cmd.Parameters.AddWithValue("@email", email);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        reader.Close();
                        return owner;
                    }

                    reader.Close();
                    return null;
                }
            }
        }

        public void AddOwner(Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    INSERT INTO Owner ([Name], Email, Phone, Address, NeighborhoodId)
                    OUTPUT INSERTED.ID
                    VALUES (@name, @email, @phoneNumber, @address, @neighborhoodId);
                ";

                    cmd.Parameters.AddWithValue("@name", owner.Name);
                    cmd.Parameters.AddWithValue("@email", owner.Email);
                    cmd.Parameters.AddWithValue("@phoneNumber", owner.Phone);
                    cmd.Parameters.AddWithValue("@address", owner.Address);
                    cmd.Parameters.AddWithValue("@neighborhoodId", owner.NeighborhoodId);

                    int id = (int)cmd.ExecuteScalar();

                    owner.Id = id;
                }
            }
        }

        public void UpdateOwner(Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            UPDATE Owner
                            SET 
                                [Name] = @name, 
                                Email = @email, 
                                Address = @address, 
                                Phone = @phone, 
                                NeighborhoodId = @neighborhoodId
                            WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@name", owner.Name);
                    cmd.Parameters.AddWithValue("@email", owner.Email);
                    cmd.Parameters.AddWithValue("@address", owner.Address);
                    cmd.Parameters.AddWithValue("@phone", owner.Phone);
                    cmd.Parameters.AddWithValue("@neighborhoodId", owner.NeighborhoodId);
                    cmd.Parameters.AddWithValue("@id", owner.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteOwner(int ownerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            DELETE FROM Owner
                            WHERE Id = @id
                        ";

                    cmd.Parameters.AddWithValue("@id", ownerId);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
```

After you update the `OwnerRepository` make sure you update the `IOwnerRepository` interface to include the `AddOwner`, `UpdateOwner` and `DeleteOwner` methods.

## Creating an Owner

Let's build out a form for us to be able to add a new Owner. Open the DogGo application you created in the previous chapter, go to the Owner controller, and find the `Create` action. You might notice...there are two Create methods! How can this be? Think about interactions we have in real life involving filling out forms. Doctors' visits comes to mind... When you go to the doctor, you're likely to have 2 interactions with a person behind the counter. The first interaction is when you go to the receptionist and ask for a blank form. The receptionist gives you the form so you can go back to your chair and fill it out. Once you're done, you can go back up to the counter and give that form back so they can process it. This is the same sort of interaction end users have with a server. Notice the comments above the two `Create` methods--one says GET and the other says POST. When a user navigates to the url `/owners/create`, they are making a GET request to that url. This is the request that will give the user the html of the empty form. When the user clicks a "submit" button, that is going to make a POST request to the same url. 

Go to the `Create` action for the GET request. Currently all it does is return a View. Since the only thing the server needs to do is hand the user a blank html form, this is actually fine. The only thing we have to do is create that html form. Right click the "Create" method name and select "Add View". Name the view "Create", give it the template of "Create", and the model class Owner

### Building the form

Let's first look at a couple things about this form. 

##### asp-for

```html
<label asp-for="Email" class="control-label"></label>
<input asp-for="Email" class="form-control" />
```

The `asp-for` attribute is something we get from ASP.<span>NET</span> and razor. When the attribute is on a `<label>` element, the generated html will be whatever the property name is. In the example here, the label will literally have the word "Email" in it. The resulting html will look like this

```html
<label for="Email" class="control-label">Email</label>
```

When the `asp-for` attribute is on an `<input>` element, it will generate html attributes that will allow us to know later on that the value for this input field should be set as an owner's email address.


##### asp-action

```html
<form asp-action="Create">
```

All of our input elements should be inside a form. The `asp-action` attribute is added to the form element to specify which controller action should be called when the form gets submitted. The the contents of the form we're building here should be submitted to the `Create` method in our controller.

##### Update the form

The view that visual studio creates for us is a good start, but we have to modify it at least a little bit. For starters, it added an input field for the user to enter in an ID. Users don't chose their own Ids--the database does--so we can remove the form goup div that has the Id input in it. 

There is currently an input field for the user to enter a Neighborhood Id into. It's doubtful the user knows the actual Id of the neighborhood they are in--ideally this would be replaced by a dropdown of possible neighborhood options. We'll do that in a later chapter.

Run the application and go to `localhost:5001/owners/create` to see the form. You can try to submit it, but it won't do anything yet...

### Submitting the form

When the user hits the "Create" button, the browser is going to make a POST request to the url `/owners/create`. In the body of that request will be the contents of the form. Go into the Owners controller and find the `Create` method that handles the POST. The method is currently set up to accept a paramter of type `IFormCollection`, but we know that the thing we are actually sending to the server is an Owner object. Change the method signature to look like the following

```csharp
public ActionResult Create(Owner owner)
```

We're seeing here another piece of magic we get from the ASP<span>.NET<span> framework. The framework knows how to _bind_ values it gets from html forms fields to C# objects.

Update the `Create` method in the OwnersController so that it inserts the new owner.

```csharp
// POST: Owners/Create
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(Owner owner)
{
    try
    {
        _ownerRepo.AddOwner(owner);

        return RedirectToAction("Index");
    }
    catch(Exception ex)
    {
        return View(owner);
    }
}
```

A couple things to note here. First, if everthing goes as expected and the new user gets entered into the database, we redirect the user back to `/owners/index`. Second, if some exception gets thrown, we want to return the same view the user on so they can try again.

Run the application and submit the form. The new owner should be added to the database.


## Deleting an owner

In your owner controller, find the delete methods. Again, you'll notice that there are two methods--one for GET and another for POST. The GET method assumes you'd like to create a view that asks the user to confirm the deletion. Notice that the GET method for `Delete` accepts an `int id` parameter. ASP.<span>NET<span> will get this value from the route. i.e. `owners/delete/5` suggests that the user is attempting to delete the owner with Id of 5. Let's assume that owner with the Id of 5 is Mo Silvera. We want the view to have some text on it that says "Are you sure you want to delete the owner Mo Silvera?". To be able to generate this text, we'll have to get Mo's name from the database.

Update your `Delete` method in the OwnerController to the following:

> OwnersController.cs
```csharp
// GET: Owners/Delete/5
public ActionResult Delete(int id)
{
    Owner owner = _ownerRepo.GetOwnerById(id);

    return View(owner);
}
```

Now create the view by right clicking the method name and selecting Add View. Keep the name of the template "Delete", choose "Delete" from the template dropdown, and set the model to Owner.

Instead of having the text say "Are you sure you want to delete this?", change it to include the name of the owner.

```html+razor
<h3>Are you sure you want to delete @Model.Name?</h3>
```

Run the application and go to `owners/delete/3` to view the delete confirmation page. 

If the user clicks the delete button, a POST request will be made to `/owners/delete/3`.

Update the POST `Delete` method in the controller to the following

> OwnerController.cs
```csharp
// POST: Owners/Delete/5
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Delete(int id, Owner owner)
{
    try
    {
        _ownerRepo.DeleteOwner(id);

        return RedirectToAction("Index");
    }
    catch(Exception ex)
    {
        return View(owner);
    }
}
```


## Editing an owner

Editing an owner is similar to creating an owner except when the user gets the form, it should be pre-popolated with the current data. Once again, the controller will get an owner id from the url route (i.e `/owner/edit/2`). We can use that Id to get the current data from the database and use it to fill out the initial state of the form. Find the GET method for `Edit` and update it with the following

```csharp
// GET: Owners/Edit/5
public ActionResult Edit(int id)
{
    Owner owner = _ownerRepo.GetOwnerById(id);

    if (owner == null)
    {
        return NotFound();
    }

    return View(owner);
}
```

Now update the POST method for `Edit`. This is similar to `Create` except we are updating the database instead of inserting into.

```csharp
// POST: Owners/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Edit(int id, Owner owner)
{
    try
    {
        _ownerRepo.UpdateOwner(owner);

        return RedirectToAction("Index");
    }
    catch(Exception ex)
    {
        return View(owner);
    }
}
```

Now create the view for Edit by right clicking the method name and going through the same steps. 

Same as before, we don't want to give the user a form field for the Id--users should be able to update their Ids. You can try removing the input like we did before with the `Create` view, but that won't work this time. Give it a try anyway...

Put a breakpoint in your controller code so you can inspect the owner object that gets passed as a parameter. Notice that the Id of the owner is now zero. The reason for this is that we took the Id field out of the form, so when it got posted back up to the server, it didn't have an Id value so C# defaults that value to zero. 

The trick is to _hide_ the input field in the view, but keep it in the form. Put the Id input field back in the form and give it a `type=hidden` attribute, and delete the `<label>` and `<span>` tags.

```html
<div class="form-group">
    <input asp-for="Id" type="hidden" class="form-control" />
</div>
```

## Exercise

Create a model for `Dog` and implement a `DogRepository` and `DogController` that gives users the following functionality:

- View a list of all Dogs
- Create a new Dog (for now, capture the OwnerId as simple input field)
- Edit a Dog
- Delete a Dog

# ************************************************************************
# ************************************************************************
# ************************************************************************
# ************************************************************************


# Part #3

# View Models

We've seen how controllers are able to pass objects into our views, and how views expect a certain type of object. For example:

> OwnersController.cs

```csharp
public ActionResult Details(int id)
{
    ...

    return View(owner);
}
```

> Owners/Details.cshtml

```html+razor
@model DogGo.Models.Owner

...

<dl class="row">
  <dt class = "col-sm-2">
    @Html.DisplayNameFor(model => model.Name)
  </dt>
  <dd class = "col-sm-10">
    @Html.DisplayFor(model => model.Name)
  </dd>
  <dt class = "col-sm-2">
    @Html.DisplayNameFor(model => model.Email)
  </dt>
  <dd class = "col-sm-10">
    @Html.DisplayFor(model => model.Email)
  </dd>
  <dt class = "col-sm-2">
    @Html.DisplayNameFor(model => model.Phone)
  </dt>
  <dd class = "col-sm-10">
    @Html.DisplayFor(model => model.Phone)
  </dd>
</dl>
```

In this example we can see that the `Details` method of the Owners Controller is passing an owner instance to the View. The view will then dynamically create some html using the properties of that object.

This works out fine if the page you're creating only requires owner information on it. Let's consider a more realistic scenario though. Say your designer asks that the Owner Details page of your application is a bit more complex. Here is the mockup you're given

![](https://i.imgur.com/oG367Ik.png)

This View now seems to require more data than just an Owner object. There's now information on the owner's dogs and walkers as well.

The problem is that in ASP<span>.NET</span>, we can only pass a View **one** type of thing. So how do we pass it multiple things?

The answer is to wrap all the things up in a single class called a View Model. The difference between regular models and view models is that regular models are meant to mimic the shape of our database tables. View models are meant to mimic the shape of our html pages. It may be helpful to think of view models as similar to react state. Looking at the mockup again, what are the things on the page that would belong in state?

![](https://i.imgur.com/IS5NiAs.png)

- An Owner object
- A list of Dogs
- A list of Walkers

Let's create a View Model that contains all of these things. Create a directory inside of your `Models` folder and name it `ViewModels`. Within that folder, create a file called `ProfileViewModel.cs` and add the following code

```csharp
using System;
using System.Collections.Generic;

namespace DogGo.Models.ViewModels
{
    public class ProfileViewModel
    {
        public Owner Owner { get; set; }
        public List<Walker> Walkers { get; set; }
        public List<Dog> Dogs { get; set; }
    }
}
```

We have a repository method already for getting a single owner, but we'll also need a method for getting a list of dogs by an owner Id, and method for getting a list of walkers in a given neighborhood.

Add this method to the `DogRepository`

> DogRepository.cs

```csharp
public List<Dog> GetDogsByOwnerId(int ownerId)
{
    using (SqlConnection conn = Connection)
    {
        conn.Open();

        using (SqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT Id, Name, Breed, Notes, ImageUrl, OwnerId 
                FROM Dog
                WHERE OwnerId = @ownerId
            ";

            cmd.Parameters.AddWithValue("@ownerId", ownerId);

            SqlDataReader reader = cmd.ExecuteReader();

            List<Dog> dogs = new List<Dog>();

            while (reader.Read())
            {
                Dog dog = new Dog()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Breed = reader.GetString(reader.GetOrdinal("Breed")),
                    OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                };

                // Check if optional columns are null
                if (reader.IsDBNull(reader.GetOrdinal("Notes")) == false)
                {
                    dog.Notes = reader.GetString(reader.GetOrdinal("Notes"));
                }
                if (reader.IsDBNull(reader.GetOrdinal("ImageUrl")) == false)
                {
                    dog.ImageUrl = reader.GetString(reader.GetOrdinal("Notes"));
                }

                dogs.Add(dog);
            }
            reader.Close();
            return dogs;
        }
    }
}
```

Because we are updating the DogRepository, we also need to add a corresponding method to the IDogRepository
```csharp
List<Dog> GetDogsByOwnerId(int ownerId);
```
Now update the Walkers Repository to add a method to get a list of walkers in a neighborhood

> WalkersRepository.cs

```csharp
public List<Walker> GetWalkersInNeighborhood(int neighborhoodId)
{
    using (SqlConnection conn = Connection)
    {
        conn.Open();
        using (SqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT Id, [Name], ImageUrl, NeighborhoodId
                FROM Walker
                WHERE NeighborhoodId = @neighborhoodId
            ";

            cmd.Parameters.AddWithValue("@neighborhoodId", neighborhoodId);

            SqlDataReader reader = cmd.ExecuteReader();

            List<Walker> walkers = new List<Walker>();
            while (reader.Read())
            {
                Walker walker = new Walker
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                };

                walkers.Add(walker);
            }

            reader.Close();

            return walkers;
        }
    }
}
```
Similar to how we updated the IDogRepository, we also need to change the IWalkerRepository by adding the following method:
```csharp
List<Walker> GetWalkersInNeighborhood(int neighborhoodId);
```
Now that the Owner Details view will need to know about more than just the owner, we'll need access to other repositories. Update the private fields and constructor in your OwnerController class to add them

> OwnerController.cs

```csharp
private IOwnerRepository _ownerRepo;
private IDogRepository _dogRepo;
private IWalkerRepository _walkerRepo;

public OwnersController(IOwnerRepository ownerRepo, IDogRepository dogRepo, IWalkerRepository walkerRepo)
{
    _ownerRepo = ownerRepo;
    _dogRepo = dogRepo;
    _walkerRepo = walkerRepo;
}
```

Now that we have all the repositories we need, we can refactor the Owner Details method to create a ProfileViewModel. Change the `Details` method to use the following code

> OwnersController.cs 

```csharp
// GET: Owners/Details/5
public ActionResult Details(int id)
{
    Owner owner = _ownerRepo.GetOwnerById(id);
    List<Dog> dogs = _dogRepo.GetDogsByOwnerId(owner.Id);
    List<Walker> walkers = _walkerRepo.GetWalkersInNeighborhood(owner.NeighborhoodId);

    ProfileViewModel vm = new ProfileViewModel()
    {
        Owner = owner,
        Dogs = dogs,
        Walkers = walkers
    };

    return View(vm);
}
```

Try running the application now and going to `/owners/details/1` while logged in. You should see this error message

```
InvalidOperationException: The model item passed into the ViewDataDictionary is of type 'DogWalker.Models.ViewModels.ProfileViewModel', but this ViewDataDictionary instance requires a model item of type 'DogWalker.Models.Owner'
```

This is because the controller is now passing the view an instance of `ProfileViewModel` but the view is still expecting an `Owner`. Fix this by changing the first line of `Details.cshtml` to this

```html+razor
@model DogGo.Models.ViewModels.ProfileViewModel
```

Now replace the rest of the view with the following code

```html+razor
@model DogGo.Models.ViewModels.ProfileViewModel 

@{ ViewData["Title"] ="Profile"; }
<div>
  <h1 class="mb-4">@Model.Owner.Name</h1>

  <section class="container">
    <img
      style="width:100px;float:left;margin-right:20px"
      src="https://upload.wikimedia.org/wikipedia/commons/a/a0/Font_Awesome_5_regular_user-circle.svg"
    />
    <div>
      <label class="font-weight-bold">Address:</label>
      <span>@Model.Owner.Address</span>
    </div>
    <div>
      <label class="font-weight-bold">Phone:</label>
      <span>@Model.Owner.Phone</span>
    </div>
    <div>
      <label class="font-weight-bold">Email:</label>
      <span>@Model.Owner.Email</span>
    </div>
  </section>

  <hr class="mt-5" />
  <div class="clearfix"></div>

  <div class="row">
    <section class="col-8 container mt-5">
      <h1 class="text-left">Dogs</h1>

      <div class="row">
        @foreach (Dog dog in Model.Dogs) {
        <div class="card m-4" style="width: 18rem;">
          @if (String.IsNullOrEmpty(dog.ImageUrl)) {
          <img
            src="https://cdn.pixabay.com/photo/2018/08/15/13/12/dog-3608037_960_720.jpg"
            class="card-img-top"
            alt="Doggo"
          />
          } else {
          <img src="@dog.ImageUrl" class="card-img-top" alt="Doggo" />
          }
          <div class="card-body">
            <div>
              <label class="font-weight-bold">Name:</label>
              <span>@dog.Name</span>
            </div>
            <div>
              <label class="font-weight-bold">Breed:</label>
              <span>@dog.Breed</span>
            </div>
            <div>
              <label class="font-weight-bold">Notes:</label>
              <p>@dog.Notes</p>
            </div>
          </div>
        </div>
        }
      </div>
    </section>

    <section class="col-lg-4 col-md-8 container mt-5">
      <h1>Walkers Near Me</h1>

      <ul class="list-group mt-4">
        @foreach (Walker walker in Model.Walkers) {
        <li class="list-group-item disabled" aria-disabled="true">
          <img src="@walker.ImageUrl" style="width:50px" />
          <span class="font-weight-bold ml-4">@walker.Name</span>
        </li>
        }
      </ul>
    </section>
  </div>
</div>
```

## Using View Models with Forms

Currently the Create and Edit forms for Owners have a text input field to collect an owner's neighborhood Id. It was mentioned ealier that we'd ideally like to have that be a dropdown list instead. We can make this happen with view models. Once again, lets think about what we'd need to have in _state_ if this were a React application. 

- Properties for all the Owner form fields
- A list of available options for the dropdown

Create a new class inside the ViewModels folder and name it `OwnerFormViewModel`. Add the following code

```csharp
using System.Collections.Generic;

namespace DogGo.Models.ViewModels
{
    public class OwnerFormViewModel
    {
        public Owner Owner { get; set; }
        public List<Neighborhood> Neighborhoods { get; set; }
    }
}
```

We don't yet have a repository for Neighborhoods, so lets add that now. Create a `NeighborhoodRepository.cs` file in the repositories folder and add the following code

> NeighborhoodRepository.cs

```csharp
using DogGo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace DogGo.Repositories
{
    public class NeighborhoodRepository : INeighborhoodRepository
    {
        private readonly IConfiguration _config;

        public NeighborhoodRepository(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public List<Neighborhood> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name FROM Neighborhood";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Neighborhood> neighborhoods = new List<Neighborhood>();

                    while (reader.Read())
                    {
                        Neighborhood neighborhood = new Neighborhood()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };
                        neighborhoods.Add(neighborhood);
                    }

                    reader.Close();

                    return neighborhoods;
                }
            }
        }
    }
}

```

Just like before we have to create the `INeighborhoodRepository` and register it with the `Startup.cs` class

> INeighborhoodRepository

```csharp
using DogGo.Models;
using System.Collections.Generic;

namespace DogGo.Repositories
{
    public interface INeighborhoodRepository
    {
        List<Neighborhood> GetAll();
    }
}
```

> Startup.cs

```csharp
services.AddTransient<INeighborhoodRepository, NeighborhoodRepository>();
```

Now add a `NeighborhoodRepository`  to the fields and the constructor inside `OwnersController` like before

> OwnersController.cs

```csharp
private readonly OwnerRepository _ownerRepo;
private readonly DogRepository _dogRepo;
private readonly WalkerRepository _walkerRepo;
private readonly NeighborhoodRepository _neighborhoodRepo;


// The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
public OwnersController(IConfiguration config)
{
    _ownerRepo = new OwnerRepository(config);
    _dogRepo = new DogRepository(config);
    _walkerRepo = new WalkerRepository(config);
    _neighborhoodRepo = new NeighborhoodRepository(config);

}
```

Update the GET `Create` method to now create a view model and pass it to the view

> OwnersController.cs

```csharp
// GET: Owners/Create
public ActionResult Create()
{
    List<Neighborhood> neighborhoods = _neighborhoodRepo.GetAll();

    OwnerFormViewModel vm = new OwnerFormViewModel()
    {
        Owner = new Owner(),
        Neighborhoods = neighborhoods
    };

    return View(vm);
}
```

Now update the view to accept an instance of an `OwnerFormViewModel` and change the NeighborhoodId field from an `<input>` to a `<select>` 

```html+razor
@model DogGo.Models.ViewModels.OwnerFormViewModel

@{
    ViewData["Title"] = "Create";
}

<h1>Create</h1>

<h4>Owner</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Owner.Email" class="control-label"></label>
                <input asp-for="Owner.Email" class="form-control" />
                <span asp-validation-for="Owner.Email" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Owner.Name" class="control-label"></label>
                <input asp-for="Owner.Name" class="form-control" />
                <span asp-validation-for="Owner.Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Owner.Phone" class="control-label"></label>
                <input asp-for="Owner.Phone" class="form-control" />
                <span asp-validation-for="Owner.Phone" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Owner.Address" class="control-label"></label>
                <input asp-for="Owner.Address" class="form-control" />
                <span asp-validation-for="Owner.Address" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Owner.NeighborhoodId" class="control-label"></label>
                <select asp-for="Owner.NeighborhoodId" class="form-control">
                    <option value="">Select Neighborhood</option>
                    @foreach (Neighborhood neighborhood in Model.Neighborhoods)
                    {
                        <option value="@neighborhood.Id">@neighborhood.Name</option>
                    }
                </select>
                <span asp-validation-for="Owner.NeighborhoodId" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-action="Index">Back to List</a>
</div>
```

## Exercise

1. Update the `/owner/edit/{id}` route to use the `OwnerFormViewModel` so that the Neighborhood Id uses a dropdown instead of an input. field. 

1. Try to implement the following design for the walker details page at `/walkers/details/{id}`. Hint: Use the `DateTime` class to help format the date strings.

![](https://i.imgur.com/aCD0SKf.png)
