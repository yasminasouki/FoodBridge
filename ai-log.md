# AI log

I used Cursor to draft this project from the approved proposal. I kept the business rules from that proposal and from my mentor's notes. I did not let the tool add the features that were cut, such as editing a listing, delivery, payments, notifications, a background worker, or a waitlist.

## Rules I kept

- A listing closes only at or after the end of the pickup window. Uncollected reservations become no-shows then, not when the shop allocates.
- If the shop does not press Allocate or Close, nothing else does.
- One person can request a listing once. One claim request has at most one reservation. Both rules are unique indexes.
- A conflict on `RowVersion`, or a duplicate request, returns 409. The code does not retry that save.
- An NGO uses the NGO reservation policy. Any other receiver uses the individual policy. The choice is that one check, not a dependency-injection key.
- An individual can request at most 2 units. An NGO can request at most 10.
- Priority is NGO size capped at 50, plus recent receipts starting at 20, plus the reliability score divided by 5.
- Reliability starts at 80. A no-show subtracts 20. The account is blocked for 3 days only when the score falls below 40.

## What I checked

I ran the unit tests and walked the website with the demo accounts.

- A receiver can request an open listing. Asking again returns a conflict.
- A shop can post a listing, allocate it, and confirm a pickup code. A wrong code is rejected. Confirming the same code twice is a conflict.
- Closing before the pickup window ends is rejected. Closing after it ends marks the pending reservations as no-shows.
- Two reservations of the last unit do not both succeed.
- A receiver cannot open the impact report. A shop sees only that shop. An admin sees the rows and can filter by area.
- An NGO that is still waiting cannot request food. An admin can approve that account from the website.
- The API root has no page. Swagger is at `/swagger`.

The local database also contains listings I created while testing. A fresh database gets the demo users and one Evening bread listing.

A waitlist was added after the first version. It offers released food only when a reservation is cancelled, and it tells the person on the website. A missed waitlist offer does not change the reliability score. There is still no background worker, no email, and no phone message.
