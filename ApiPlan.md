# API Plan

<details>
<summary>1. Authentication & Users</summary>

### User Registration
**POST** `/auth/register`
```
Request:
{
    "email": "user@example.com",
    "nickname": "Speedster99",
    "password": "SecurePass123"
}
```
```
Response:
{
    "message": "User registered successfully"
}
```

### User Login
**POST** `/auth/login`
```
Request:
{
    "email": "user@example.com",
    "password": "SecurePass123"
}
```
```
Response:
{
    "id": 1,
    "nickname": "Speedster99"
}
+ JWT Cookie
```
🔹 Token-based authentication (JWT) will be used for securing the API.

### Get User Profile
**GET** `/users/{id}`
```
Response:
{
    "id": 1,
    "email": "user@example.com",
    "nickname": "Speedster99",
    "description": "Car enthusiast",
    "imageLocation": "/uploads/avatar1.png"
}
```

### Get All Users Profile
**GET** `/users`
```
Response:
{
    [
        "id": 1,
        "email": "user@example.com",
        "nickname": "Speedster99",
        "description": "Car enthusiast",
        "imageLocation": "/uploads/avatar1.png"
    ]
}
```

### Update User Profile
**PUT** `/users/{id}`
```
Request:
{
    "nickname": "Speedster99",
    "description": "Car enthusiast",
    "imageLocation": "/uploads/avatar1.png"
}
```

### Get all meets that User applied to
**Get**  `/users/{id}/events`
```
Response:
[
    {
        "type": "Meet/Race",
        "creatorId": 1,
        "name": "Supercar Sunday",
        "description": "Biggest car meet in the city",
        "location": "Downtown Parking Lot",
        "coordinates": "40.7128,-74.0060",
        "crewId": 3,
        "tags": [
                "Cars N Coffee",
                "Tour"
        ]
    }
]
```
</details>

<details>
<summary>2. Cars</summary>

### Add Car
**POST** `/cars`
```
Request:
{
    "userId": 1,
    "brand": "Nissan",
    "model": "GT-R",
    "engine": "3.8L V6",
    "horsePower": 565,
    "description": "Modified for track racing"
}
```
```
Response:
{
    "id": 101,
    "message": "Car added successfully"
}
```

### Update Car
**POST** `/cars`
```
Request:
{
    "userId": 1,
    "brand": "Nissan",
    "model": "GT-R",
    "engine": "3.8L V6",
    "horsePower": 565,
    "description": "Modified for track racing"
}
```
```
Response:
{
    "id": 101,
    "message": "Car added successfully"
}
```

### Get Cars of a User
**GET** `/users/{id}/cars`
```
Response:
[
    {
        "id": 101,
        "brand": "Nissan",
        "model": "GT-R",
        "horsePower": 565
    }
]
```
</details>

<details>
<summary>3. Car Meets</summary>

### Create a Meet
**POST** `/meets`
```
Request:
{
    "creatorId": 1,
    "name": "Supercar Sunday",
    "description": "Biggest car meet in the city",
    "location": "Downtown Parking Lot",
    "coordinates": "40.7128,-74.0060",
    "crewId": 3,
    "private": false,
    "tags": [
        "Cars N Coffee",
        "Tour"
    ]
}
```
```
Response:
{
    "id": 201,
    "message": "Meet created successfully"
}
```

### Update a Meet
**POST** `/meets/{id}`
```
Request:
{
    "name": "Supercar Sunday",
    "description": "Biggest car meet in the city",
    "location": "Downtown Parking Lot",
    "coordinates": "40.7128,-74.0060",
    "crewId": 3,
    "private": false,
    "tags": [
        "Cars N Coffee",
        "Tour"
    ]
}
```
```
Response:
{
    "id": 201,
    "message": "Meet updated successfully"
}
```

### Get a Meet
**GET** `/meets/{id}`
```
Response:
{
    "id": 201,
    "name": "Supercar Sunday",
    "location": "Downtown Parking Lot",
    "date": "2024-06-20T15:00:00Z",
    "tags": [
        "Cars N Coffee",
        "Tour"
    ]
}
```

### Get All Meets (With Filters)
**GET** `/meets?location=NYC&tag=supercars`
```
Response:
[
    {
        "id": 201,
        "name": "Supercar Sunday",
        "location": "Downtown Parking Lot",
        "date": "2024-06-20T15:00:00Z",
        "tags": [
            "Cars N Coffee",
            "Tour"
        ]
    }
]
```

### Join a Meet
**POST** `/meets/{id}/join`
```
Request:
{
    "userId": 1
}
```
```
Response:
{
    "message": "You have joined the meet."
}
```
</details>

<details>
<summary>4. Races</summary>

### Create a Race
**POST** `/races`
```
Request:
{
    "creatorId": 1,
    "name": "Night Drag Race",
    "raceType": "drag",
    "location": "Airport Strip",
    "coordinates": "40.7128,-74.0060",
    "crewId": 3,
    "private": false
}
```
```
Response:
{
    "id": 301,
    "message": "Race created successfully"
}
```

### Update a Race
**PUT** `/races/{id}`
```
Request:
{
    "name": "Night Drag Race",
    "raceType": "drag",
    "location": "Airport Strip",
    "coordinates": "40.7128,-74.0060",
    "crewId": 3,
    "private": false
}
```
```
Response:
{
    "id": 301,
    "message": "Race created successfully"
}
```

### Get All Races (with filters)
**GET** `/races?location=NYC&raceType=drag`
```
Response:
[
    {
        "name": "Night Drag Race",
        "raceType": "drag",
        "location": "Airport Strip",
        "coordinates": "40.7128,-74.0060",
        "crewId": 3
    }
]
```

### Get a Races
**GET** `/races/{id}`
```
Response:
{
    "name": "Night Drag Race",
    "raceType": "drag",
    "location": "Airport Strip",
    "coordinates": "40.7128,-74.0060",
    "crewId": 3
}
```

### Join a Race
**POST** `/races/{id}/join`
```
Request:
{
    "userId": 1
}
```
```
Response:
{
    "message": "You have joined the race."
}
```
</details>

<details>
<summary>5. Crews</summary>

### Create a Crew
**POST** `/crews`
```
Request:
{
    "name": "Street Kings",
    "description": "Elite racers of NYC",
    "imageLocation": "/uploads/crew1.png"
}
```
```
Response:
{
    "id": 401,
    "message": "Crew created successfully"
}
```

### Update a Crew
**PUT** `/crews/{id}`
```
Request:
{
    "name": "Street Kings",
    "description": "Elite racers of NYC",
    "imageLocation": "/uploads/crew1.png"
}
```
```
Response:
{
    "message": "Crew updated successfully"
}
```

### Add User to Crew
**POST** `/crews/{id}/add`
```
Request:
{
    "userId": 2,
    "rank": "recruiter"
}
```
```
Response:
{
    "message": "User added to the crew."
}
```
</details>
