# Kebapi IO Reference

This reference lists all of the endpoints, showing a range of inputs together with expected outputs, plus other relevant data.

The list isn't exhaustive chiefly because this is a demonstration project, and because Markdown isn't the best vehicle for documenting something like this - pushing the MD envelope a bit! However, it should give an excellent picture of what the API does and how it handles the unexpected as well as the expected.

## TOC



<div id="toc">

[Kebapi IO Reference](#kebapi-io-reference)  
 [Dev/Admin](#devadmin)  
  [`dev/createdb`](#devcreatedb)  
   [🔧`https://localhost:5001/dev/createdb`](#httpslocalhost5001devcreatedb)  
  [`dev/dropdb`](#devdropdb)  
   [🔨`https://localhost:5001/dev/dropdb`](#httpslocalhost5001devdropdb)  
  [`dev/resetdb`](#devresetdb)  
   [🛠️`https://localhost:5001/dev/resetdb`](#%EF%B8%8Fhttpslocalhost5001devresetdb)  
  [`dev/resettestdb`](#devresettestdb)  
   [🛠️`https://localhost:5001/dev/resettestdb`](#%EF%B8%8Fhttpslocalhost5001devresettestdb)  
 [Venues](#venues)  
  [`venues/:venueId/distance?originLat=lat&originLng=long`](#venuesvenueiddistanceoriginlatlatoriginlnglong)  
   [🗺️
`https://localhost:5001/venues/1/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](#%EF%B8%8F-httpslocalhost5001venues1distanceoriginlat4042313821277501originlng-37299816289728036)  
   [🗺️
`https://localhost:5001/venues/254/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](#%EF%B8%8F-httpslocalhost5001venues254distanceoriginlat4042313821277501originlng-37299816289728036)  
   [🗺️
`https://localhost:5001/venues/abc/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](#%EF%B8%8F-httpslocalhost5001venuesabcdistanceoriginlat4042313821277501originlng-37299816289728036)  
   [🗺️
`https://localhost:5001/venues/1/distance`](#%EF%B8%8F-httpslocalhost5001venues1distance)  
   [🗺️
`https://localhost:5001/venues/1/distance?originLng=-3.7299816289728036`](#%EF%B8%8F-httpslocalhost5001venues1distanceoriginlng-37299816289728036)  
   [🗺️
`https://localhost:5001/venues/1/distance?originLat=40.42313821277501`](#%EF%B8%8F-httpslocalhost5001venues1distanceoriginlat4042313821277501)  
   [🗺️
`https://localhost:5001/venues/1/distance?originLat=abc&originLng=--99999999999999999`](#%EF%B8%8F-httpslocalhost5001venues1distanceoriginlatabcoriginlng--99999999999999999)  
  [`venues/nearby?originLat=lat&originLng=long`](#venuesnearbyoriginlatlatoriginlnglong)  
   🗺️1.
[`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036`](#%EF%B8%8F1-httpslocalhost5001venuesnearbyoriginlat4042313821277501originlng-37299816289728036)  
   [🗺️1.
`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&withinMetres=3500`](#%EF%B8%8F1-httpslocalhost5001venuesnearbyoriginlat4042313821277501originlng-37299816289728036withinmetres3500)  
   [🗺️1.
`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=1`](#%EF%B8%8F1-httpslocalhost5001venuesnearbyoriginlat4042313821277501originlng-37299816289728036rowcount1)  
   [🗺️1.
`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=3`](#%EF%B8%8F1-httpslocalhost5001venuesnearbyoriginlat4042313821277501originlng-37299816289728036rowcount3)  
   [🗺️1.
`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&startRow=3&rowCount=3`](#%EF%B8%8F1-httpslocalhost5001venuesnearbyoriginlat4042313821277501originlng-37299816289728036startrow3rowcount3)  
  [`venues/:venueId`](#venuesvenueid)  
   [🥙`https://localhost:5001/venues/2`](#httpslocalhost5001venues2)  
   [🥙`https://localhost:5001/venues/300`](#httpslocalhost5001venues300)  
   [🥙`https://localhost:5001/venues/abc`](#httpslocalhost5001venuesabc)  
  [`venues (optional: ?startRow=n&rowCount=n)`](#venues-optional-startrownrowcountn)  
   [🥙1.
`https://localhost:5001/venues`](#1-httpslocalhost5001venues)  
   [🥙2.
`https://localhost:5001/venues?startRow=4`](#2-httpslocalhost5001venuesstartrow4)  
   [🥙3.
`https://localhost:5001/venues?rowCount=2`](#3-httpslocalhost5001venuesrowcount2)  
   [🥙4.
`https://localhost:5001/venues?startRow=6&rowCount=3`](#4-httpslocalhost5001venuesstartrow6rowcount3)  
  [`venues/count`](#venuescount)  
   [🧛`https://localhost:5001/venues/count`](#httpslocalhost5001venuescount)  
 [Users](#users)  
  [`users/auth`](#usersauth)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-1)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-2)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-3)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-4)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-5)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-6)  
   [👮`https://localhost:5001/users/auth`](#httpslocalhost5001usersauth-7)  
  [`users (optional: ?startRow=n&rowCount=n)`](#users-optional-startrownrowcountn)  
   [👪1.
`https://localhost:5001/users`](#1-httpslocalhost5001users)  
   [👪2.
`https://localhost:5001/users?startRow=4`](#2-httpslocalhost5001usersstartrow4)  
   [👪3.
`https://localhost:5001/users?rowCount=2`](#3-httpslocalhost5001usersrowcount2)  
   [👪4.
`https://localhost:5001/users?startRow=3&rowCount=2`](#4-httpslocalhost5001usersstartrow3rowcount2)  
  [`users/find?username=a`](#usersfindusernamea)  
   [👨`https://localhost:5001/users/find?username=MeatyMan`](#httpslocalhost5001usersfindusernamemeatyman)  
   [👨`https://localhost:5001/users/find?username=DontExist`](#httpslocalhost5001usersfindusernamedontexist)  
  [`users/:id`](#usersid)  
   [👩`https://localhost:5001/users/4`](#httpslocalhost5001users4)  
   [👩`https://localhost:5001/users/567`](#httpslocalhost5001users567)  
   [👩`https://localhost:5001/users/abc`](#httpslocalhost5001usersabc)  
  [`users/:id/favourites (optional: ?startRow=n &rowCount=n)`](#usersidfavourites-optional-startrown-rowcountn)  
   [💗1.
`https://localhost:5001/users/2/favourites`](#1-httpslocalhost5001users2favourites)  
   [💗2.
`https://localhost:5001/users/2/favourites?startRow=2`](#2-httpslocalhost5001users2favouritesstartrow2)  
   [💗3.
`https://localhost:5001/users/2/favourites?rowCount=2`](#3-httpslocalhost5001users2favouritesrowcount2)  
   [💗4.
`https://localhost:5001/users/2/favourites?startRow=2&rowCount=1`](#4-httpslocalhost5001users2favouritesstartrow2rowcount1)  
  [`users/:id/favourites/:venueId`](#usersidfavouritesvenueid)  
   [💖`https://localhost:5001/users/1/favourites/1`](#httpslocalhost5001users1favourites1)  
   [💖`https://localhost:5001/users/1/favourites/1`](#httpslocalhost5001users1favourites1-1)  
   [💔`https://localhost:5001/users/1/favourites/1`](#httpslocalhost5001users1favourites1-2)  
   [💔`https://localhost:5001/users/1/favourites/1`](#httpslocalhost5001users1favourites1-3)  
  [`users/:id/status`](#usersidstatus)  
   [❔`https://localhost:5001/users/2/status`](#httpslocalhost5001users2status)  
  [`users/:id/activate`](#usersidactivate)  
   [✔️`https://localhost:5001/users/2/activate`](#%EF%B8%8Fhttpslocalhost5001users2activate)  
  [`users/:id/deactivate`](#usersiddeactivate)  
   [❌`https://localhost:5001/users/2/deactivate`](#httpslocalhost5001users2deactivate)  
  [`users/register`](#usersregister)  
   [🙋`https://localhost:5001/users/register`](#httpslocalhost5001usersregister)  
   [🙋`https://localhost:5001/users/register`](#httpslocalhost5001usersregister-1)  
   [🙋`https://localhost:5001/users/register`](#httpslocalhost5001usersregister-2)  
   [🙋`https://localhost:5001/users/register`](#httpslocalhost5001usersregister-3)  
   [🙋`https://localhost:5001/users/register`](#httpslocalhost5001usersregister-4)  
  [`users/count`](#userscount)  
   [🧛`https://localhost:5001/users/count`](#httpslocalhost5001userscount)

</div>



## Dev/Admin



### `dev/createdb`

#### 🔧[`https://localhost:5001/dev/createdb`](https://localhost:5001/dev/createdb)

`GET`

Create db named in the config connection string if it doesn't exist.

```
"Success"
```

[Jump to TOC](#toc)<br><br>


### `dev/dropdb`

#### 🔨[`https://localhost:5001/dev/dropdb`](https://localhost:5001/dev/dropdb)

`GET`

Drop db named in the config connection string if it exists.

```
"Success"
```

[Jump to TOC](#toc)<br><br>


### `dev/resetdb`

#### 🛠️[`https://localhost:5001/dev/resetdb`](https://localhost:5001/dev/resetdb)

`GET`

Perform drop, perform create, then add schema; all to the db named in the config connection string.

```
"Success"
```

[Jump to TOC](#toc)<br><br>


### `dev/resettestdb`

#### 🛠️[`https://localhost:5001/dev/resettestdb`](https://localhost:5001/dev/resettestdb)

`GET`

Perform drop, perform create, add schema, then fill with sample data; all to the db named in the config connection string.

```
"Success"
```

[Jump to TOC](#toc)<br><br>


## Venues



### `venues/:venueId/distance?originLat=lat&originLng=long`

| Category       | Name      | Type   | Required | Description                              |
| -------------- | --------- | ------ | -------- | ---------------------------------------- |
| Route value    | :venueId  | Int    | Y        | The id of the venue                      |
| Query variable | originLat | Double | Y        | Geographic latitude of the origin point  |
| Query variable | originLng | Double | Y        | Geographic longitude of the origin point |



#### 🗺️ [`https://localhost:5001/venues/1/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](https://localhost:5001/venues/1/distance?originLat=40.42313821277501&originLng=-3.7299816289728036)

`GET`

Get me the distance to the venue with venueId=1 from my current location. With valid origin arguments.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get distance to venue returned a result.",
    "Errors": []
  },
  "ApiVenueDistance": {
    "Id": 1,
    "Name": "Splendid Kebabs",
    "Rating": 4,
    "MainMediaPath": "image\\1.jpg",
    "DistanceInMetres": 1644.3163883895993,
    "DistanceInKilometres": 1.6443163883895993,
    "DistanceInMiles": 1.0217308346690324
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/254/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](https://localhost:5001/venues/254/distance?originLat=40.42313821277501&originLng=-3.7299816289728036)

`GET`

Get me the distance to the venue with **inexistent** venueId=254.

```json
{
  "ApiStatus": {
    "StatusCode": 404,
    "Message": "Get distance to venue did not return a result.",
    "Errors": []
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/abc/distance?originLat=40.42313821277501&originLng=-3.7299816289728036`](https://localhost:5001/venues/abc/distance?originLat=40.42313821277501&originLng=-3.7299816289728036)

`GET`

Get me the distance to the venue with **invalid** venueId=abc.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get distance to venue.",
    "Errors": [
      "Missing an expected integer (greater than 0) argument: id. The value supplied was 'abc'."
    ]
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/1/distance`](https://localhost:5001/venues/1/distance)

`GET`

Get me the distance to the venue with venueId=1. **Missing** originLat, originLng.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get distance to venue.",
    "Errors": [
      "Missing an expected double (between -90 and 90) argument: originLat. The value supplied was ''.",
      "Missing an expected double (between -180 and 180) argument: originLng. The value supplied was ''."
    ]
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/1/distance?originLng=-3.7299816289728036`](https://localhost:5001/venues/1/distance?originLng=-3.7299816289728036)

`GET`

Get me the distance to the venue with venueId=1. **Missing** originLat.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get distance to venue.",
    "Errors": [
      "Missing an expected double (between -90 and 90) argument: originLat. The value supplied was ''."
    ]
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/1/distance?originLat=40.42313821277501`](https://localhost:5001/venues/1/distance?originLat=40.42313821277501)

`GET`

Get me the distance to the venue with venueId=1. **Missing** originLng.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get distance to venue.",
    "Errors": [
      "Missing an expected double (between -180 and 180) argument: originLng. The value supplied was ''."
    ]
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️ [`https://localhost:5001/venues/1/distance?originLat=abc&originLng=--99999999999999999`](https://localhost:5001/venues/1/distance?originLat=abc&originLng=--99999999999999999)

`GET`

Get me the distance to the venue with venueId=1. With **invalid** originLat, originLng.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get distance to venue.",
    "Errors": [
      "Missing an expected double (between -90 and 90) argument: originLat. The value supplied was 'abc'.",
      "Missing an expected double (between -180 and 180) argument: originLng. The value supplied was '--99999999999999999'."
    ]
  },
  "ApiVenueDistance": null
}
```

[Jump to TOC](#toc)<br><br>


### `venues/nearby?originLat=lat&originLng=long`

| Category       | Name         | Type   | Required | Description                                              |
| -------------- | ------------ | ------ | -------- | -------------------------------------------------------- |
| Query variable | originLat    | Double | Y        | Geographic latitude of the origin point                  |
| Query variable | originLng    | Double | Y        | Geographic longitude of the origin point                 |
| Query variable | withinMetres | Double | N        | Maximum distance from the origin point in metres         |
| Query variable | startRow     | Int    | N        | Return results starting at this ordinal row (zero-based) |
| Query variable | rowCount     | Int    | N        | Return this number of rows                               |



#### 🗺️1. [`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036`](https://localhost:44383/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036)

`GET`

Get me all the venues nearby ranked by distance, rating. With valid arguments. No limit on distance. No limits on rows.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues nearby returned a result.",
    "Errors": []
  },
  "ApiVenueDistances": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg",
      "DistanceInMetres": 1644.3163883895993,
      "DistanceInKilometres": 1.6443163883895993,
      "DistanceInMiles": 1.0217308346690324
    },
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg",
      "DistanceInMetres": 2390.8930570150287,
      "DistanceInKilometres": 2.390893057015029,
      "DistanceInMiles": 1.4856320693493925
    },
    {
      "Id": 5,
      "Name": "The Dirty One",
      "Rating": 5,
      "MainMediaPath": "image\\5.jpg",
      "DistanceInMetres": 3090.9239679400775,
      "DistanceInKilometres": 3.0909239679400775,
      "DistanceInMiles": 1.920611111073877
    },
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg",
      "DistanceInMetres": 3793.0803541090168,
      "DistanceInKilometres": 3.7930803541090166,
      "DistanceInMiles": 2.3569108618847285
    },
    {
      "Id": 8,
      "Name": "Star Kebab",
      "Rating": 4,
      "MainMediaPath": "image\\8.jpg",
      "DistanceInMetres": 3807.58771052944,
      "DistanceInKilometres": 3.80758771052944,
      "DistanceInMiles": 2.365925315239899
    },
    {
      "Id": 6,
      "Name": "Bodrum Conundrum",
      "Rating": 2,
      "MainMediaPath": "image\\6.jpg",
      "DistanceInMetres": 4558.477958690741,
      "DistanceInKilometres": 4.558477958690741,
      "DistanceInMiles": 2.8325068839792737
    },
    {
      "Id": 4,
      "Name": "The Rotisserie",
      "Rating": 3,
      "MainMediaPath": "image\\4.jpg",
      "DistanceInMetres": 5260.325289152695,
      "DistanceInKilometres": 5.2603252891526955,
      "DistanceInMiles": 3.2686145964770086
    },
    {
      "Id": 9,
      "Name": "Kebab Slab",
      "Rating": 3,
      "MainMediaPath": "image\\9.jpg",
      "DistanceInMetres": 5266.337874929758,
      "DistanceInKilometres": 5.266337874929758,
      "DistanceInMiles": 3.2723506440697316
    }
  ]
}

```

[Jump to TOC](#toc)<br><br>


#### 🗺️1. [`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&withinMetres=3500`](https://localhost:44383/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&withinMetres=3500)

`GET`

Get me all the venues nearby ranked by distance, rating, and within 3500 metres of my location.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues nearby returned a result.",
    "Errors": []
  },
  "ApiVenueDistances": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg",
      "DistanceInMetres": 1644.3163883895993,
      "DistanceInKilometres": 1.6443163883895993,
      "DistanceInMiles": 1.0217308346690324
    },
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg",
      "DistanceInMetres": 2390.8930570150287,
      "DistanceInKilometres": 2.390893057015029,
      "DistanceInMiles": 1.4856320693493925
    },
    {
      "Id": 5,
      "Name": "The Dirty One",
      "Rating": 5,
      "MainMediaPath": "image\\5.jpg",
      "DistanceInMetres": 3090.9239679400775,
      "DistanceInKilometres": 3.0909239679400775,
      "DistanceInMiles": 1.920611111073877
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️1. [`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=1`](https://localhost:44383/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=1)

`GET`

Get me the closest, best-ranked venue to my location. (I.e. get me all the venues nearby ranked by distance, rating, but limit to only 1 result.)

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues nearby returned a result.",
    "Errors": []
  },
  "ApiVenueDistances": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg",
      "DistanceInMetres": 1644.3163883895993,
      "DistanceInKilometres": 1.6443163883895993,
      "DistanceInMiles": 1.0217308346690324
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️1. [`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=3`](https://localhost:44383/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&rowCount=3)

`GET`

Get me the 3 closest venues to my location, ranked by distance, rating. (I.e. get me all the venues nearby ranked by distance, rating, but limit to 3 results.)

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues nearby returned a result.",
    "Errors": []
  },
  "ApiVenueDistances": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg",
      "DistanceInMetres": 1644.3163883895993,
      "DistanceInKilometres": 1.6443163883895993,
      "DistanceInMiles": 1.0217308346690324
    },
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg",
      "DistanceInMetres": 2390.8930570150287,
      "DistanceInKilometres": 2.390893057015029,
      "DistanceInMiles": 1.4856320693493925
    },
    {
      "Id": 5,
      "Name": "The Dirty One",
      "Rating": 5,
      "MainMediaPath": "image\\5.jpg",
      "DistanceInMetres": 3090.9239679400775,
      "DistanceInKilometres": 3.0909239679400775,
      "DistanceInMiles": 1.920611111073877
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🗺️1. [`https://localhost:5001/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&startRow=3&rowCount=3`](https://localhost:44383/venues/nearby?originLat=40.42313821277501&originLng=-3.7299816289728036&startRor=2&rowCount=3)

`GET`

Get me the *next* 3 closest venues to my location, ranked by distance, rating. (Follows on from the previous example.)

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues nearby returned a result.",
    "Errors": []
  },
  "ApiVenueDistances": [
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg",
      "DistanceInMetres": 3793.0803541090168,
      "DistanceInKilometres": 3.7930803541090166,
      "DistanceInMiles": 2.3569108618847285
    },
    {
      "Id": 8,
      "Name": "Star Kebab",
      "Rating": 4,
      "MainMediaPath": "image\\8.jpg",
      "DistanceInMetres": 3807.58771052944,
      "DistanceInKilometres": 3.80758771052944,
      "DistanceInMiles": 2.365925315239899
    },
    {
      "Id": 6,
      "Name": "Bodrum Conundrum",
      "Rating": 2,
      "MainMediaPath": "image\\6.jpg",
      "DistanceInMetres": 4558.477958690741,
      "DistanceInKilometres": 4.558477958690741,
      "DistanceInMiles": 2.8325068839792737
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


### `venues/:venueId`

| Category    | Name     | Type | Required                                   | Description         |
| ----------- | -------- | ---- | ------------------------------------------ | ------------------- |
| Route value | :venueId | Int  | Y (If missing could invoke the `venues/`.) | The id of the venue |

#### 🥙[`https://localhost:5001/venues/2`](https://localhost:5001/venues/2)

`GET`

Get me the details of the venue with venueId=2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venue returned a result.",
    "Errors": []
  },
  "ApiVenue": {
    "Id": 2,
    "Name": "The Kebaberie",
    "GeoLat": 40.422072,
    "GeoLng": -3.685308,
    "Address": "101 Santa Monica Way, Madrid",
    "Rating": 3,
    "MainMediaPath": "image\\2.jpg"
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 🥙[`https://localhost:5001/venues/300`](https://localhost:5001/venues/300)

`GET`

Get me the details of the venue with **inexistent** venueId=300.

```json
{
  "ApiStatus": {
    "StatusCode": 404,
    "Message": "Get venue did not return a result.",
    "Errors": []
  },
  "ApiVenue": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🥙[`https://localhost:5001/venues/abc`](https://localhost:5001/venues/abc)

`GET`

Get me the details of the venue with **invalid** venueId=abc.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get venue.",
    "Errors": [
      "Missing an expected integer (greater than 0) argument: id. The value supplied was 'abc'."
    ]
  },
  "ApiVenue": null
}
```

[Jump to TOC](#toc)<br><br>


### `venues (optional: ?startRow=n&rowCount=n)`

| Category       | Name     | Type | Required | Description                                              |
| -------------- | -------- | ---- | -------- | -------------------------------------------------------- |
| Query variable | startRow | Int  | N        | Return results starting at this ordinal row (zero-based) |
| Query variable | rowCount | Int  | N        | Return this number of rows                               |


#### 🥙1. [`https://localhost:5001/venues`](https://localhost:5001/venues)

`GET`

Get me all the venues you have. (Config specifies row count limit when none is given.)

```
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "GeoLat": 40.427953,
      "GeoLng": -3.711658,
      "Address": "42 Bla Avenue, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg"
    },
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "GeoLat": 40.422072,
      "GeoLng": -3.685308,
      "Address": "101 Santa Monica Way, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg"
    },
    {
      "Id": 3,
      "Name": "Meats Peeps",
      "GeoLat": 40.382680,
      "GeoLng": -3.668768,
      "Address": "276 Rita St, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\3.jpg"
    },
    {
      "Id": 4,
      "Name": "The Rotisserie",
      "GeoLat": 40.376403,
      "GeoLng": -3.719854,
      "Address": "7 Rick Road, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\4.jpg"
    },
    {
      "Id": 5,
      "Name": "The Dirty One",
      "GeoLat": 40.428475,
      "GeoLng": -3.694234,
      "Address": "10 Banana Place, Madrid",
      "Rating": 5,
      "MainMediaPath": "image\\5.jpg"
    },
    {
      "Id": 6,
      "Name": "Bodrum Conundrum",
      "GeoLat": 40.401442,
      "GeoLng": -3.684389,
      "Address": "55 High Five Drive, Madrid",
      "Rating": 2,
      "MainMediaPath": "image\\6.jpg"
    },
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "GeoLat": 40.428606,
      "GeoLng": -3.702731,
      "Address": "11b Indy Place, Madrid",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg"
    },
    {
      "Id": 8,
      "Name": "Star Kebab",
      "GeoLat": 40.395886,
      "GeoLng": -3.702757,
      "Address": "222 Crispy Crescent, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\8.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🥙2. [`https://localhost:5001/venues?startRow=4`](https://localhost:5001/venues?startRow=4)

`GET`

Get me all the venues you have, starting at row 4 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 5,
      "Name": "The Dirty One",
      "GeoLat": 40.428475,
      "GeoLng": -3.694234,
      "Address": "10 Banana Place, Madrid",
      "Rating": 5,
      "MainMediaPath": "image\\5.jpg"
    },
    {
      "Id": 6,
      "Name": "Bodrum Conundrum",
      "GeoLat": 40.401442,
      "GeoLng": -3.684389,
      "Address": "55 High Five Drive, Madrid",
      "Rating": 2,
      "MainMediaPath": "image\\6.jpg"
    },
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "GeoLat": 40.428606,
      "GeoLng": -3.702731,
      "Address": "11b Indy Place, Madrid",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg"
    },
    {
      "Id": 8,
      "Name": "Star Kebab",
      "GeoLat": 40.395886,
      "GeoLng": -3.702757,
      "Address": "222 Crispy Crescent, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\8.jpg"
    },
    {
      "Id": 9,
      "Name": "Kebab Slab",
      "GeoLat": 40.389806,
      "GeoLng": -3.685848,
      "Address": "5 Five Drive, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\9.jpg"
    },
    {
      "Id": 10,
      "Name": "Turku Kebabi",
      "GeoLat": 40.435009,
      "GeoLng": -3.650976,
      "Address": "21B Baker Street, Madrid",
      "Rating": 5,
      "MainMediaPath": "image\\10.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🥙3. [`https://localhost:5001/venues?rowCount=2`](https://localhost:5001/venues?rowCount=2)

`GET`

Of all the venues you have, get me a maximum of 2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 1,
      "Name": "Splendid Kebabs",
      "GeoLat": 40.427953,
      "GeoLng": -3.711658,
      "Address": "42 Bla Avenue, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\1.jpg"
    },
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "GeoLat": 40.422072,
      "GeoLng": -3.685308,
      "Address": "101 Santa Monica Way, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 🥙4. [`https://localhost:5001/venues?startRow=6&rowCount=3`](https://localhost:5001/venues?startRow=6&rowCount=3)

`GET`

Get me the next 3 venues you have, starting at row 6 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get venues returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 7,
      "Name": "Korner Kebab",
      "GeoLat": 40.428606,
      "GeoLng": -3.702731,
      "Address": "11b Indy Place, Madrid",
      "Rating": 1,
      "MainMediaPath": "image\\7.jpg"
    },
    {
      "Id": 8,
      "Name": "Star Kebab",
      "GeoLat": 40.395886,
      "GeoLng": -3.702757,
      "Address": "222 Crispy Crescent, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\8.jpg"
    },
    {
      "Id": 9,
      "Name": "Kebab Slab",
      "GeoLat": 40.389806,
      "GeoLng": -3.685848,
      "Address": "5 Five Drive, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\9.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


### `venues/count`

#### 🧛[`https://localhost:5001/venues/count`](https://localhost:5001/venues/count)

`GET`

Get me *the count* of all the venues. That's how important he is.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Got count.",
    "Errors": []
  },
  "ApiAffectedRows": {
    "Count": 5
  }
}
```

[Jump to TOC](#toc)<br><br>


## Users


###  `users/auth`

#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Authenticate an existing (registered) user using their username and password.

```json
{
    "UsernameOrEmail": "Babs",
    "Password": "lucy1"
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Authentication succeeded.",
    "Errors": []
  },
  "ApiSecurityToken": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJCYWJzIiwianRpIjoiNjM4Njc5MGMtM2FlNi00YzRhLTgzYzAtNTY5ZGE2MjM1NTRlIiwidXNlcm5hbWUiOiJCYWJzIiwiaWQiOiIyIiwiZGlzcGxheW5hbWUiOiJMdWN5IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTYyNzU4MTIxNCwiaXNzIjoiaHR0cHM6Ly9hcGlzaXRlLmNvbSIsImF1ZCI6Imh0dHBzOi8vYXBpc2l0ZS5jb20ifQ.o4Ocavhr69c0c9c_YroENCSmJjClMN3myV397Qm8o8M",
    "Expires": "2021-07-29T17:53:34Z"
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Authenticate an existing (registered) user using their username and an **incorrect** password.

```json
{
    "UsernameOrEmail": "Babs",
    "Password": "WrongPassword"
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 401,
    "Message": "Authentication failed.",
    "Errors": []
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Authenticate an existing (registered) user using their **email** (instead of username) and password.

```json
{
    "UsernameOrEmail": "babs@matthews.co.uk",
    "Password": "lucy1"
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Authentication succeeded.",
    "Errors": []
  },
  "ApiSecurityToken": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJCYWJzIiwianRpIjoiNDJkNGUzMGQtNjk1Ny00ZjJlLTkwYmItZGQ0MWUwZGUyNTBjIiwidXNlcm5hbWUiOiJCYWJzIiwiaWQiOiIyIiwiZGlzcGxheW5hbWUiOiJMdWN5IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTYyNzU4MTYzOSwiaXNzIjoiaHR0cHM6Ly9hcGlzaXRlLmNvbSIsImF1ZCI6Imh0dHBzOi8vYXBpc2l0ZS5jb20ifQ.dr7ZMjlnATFofrlEZrSG3FTryz1go6q7A3UWRGi471U",
    "Expires": "2021-07-29T18:00:39Z"
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Attempt to authenticate with **missing** POST data.

```json

```


```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky login request received. Check the request method and body.",
    "Errors": null
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Attempt to authenticate with **incorrect** POST data.

```json
{
    "HousePrices": "up 46%",
    "FavouriteColour": "Pink"
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky info received.",
    "Errors": [
      "Missing needed info: UsernameOrEmail, Password."
    ]
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Authenticate an existing (registered) user by email with **missing** password in POST data.

```json
{
    "UsernameOrEmail": "babs@matthews.co.uk"
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky info received.",
    "Errors": [
      "Missing needed info: Password."
    ]
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type application/json`

Authenticate an existing (registered) user by email with an error in JSON POST data (extra comma at the end of second field).

```json
{
    "UsernameOrEmail": "babs@matthews.co.uk",
    "Password": "lucy1",
}
```


```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky login request received. Check the request method and body.",
    "Errors": null
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👮[`https://localhost:5001/users/auth`](https://localhost:5001/users/auth)

`POST Content-Type multipart/form-data`

Authenticate an existing (registered) user by email with **wrong** body type, e.g. Multipart Form.

```
HEADER				VALUE
UsernameOrEmail		babs@matthews.co.uk
Password			lucy1
```


```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky login request received. Check the request method and body.",
    "Errors": null
  },
  "ApiSecurityToken": null
}
```

[Jump to TOC](#toc)<br><br>


### `users (optional: ?startRow=n&rowCount=n)`

| Category       | Name     | Type | Required | Description                                              |
| -------------- | -------- | ---- | -------- | -------------------------------------------------------- |
| Query variable | startRow | Int  | N        | Return results starting at this ordinal row (zero-based) |
| Query variable | rowCount | Int  | N        | Return this number of rows                               |


#### 👪1. [`https://localhost:5001/users`](https://localhost:5001/users)

`GET`

Get me all the users you have. (Config specifies row count limit when none is given.)

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get users returned a result.",
    "Errors": []
  },
  "ApiUser": [
    {
      "Id": 1,
      "Username": "aard",
      "Name": "Bob",
      "Surname": "Smithers",
      "Email": "aard@smithers.com"
    },
    {
      "Id": 2,
      "Username": "Babs",
      "Name": "Lucy",
      "Surname": "Matthews",
      "Email": "babs@matthews.co.uk"
    },
    {
      "Id": 3,
      "Username": "MeatyMan",
      "Name": "Percy",
      "Surname": "Archibald-Hyde",
      "Email": "meatyman@archibald-hyde.eu"
    },
    {
      "Id": 4,
      "Username": "kAb0000B",
      "Name": "Farquhar",
      "Surname": "Rogers",
      "Email": "kAb0000B@rogers.me"
    },
    {
      "Id": 5,
      "Username": "ItsGigi",
      "Name": "Gigi",
      "Surname": "McInactive-User",
      "Email": "gigi@gmail.com"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 👪2. [`https://localhost:5001/users?startRow=4`](https://localhost:5001/users?startRow=4)

`GET`

Get me all the users you have, starting at row 4 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get users returned a result.",
    "Errors": []
  },
  "ApiUser": [
    {
      "Id": 5,
      "Username": "ItsGigi",
      "Name": "Gigi",
      "Surname": "McInactive-User",
      "Email": "gigi@gmail.com"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 👪3. [`https://localhost:5001/users?rowCount=2`](https://localhost:5001/users?rowCount=2)

`GET`

Of all the users you have, get me a maximum of 2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get users returned a result.",
    "Errors": []
  },
  "ApiUser": [
    {
      "Id": 1,
      "Username": "aard",
      "Name": "Bob",
      "Surname": "Smithers",
      "Email": "aard@smithers.com"
    },
    {
      "Id": 2,
      "Username": "Babs",
      "Name": "Lucy",
      "Surname": "Matthews",
      "Email": "babs@matthews.co.uk"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 👪4. [`https://localhost:5001/users?startRow=3&rowCount=2`](https://localhost:5001/users?startRow=3&rowCount=2)

`GET`

Get me the next 2 users you have, starting at row 3 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get users returned a result.",
    "Errors": []
  },
  "ApiUser": [
    {
      "Id": 4,
      "Username": "kAb0000B",
      "Name": "Farquhar",
      "Surname": "Rogers",
      "Email": "kAb0000B@rogers.me"
    },
    {
      "Id": 5,
      "Username": "ItsGigi",
      "Name": "Gigi",
      "Surname": "McInactive-User",
      "Email": "gigi@gmail.com"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


### `users/find?username=a`

| Category       | Name     | Type   | Required | Description           |
| -------------- | -------- | ------ | -------- | --------------------- |
| Query variable | username | String | Y        | The username to find. |

#### 👨[`https://localhost:5001/users/find?username=MeatyMan`](https://localhost:5001/users/find?username=MeatyMan)

`GET`

Find me the existing (registered) user with username=MeatyMan.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user by username returned a result.",
    "Errors": []
  },
  "ApiUser": {
    "Id": 3,
    "Username": "MeatyMan",
    "Name": "Percy",
    "Surname": "Archibald-Hyde",
    "Email": "meatyman@archibald-hyde.eu"
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 👨[`https://localhost:5001/users/find?username=DontExist`](https://localhost:5001/users/find?username=DontExist)

`GET`

Find me the **inexistent** user with username=DontExist.

```json
{
  "ApiStatus": {
    "StatusCode": 404,
    "Message": "Get user by username did not return a result.",
    "Errors": []
  },
  "ApiUser": null
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id`

| Category    | Name | Type | Required                              | Description        |
| ----------- | ---- | ---- | ------------------------------------- | ------------------ |
| Route value | :id  | Int  | Y (If missing could invoke `users/`.) | The id of the user |


#### 👩[`https://localhost:5001/users/4`](https://localhost:5001/users/4)

`GET`

Get me the details of the user with id=4.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user returned a result.",
    "Errors": []
  },
  "ApiUser": {
    "Id": 4,
    "Username": "kAb0000B",
    "Name": "Farquhar",
    "Surname": "Rogers",
    "Email": "kAb0000B@rogers.me"
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 👩[`https://localhost:5001/users/567`](https://localhost:5001/users/567)

`GET`

Get me the details of the user with **inexistent** id=567.

```json
{
  "ApiStatus": {
    "StatusCode": 404,
    "Message": "Get user did not return a result.",
    "Errors": []
  },
  "ApiUser": null
}
```

[Jump to TOC](#toc)<br><br>


#### 👩[`https://localhost:5001/users/abc`](https://localhost:5001/users/abc)

`GET`

Get me the details of the user with **invalid** id=abc.

```json
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Cannot invoke Get user.",
    "Errors": [
      "Missing an expected integer (greater than 0) argument: id. The value supplied was 'abc'."
    ]
  },
  "ApiUser": null
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id/favourites (optional: ?startRow=n &rowCount=n)`

| Category       | Name     | Type | Required | Description                                              |
| -------------- | -------- | ---- | -------- | -------------------------------------------------------- |
| Route value    | :id      | Int  | Y        | The id of the user                                       |
| Query variable | startRow | Int  | N        | Return results starting at this ordinal row (zero-based) |
| Query variable | rowCount | Int  | N        | Return this number of rows                               |


#### 💗1. [`https://localhost:5001/users/2/favourites`](https://localhost:5001/users/2/favourites)

`GET`

Get me all the favourite venues you have for the user with id=2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user favourites returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 3,
      "Name": "Meats Peeps",
      "GeoLat": 40.382680,
      "GeoLng": -3.668768,
      "Address": "276 Rita St, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\3.jpg"
    },
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "GeoLat": 40.422072,
      "GeoLng": -3.685308,
      "Address": "101 Santa Monica Way, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg"
    },
    {
      "Id": 4,
      "Name": "The Rotisserie",
      "GeoLat": 40.376403,
      "GeoLng": -3.719854,
      "Address": "7 Rick Road, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\4.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 💗2. [`https://localhost:5001/users/2/favourites?startRow=2`](https://localhost:5001/users/2/favourites?startRow=2)

`GET`

Get me all the favourite venues you have for the user with id=2, starting at row 2 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user favourites returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 4,
      "Name": "The Rotisserie",
      "GeoLat": 40.376403,
      "GeoLng": -3.719854,
      "Address": "7 Rick Road, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\4.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 💗3. [`https://localhost:5001/users/2/favourites?rowCount=2`](https://localhost:5001/users/2/favourites?rowCount=2)

`GET`

Get me up to 2 of the favourite venues you have for the user with id=2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user favourites returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 3,
      "Name": "Meats Peeps",
      "GeoLat": 40.382680,
      "GeoLng": -3.668768,
      "Address": "276 Rita St, Madrid",
      "Rating": 4,
      "MainMediaPath": "image\\3.jpg"
    },
    {
      "Id": 2,
      "Name": "The Kebaberie",
      "GeoLat": 40.422072,
      "GeoLng": -3.685308,
      "Address": "101 Santa Monica Way, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\2.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


#### 💗4. [`https://localhost:5001/users/2/favourites?startRow=2&rowCount=1`](https://localhost:5001/users/2/favourites?startRow=2&rowCount=1)

`GET`

Get me the 3rd favourite venue of the user with id=2. I.e. get me at most 1 of the favourite venues you have for the user with id=2, starting at row 2 (zero-based).

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user favourites returned a result.",
    "Errors": []
  },
  "ApiVenue": [
    {
      "Id": 4,
      "Name": "The Rotisserie",
      "GeoLat": 40.376403,
      "GeoLng": -3.719854,
      "Address": "7 Rick Road, Madrid",
      "Rating": 3,
      "MainMediaPath": "image\\4.jpg"
    }
  ]
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id/favourites/:venueId`

| Category    | Name     | Type | Required                                             | Description         |
| ----------- | -------- | ---- | ---------------------------------------------------- | ------------------- |
| Route value | :id      | Int  | Y                                                    | The id of the user  |
| Route value | :venueId | Int  | Y (If missing could invoke `users/:id/favourites/`.) | The id of the venue |


#### 💖[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)

`POST Content-Type application/json`

Add the venue with venueId=1 to the list of favourite venues of the user with id=1. The venue is not already in the favourites.

Empty POST body is **expected**.

```json
```


```
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Favourite added.",
    "Errors": []
  },
  "ApiAffectedId": {
    "Value": 7
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 💖[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)

`POST Content-Type application/json`

Add the venue with venueId=1 to the list of favourite venues of the user with id=1. The venue is already in the favourites.

Empty POST body is **expected**.

```json

```


```
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Favourite already exists.",
    "Errors": []
  },
  "ApiAffectedId": {
    "Value": null
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 💔[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)

`DELETE`

Remove the venue with venueId=1 from the list of favourite venues of the user with id=1. The venue is already in the favourites.

Empty DELETE body is **expected**.

```
```


```
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Favourite removed.",
    "Errors": []
  },
  "ApiAffectedId": {
    "Value": 7
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 💔[`https://localhost:5001/users/1/favourites/1`](https://localhost:5001/users/1/favourites/1)

`DELETE`

Remove the venue with venueId=1 from the list of favourite venues of the user with id=1. The venue is not in the favourites.

Empty DELETE body is **expected**.

```

```


```
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Favourite does not exist.",
    "Errors": []
  },
  "ApiAffectedId": {
    "Value": null
  }
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id/status`

| Category    | Name | Type | Required | Description        |
| ----------- | ---- | ---- | -------- | ------------------ |
| Route value | :id  | Int  | Y        | The id of the user |


#### ❔[`https://localhost:5001/users/2/status`](https://localhost:5001/users/2/status)

`GET`

Get me the account status of the user with id=2.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Get user account status returned a result.",
    "Errors": []
  },
  "ApiUserAccountStatus": {
    "Id": 2,
    "Status": "Active"
  }
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id/activate`

| Category    | Name | Type | Required | Description        |
| ----------- | ---- | ---- | -------- | ------------------ |
| Route value | :id  | Int  | Y        | The id of the user |


#### ✔️[`https://localhost:5001/users/2/activate`](https://localhost:5001/users/2/activate)

`PATCH`

Set the account status of the user with id=2 to 'Active'.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Activate user succeeded.",
    "Errors": []
  },
  "ApiAffectedRows": {
    "Count": 1
  }
}
```

[Jump to TOC](#toc)<br><br>


### `users/:id/deactivate`

| Category    | Name | Type | Required | Description        |
| ----------- | ---- | ---- | -------- | ------------------ |
| Route value | :id  | Int  | Y        | The id of the user |


#### ❌[`https://localhost:5001/users/2/deactivate`](https://localhost:5001/users/2/deactivate)

`PATCH`

Set the account status of the user with id=2 to 'Inactive'.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Deactivate user succeeded.",
    "Errors": []
  },
  "ApiAffectedRows": {
    "Count": 1
  }
}
```

[Jump to TOC](#toc)<br><br>


### `users/register`

#### 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)

`POST Content-Type application/json`

Register a **new** (unregistered) user with valid details.

```
{
	"Username": "KebabSeeker33",
	"Name": "Charlie",
	"Surname": "Lees",
	"Email": "charlie.lees@example.com",
	"Password": "SecretPassword"
}
```


```
{
  "ApiStatus": {
    "StatusCode": 201,
    "Message": "User registered.",
    "Errors": []
  },
  "ApiAffectedId": {
    "Value": 6
  }
}
```

[Jump to TOC](#toc)<br><br>


#### 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)

`POST Content-Type application/json`

Register a new (unregistered) user with **invalid** details: username already exists.

```
{
	"Username": "Babs",
	"Name": "Jessie",
	"Surname": "Smith",
	"Email": "jessie92@example.com",
	"Password": "SecretPasswordJessie"
}
```


```
{
  "ApiStatus": {
    "StatusCode": 422,
    "Message": "User is already registered.",
    "Errors": []
  },
  "ApiAffectedId": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)

`POST Content-Type application/json`

Register a new (unregistered) user with **invalid** details: email already exists.

```
{
	"Username": "XtraSAUCE",
	"Name": "Vanessa",
	"Surname": "Johnson",
	"Email": "babs@matthews.co.uk",
	"Password": "SecretPasswordVanessa"
}
```


```
{
  "ApiStatus": {
    "StatusCode": 422,
    "Message": "User is already registered.",
    "Errors": []
  },
  "ApiAffectedId": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)

`POST Content-Type application/json`

Register a new (unregistered) user with **invalid** details. Username too short.

```
{
	"Username": "Mi",
	"Name": "Mika",
	"Surname": "Michaels",
	"Email": "mim@example.com",
	"Password": "SecretPasswordMika"
}
```


```
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky info received.",
    "Errors": [
      "Username must be longer. At least 3 in length."
    ]
  },
  "ApiAffectedId": null
}
```

[Jump to TOC](#toc)<br><br>


#### 🙋[`https://localhost:5001/users/register`](https://localhost:5001/users/register)

`POST Content-Type application/json`

Register a new (unregistered) user with **invalid** details: username too short, email not valid, password too short.

```
{
	"Username": "MT",
	"Name": "Moses",
	"Surname": "Fletcher",
	"Email": "www.apple.com",
	"Password": "m0535"
}
```


```
{
  "ApiStatus": {
    "StatusCode": 400,
    "Message": "Wonky info received.",
    "Errors": [
      "Email doesn't look right.",
      "Username must be longer. At least 3 in length.",
      "Password must be longer. At least 8 in length."
    ]
  },
  "ApiAffectedId": null
}
```

[Jump to TOC](#toc)<br><br>


### `users/count`

#### 🧛[`https://localhost:5001/users/count`](https://localhost:5001/users/count)

`GET`

Get me *the count* of all the users. That's how important he is.

```json
{
  "ApiStatus": {
    "StatusCode": 200,
    "Message": "Got count.",
    "Errors": []
  },
  "ApiAffectedRows": {
    "Count": 6
  }
}
```

[Jump to TOC](#toc)<br><br>
