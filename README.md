# FoodBridge

FoodBridge lets a shop post surplus food, and lets a person or an NGO ask for it. When the shop allocates the listing, the food is shared by priority and each granted request gets a pickup code. The shop confirms the pickup. If someone never collects, closing the listing after the pickup window marks that reservation as a no-show.

The website is an ASP.NET Core MVC app. It talks to an ASP.NET Core Web API. The API uses SQL Server. The projects follow onion architecture: the website does not touch the database, and the domain project has no NuGet packages.

## Projects

- `FoodBridge.Domain` holds the entities and the fair allocator.
- `FoodBridge.Application` holds the use cases, the priority rules, and the repository interfaces.
- `FoodBridge.Infrastructure` holds Entity Framework, Identity, the JWT, and the repositories.
- `FoodBridge.Api` is the HTTP API and Swagger.
- `FoodBridge.Mvc` is the website. It keeps the JWT in a cookie and calls the API.
- `FoodBridge.Tests` holds the unit tests.

## What you need

- .NET 10 SDK
- SQL Server, reachable as `localhost` with Windows authentication
- Two terminals

The API creates the `FoodBridge` database and applies the migrations on startup. In Development it also inserts the demo accounts, the areas, the categories, and one open listing named Evening bread when Sunrise Bakery does not already have one.

## Run it

From the repository folder, set a local signing key. This value stays on your machine. It is not committed.

```powershell
dotnet user-secrets set "Jwt:Key" "FoodBridge-dev-signing-key-change-before-demo-32" --project src/FoodBridge.Api
```

The key must be at least 32 bytes. Use another long value if you prefer.

Start the API:

```powershell
dotnet run --project src/FoodBridge.Api --launch-profile http
```

The API listens on http://localhost:5280. http://localhost:5280/ has no page. Open http://localhost:5280/swagger to call the API directly.

Start the website in a second terminal:

```powershell
dotnet run --project src/FoodBridge.Mvc --launch-profile http
```

Open http://localhost:5011. Times on the website are shown in Lebanon time. The database stores UTC.

Run the tests with:

```powershell
dotnet test
```

## Demo accounts

Every demo account uses the password `Password1!`.

| Email | Role | What is already true |
| --- | --- | --- |
| admin@foodbridge.test | Admin | Can approve shops and NGOs, and can see every shop in the impact report |
| shop@foodbridge.test | Shop | Sunrise Bakery, Hamra Street, approved |
| receiver@foodbridge.test | Receiver | Layla Receiver, an individual, approved |
| ngo@foodbridge.test | NGO | Community Kitchen, 80 people served, approved |
| ngo.pending@foodbridge.test | NGO | Pending Shelter, 12 people served, waiting for approval |

An individual is approved at registration. A shop or an NGO cannot post or request food until an admin approves them. Nobody can register as an admin.

## What each role does

A visitor can browse open listings whose request window is still in the future, and can filter by area and category. An allocated listing stays on that page until the reserved food is collected or the pickup window ends, so a person can open it and join the waitlist.

A receiver or an approved NGO can request food while the listing is open. An individual can ask for at most 2 units. An NGO can ask for at most 10. One account can request a listing only once. After the shop allocates, any food that is left can be reserved directly until the pickup window ends. My Activity shows the requests, the pickup codes, and the reliability score.

A shop posts a listing, allocates it, types a pickup code to confirm collection, and closes the listing. The impact page shows that shop only. An admin can filter the same report by area. Kilos saved are collected units multiplied by the weight of each unit. The end date on the form is included.

An admin approves waiting shops and NGOs.

## How a listing is shared

Allocation scores each request and then grants food from the highest score down. The same listing is saved once: the grants, the reservations, and the pickup codes succeed together or not at all.

The score is the sum of three rules:

- NGO size is the people served, capped at 50. An individual has no size points.
- Recent receipts start at 20 and lose one point for each unit that account collected in the last 7 days. The result is never below 0.
- Reliability starts at 80. The rule adds the score divided by 5.

Equal scores are ordered by the request id, oldest first. A request for more than the role maximum is cut down to that maximum. A request that gets nothing stays not allocated and gets no pickup code.

The reservation limit is chosen with one check: an NGO uses the NGO policy, and every other receiver uses the individual policy.

A no-show subtracts 20 from the reliability score. The account is blocked for 3 days only when the score falls below 40. Collecting food does not change the score.

## Known limitations

Nothing allocates or closes a listing unless the shop presses the button. There is no background worker. If the shop never comes back, open requests stay open and uncollected reservations are not marked as no-shows.

A sold-out listing has a waitlist. Food is offered to the next person only when someone cancels a reservation, in the same save. The offer appears on the notification bell. Pressing it opens the food. If that account has a FoodBridge page open, the same message also appears there immediately. The same live notice is sent for an approval, a request, an allocation, a cancellation, a waitlist change, a pickup, and a no-show. FoodBridge does not send email, SMS, or phone push messages. A missed waitlist offer does not change the reliability score, because the person may not have seen the update in time. A normal reservation still loses 20 points if it is still pending when the shop closes the listing after the pickup window.

A listing can be closed only when the current time is at or after the end of the pickup window. Closing earlier returns 409. Pending reservations become no-shows only at that moment.

A 409 means the save lost a race or the action is no longer allowed, for example two people taking the last unit, or closing too early. The API does not try the same save again. The caller sees the conflict and can refresh.

A listing cannot be edited. The website has no cancel button. `DELETE /api/listings/{id}` cancels an open listing from Swagger.

The impact procedure accepts a date range and an optional area. It does not accept a shop id. The API keeps only the signed-in shop's rows after the procedure runs. An admin sees all of the rows.

## Demo walkthrough

1. Start the API, then the website.
2. Browse the listings without logging in.
3. Log in as Layla Receiver and request 1 unit of an open listing.
4. Log out. Log in as Sunrise Bakery. On the dashboard, allocate that listing. The receiver's My Activity page then shows a pickup code.
5. On the shop dashboard, type that code and confirm the pickup.
6. Log in as the admin. Pending Shelter is waiting under Approvals. Leave it pending if you want to show that an unapproved NGO cannot request food.
7. Open Impact as the shop, then again as the admin. The admin can choose an area. Hamra is area 1.
8. To show a no-show, close a listing only after its pickup window has ended. Closing before that is the 409 case.
