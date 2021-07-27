# Kebapi ASP.Net

<br>
<br>

<blockquote>
<p align="center">
<span style="font-size:1.5rem;">An asynchronous ASP.NET REST API, specifically written without the usual frameworks and packages in favour of exploring vanilla ASP.NET, and to satisfy a not-so-secret lust for dirty kebabs!</span>
</p>
</blockquote>

<br>

<p align="center">
  <img src="Doc/kebab-30deg.svg" width="200" />
</p>

<br>
<br>

For the eagle-eyed, this is an ASP.NET flavoured evolution of the original [Node.js kebapi](https://github.com/critr/kebapi "A seriously tasty REST API") project! As you'll see, it's a little older and a little wiser than its sibling, but markedly more complex. Whether that's a good thing I'll leave for another discussion.

<br>

The purpose of the API is to implement a small set of operations that can serve discerning kebab-eating clients everywhere.

This time, the API fronts a **SQL Server** database tamed by a **home grown data access layer** built from `SqlClient` objects only (🙅 NO 🚫 to bloaty Entity Framework here!). The layer leverages the **Geography** spatial data type so we get **on-the-fly GPS distance calculations** given any latitude and logitude. There's also a **routing service** that maps endpoints to API actions while applying **roles-based authorisation** rules and policies, and an authentication/login service that can **authenticate via different sets of credentials**. Login and security are built upon **salt 'n' hashed crypto goodness** carefully wrapped in **JSON Web Tokens**. (`JwtBearer` and `SqlClient` are the *only* packages used for the core API.) **Config** is handled by **parsable POCOs** plucked from the latest `IConfigure`-hoo-ha.

On the data side, there are **scripted rebuilds** of the database schema, including test data loading (all triggerable in Dev Environment from the API), and the database model includes typical normalisation and PK/FK/data integrity constraints. All SQL statements are purposely stowed in their own easy-to-reach classes.

On the testing side, instead of synthesising my own from-the-ground-up tests as happened with [the Node.js version](https://github.com/critr/kebapi "A seriously tasty REST API"), I've opted to go with a 'proper' testing framework. Enter **Xunit** *and* test fixtures *and* test database replication *and* mock authentication handling *and* some 200 unit tests.

Feeling hungry yet? Hope so! Let's tuck in!

<br>

## 🥙 Get started

**tl;dr:**

```
dotnet run kebapi
```

`https://localhost:5001/admin/dev/resettestdb`

Peruse and use [the endpoints](#-api-endpoints)!

<br>

**In more detail:**

You'll need access to something running SQL Server. The SQL Server Express version is fine for our purposes.

Download this repo. Then review the file `appsettings.json`, particularly the connection string, for any changes you may need to make in your environment. Note that the named database in the connection string does *not* need to exist beforehand. Config is covered [in more detail](#-config) later if you need a hand.

To fire up the API, either run it in Visual Studio or go to the project root and in your bash/command prompt hit:

```
dotnet run kebapi
```

It should come back with something like this:

```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
```

Make a note of where it's listening, because that'll be the root of all your requests. Throughout this doc we'll be going with the `https://localhost:5001` SSL-protected version.

I recommend using [Insomnia](https://insomnia.rest/) for anything you need to poke with a request. ([Postman](https://www.postman.com/) is another popular alternative.)

If it's your fist run of Kebapi, you'll need to set up the database and get some data into it. [In a Dev Environment](#setting-up-in-a-dev-environment), we can do that through the API. Send it this `GET` request:

`https://localhost:5001/admin/dev/resettestdb`

⚠️ WARNING! All previous data will be blitzed every time you run that command! But it's perfect to use over and over for testing so long as your database doesn't contain data you care about, because it will be reset each time.

In reponse to the reset request, you should get back a very plain `"Success"` message. This means your database has been created, is ready to go, *and* it contains sufficient test data for you to play around with all of the API's endpoints! Hurrah!

Jump to the [API Enpoints](#-api-endpoints) or have a quick scan of the config, covered next.

## 🥙 Config

The file `appsettings.json` will be your goto for tweaking most settings:


```json
"Settings": {
   "Dal": {
     "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=KebapiASPNet;Trusted_Connection=True;MultipleActiveResultSets=true",
     "MaxSelectRows": 10
   },
   "Api": {
     "Paging": {
       "MinStartRow": 0,
       "MinRowCount": 1,
       "MaxRowCount": 8
     },
     "UserRegistration": {
       "MinUsernameLength": 3,
       "MinPasswordLength": 8
     },
     "Auth": {
       "TokenValidation": {
         "Issuer": "https://apisite.com",
         "Audience": "https://apisite.com",
         "ExpireMinutes": "60"
       }
     }
   }
 }
```

Those settings should be reasonably self-explanatory, but if not, review the comments in `Settings.cs` which is the POCO that maps directly to this config.

There is one setting deliberately omitted from the config file, and which is instead expected to be set up in the Environment. This is just a security choice for this project. So ensure you have an Environment Variable* called `KEBAPI_AUTH_SECRET` and add to it a suitably long and cryptic value, because that value will be used to sign all security tokens for the API. (*In Windows: System Properties > Advanced tab > Environment Variables.)

## 🥙 API Endpoints
Each table below will focus on a set of API actions, show you how to invoke them, and indicate any restrictions for doing so.

Do use something like [Insomnia](https://insomnia.rest/) (or [Postman](https://www.postman.com/)) to fire off requests to the endpoints with the correct HTTP methods and data, otherwise you'll likely get unexpected results.

From the tables you'll see that many resources (endpoints) are either owned by a user, or restricted by role. To access those endpoints, you'll need to authenticate first and use the token you get back. If you don't, you'll likely get a `401 (Unauthorised)` response and not much more. (No body will be returned.)

To authenticate, we just need to send a request to the `users/auth` endpoint with a username (or email) and a password as JSON. [See here](#discerning-clientele). When you successfully authenticate, you'll get back a token just like the one below, only completely different and unique! Copy it! (Just the encoded string after "Token": "*copy-the-gobbledygook-here-without-quotes*")

```JSON
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Authentication succeeded.",
    "Errors": []
  },
  "ApiSecurityToken": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJCYWJzIiwianRpIjoiMzBlMzVmNGQtYjNkYS00N2MyLTg4MWYtYTYxODhmNzEzMDgwIiwidXNlcm5hbWUiOiJCYWJzIiwiaWQiOiIyIiwiZGlzcGxheW5hbWUiOiJMdWN5IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTYyNzA5MjE5MywiaXNzIjoiaHR0cHM6Ly9hcGlzaXRlLmNvbSIsImF1ZCI6Imh0dHBzOi8vYXBpc2l0ZS5jb20ifQ.AhlCmFzILHXY0hvnZw-n-c1gs37ZrQBu6sxXxgSLkiI",
    "Expires": "2021-07-24T02:03:13Z"
  }
}
```

So, to get the expected response, authenticate first (or register a new user and then authenticate), and then supply the returned token in your next requests. (If using Insomnia, in your request, set the Auth tab to "Bearer Token" and paste the copied token in there.) Note that each returned token is accompanied by an expiry date. After that date/time, you will need to authenticate again to get a new token to put into your requests. (That expiry period can be set in [the config](#-config).)

### Setting up (in a Dev Environment)

These endpoints are for database management, and only available if your Environment has an `ASPNETCORE_ENVIRONMENT` variable set to ``"Development"``. (In VS you can edit this variable in Project properties under Debug.)

⚠️ WARNING! These endpoints can and will blitz existing data in your database and will run immediately with no further prompt. They are perfect for testing and setting up, but apply due caution.

When running the API for the first time, and assuming you want to use the included test data, do a `GET` on  `https://localhost:44383/admin/dev/resettestdb`. That will create and automagically populate the database with enough to get you going with every endpoint.

You are of course free to add your own data and use the endpoints only to create a database and add a schema. Said schema will include creation of all of the tables and database constraints needed to run the API.

As alluded to above, there's just one blanket \*restriction<sup>1</sup> for all of these endpoints, abbreviated in the table below as:
<br>&emsp; **IDE** - Is Development Environment.

| Method | Endpoint | Restrictions* | Description | Example
|----|------------|------------|------------|------------
| GET | `admin/dev/createdb` | IDE | Create empty db if it doesn't exist. | 🔧[`https://localhost:5001/admin/dev/createdb`](https://localhost:5001/admin/dev/createdb)
| GET | `admin/dev/dropdb` | IDE | Drop db if it exists. | 🔨[`https://localhost:5001/admin/dev/dropdb`](https://localhost:5001/admin/dev/dropdb)
| GET | `admin/dev/resetdb` | IDE | On db perform: drop, create, add schema. | 🛠️[`https://localhost:5001/admin/dev/resetdb`](https://localhost:5001/admin/dev/resetdb)
| GET | `admin/dev/resettestdb` | IDE | On db perform: drop, create, add schema, add test data. | 🛠️[`https://localhost:5001/admin/dev/resettestdb`](https://localhost:5001/admin/dev/resettestdb)


### Kebab eateries

Eateries are referred to as "venues" in the API, with each one representing an exquisite place of juicy kebab nirvana to fizzy-drink and dine at!

Venues can be browsed (with any paging arguments), and viewed in detail. A count of the number of venues is also obtainable.

A cool feature is that you can use **real GPS coordinates** to check distances to venues too. Since our test data contains fictional venues, I've overlain them onto *real* GPS latitudes and longitudes. So while the venues continue to be fictitious, their GPS coordinates aren't and you can use them with this API! Check out this image!

<p align="center">
<a href="Doc/venues-gps-grid.jpeg?raw=true"><img src="Doc/venues-gps-grid.jpeg?raw=true" alt="Map of our test kebab houses superimposed with real GPS latitudes and longitudes" title="How far to kebab nirvana? GPS it with the API!" style="max-width:100%;"></a>
</p>

Now try the `venues/:venueId/distance` endpoint!

Feel free to use any GPS coordinates from [Google Maps!](https://www.google.com/maps/@40.4065396,-3.6915295,14z?hl=en) But if you want to use the example coordinates in the image, here they are for ease of copy-pasting:

| Point of origin (🔵 blue circles) | Latitude & Longitude
|----|----
| 🔵1 | `originLat=40.42313821277501&originLng=-3.7299816289728036`
| 🔵2 | `originLat=40.40281412801246&originLng=-3.669299331455333`
| 🔵3 | `originLat=40.38300694594641&originLng=-3.713845459335909`

And for reference:

| Venues (with grid X,Y)| Latitude & Longitude
|----|----
| 🛐1 Splendid Kebabs (2,1) | `40.42795262756104, -3.7116578794243558`
| 🛐2 The Kebaberie (5,2) | `40.42207211170083, -3.6853078577300646`
| 🛐3 Meats Peeps (7,8) | `40.38268004589479, -3.6687681075408354`
| 🛐4 The Rotisserie (1,9) | `40.37640325727719, -3.719853607604965`
| 🛐5 The Dirty One (4,1) | `40.42847531520142, -3.6942342494440914`
| 🛐6 Bodrum Conundrum (5,5) | `40.40144154092246, -3.684389293040381`
| 🛐7 Korner Kebab (3,1) | `40.4286059864768, -3.7027314877103286`
| 🛐8 Star Kebab (3,6) | `40.39588554537884, -3.702757060605783`
| 🛐9 Kebab Slab (5,7) | `40.38980610735031, -3.6858484147628663`
| 🛐10 Turku Kebabi (9,0) | `40.43500856795563, -3.650975581906885`


\
Endpoints for venues have access \*restrictions<sup>1</sup>, so not every action is available for every request. In the table below these are abbreviated as:
<br>&emsp;**AA** - Allow Anonymous (i.e. specifically no restriction).
<br>&emsp;**IRA** - Is Role Admin.
<br>&emsp;**IDE** - Is Development Environment.
<br>Applicable logical operators<sup>2</sup> for these restrictions are represented.


| Method | Endpoint	| Restrictions* | Description | Example
|----|------------|------------|------------|------------
| GET | `venues/:venueId/distance?originLat=latitude&originLng=longitude` | AA | Gets the distance in m, km, mi from any GPS origin point specified by the query variables `originLat` and `originLng`, to the exquisite kebab house uniquely identified by `venueId`. | 🗺️ [`https://localhost:5001/venues/1/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](https://localhost:5001/venues/1/distance?originLat=40.42313821277501&originLng=-3.7299816289728036)
| GET | `venues/:venueId`| AA | Retrieves details of a single place of kebab worship, by its id. | 🥙[`https://localhost:5001/venues/2`](https://localhost:5001/venues/2)
| GET | `venues (optional: ?startRow=n&rowCount=n)` | AA | Retrieves a list of fine kebab eateries, optionally beginning at `startRow` and optionally continuing for `rowCount` rows. Defaults are applied if the optional parameters are not supplied. | 🥙1. [`https://localhost:5001/venues`](https://localhost:5001/venues) <br> 🥙2. [`https://localhost:5001/venues?startRow=4`](https://localhost:5001/venues?startRow=4) <br> 🥙3. [`https://localhost:5001/venues?rowCount=2`](https://localhost:5001/venues?rowCount=2) <br> 🥙4. [`https://localhost:5001/venues?startRow=6&rowCount=3`](https://localhost:5001/venues?startRow=6&rowCount=3)
| GET | `venues/count` | IDE+IRA | Retrieves *the count* (total number) of enticing kebab houses registered. | 🧛[`https://localhost:5001/venues/count`](https://localhost:5001/venues/count)

### Discerning clientele
Fans of kebabs are referred to (very underwhelmingly) as "users" in the API, with each one of course representing an upstanding person of impeccable taste and manners.

Users can use the API to register, authenticate themselves (login), add/remove/peruse their favourite venues (with any paging arguments), and activate/deactivate their accounts (through a soft delete mechnotechnonism™).

Users can also be browsed (with any paging arguments), searched for by username, and viewed in detail. A count of the number of users is also obtainable.

Endpoints for users have access \*restrictions<sup>1</sup>, so not every action is available for every request. In the table below these are abbreviated as:
<br>&emsp;**AA** - Allow Anonymous (i.e. specifically no restriction).
<br>&emsp;**IRA** - Is Role Admin.
<br>&emsp;**IO** - Is resource Owner.
<br>Applicable logical operators<sup>2</sup> for these restrictions are represented.



| Method | Endpoint	| Restrictions* | Description | Example
|----|------------|------------|------------|------------
| POST | `users/auth` | AA | Authenticates a splendid user. <details><summary>Sample `Content-Type:application/json`:</summary><code>{"UsernameOrEmail": "Babs", "Password": "lucy1"}</code></details> |  👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)
| GET | `users (optional: ?startRow=n&rowCount=n)` | AA | Retrieves a list of fine upstanding users, optionally beginning at `startRow` and optionally continuing for `rowCount` rows. Defaults are applied if the optional parameters are not supplied. | 👪1. [`https://localhost:5001/users`](https://localhost:5001/users) <br> 👪2. [`https://localhost:5001/users?startRow=4`](https://localhost:5001/users?startRow=4) <br> 👪3. [`https://localhost:5001/users?rowCount=2`](https://localhost:5001/users?rowCount=2) <br> 👪4. [`https://localhost:5001/users?startRow=3&rowCount=2`](https://localhost:5001/users?startRow=3&rowCount=2)
| GET | `users/find?username=a` | AA | Gets the delightful user whose lovingly-crafted username exactly matches `username`. | 👨[`https://localhost:5001/users/find?username=MeatyMan`](https://localhost:5001/users/find?username=MeatyMan)
| GET | `users/:id` | IRA,IO | Obtains the stupendous user uniquely (and without equal) identified by `id`. |  👩[`https://localhost:5001/users/4`](https://localhost:5001/users/4)
| GET | `users/:id/favourites (optional: ?startRow=n&rowCount=n)` | IRA,IO | Cunningly retrieves a list of favourite venues, as carefully selected by the thoughtful user uniquely identified by `id`, optionally beginning at `startRow` and optionally continuing for `rowCount` rows. Defaults are applied if the optional parameters are not supplied. |  💗1. [`https://localhost:5001/users/2/favourites`](https://localhost:5001/users/2/favourites) <br> 💗2. [`https://localhost:5001/users/2/favourites?startRow=2`](https://localhost:5001/users/2/favourites?startRow=2) <br> 💗3. [`https://localhost:5001/users/2/favourites?rowCount=2`](https://localhost:5001/users/2/favourites?rowCount=2) <br> 💗4. [`https://localhost:5001/users/2/favourites?startRow=2&rowCount=1`](https://localhost:5001/users/2/favourites?startRow=2&rowCount=1)
| POST | `users/:id/favourites/{venueId}` | IRA,IO | Adds the endearing venue uniquely identified by `venueId` to the list of favourites of the tasteful user uniquely identified  by `id`.<br>*No JSON is expected in this POST request.* | 💖[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)
| DELETE | `users/:id/favourites/{venueId}` | IRA,IO | Removes the appetising venue uniquely identified by `venueId` from the list of favourites of the discerning user uniquely identified  by `id`. | 💔[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)
| GET | `users/:id/status` | IRA,IO | Obtains the account status of the extraordinary user uniquely identified by `id`. (A status can be 'active' or 'inactive', and models a soft delete.) | ❔[`https://localhost:5001/users/2/status`](https://localhost:5001/users/2/status)
| PATCH | `users/:id/activate` | IRA,IO | Activates the account of the beautiful user uniquely identified by `id`. (Sets the account status to 'active', modelling a soft *un*delete.) | ✔️[`https://localhost:5001/users/2/activate`](https://localhost:5001/users/2/activate)
| PATCH | `users/:id/deactivate` | IRA,IO | Deactivates the account of the delectable user uniquely identified by `id`. (Sets the account status to 'inactive', modelling a soft delete.) | ❌[`https://localhost:5001/users/2/deactivate`](https://localhost:5001/users/2/deactivate)
| POST | `users/register` | AA | Registers a supremely intelligent user, so they can use our delectable service. <details><summary>Sample `Content-Type:application/json`:</summary><code>{"Username": "KebabSeeker33",	"Name": "Charlie",	"Surname": "Lees",	"Email": "charlie.lees@example.com",	"Password": "SecretPassword"}</code></details> | 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)
| GET | `users/count` | IDE+IRA | Retrieves *the count* (total number) of charming users registered. | 🧛[`https://localhost:5001/users/count`](https://localhost:5001/users/count)



---
<sup>1</sup>Access restrictions to endpoints occasionally favour being illustrative (i.e. "See? We can do this like this!") over being the firm design choices of a more complete project.

<sup>2</sup>Logical operators are abbreviated: `+ = AND` `, = OR`

<br>

<p align="center">
  <img src="Doc/kebab2.svg" width="200" />
</p>
<br>

<sub>Some tastefully sourced kebab icons slightly tweaked by [critr](https://github.com/critr) and lovingly made by <a href="https://www.flaticon.com/authors/smashicons" title="Smashicons">Smashicons</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></sub>
