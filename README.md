# PeakWear

A full-stack e-commerce app for athletic wear — .NET 10 API, Angular 22 frontend, PostgreSQL, JWT auth, and an AI-powered size recommender.

I work in this stack professionally. I built PeakWear to have something end-to-end I could point at, and to work with LLM integration properly rather than just reading about it. That last part was genuinely new to me and it's where most of the interesting problems turned up. Payments turned out to be the other one — I'd assumed "take card, get money" and found a distributed systems problem underneath.


Product detail with AI size recommendation:
![Product detail with AI size recommendation](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/product_detail.jpg)

---

## Why this project

Most portfolio apps stop at CRUD. I wanted the problems that only show up in a real domain.

**Product variants.** A pair of leggings isn't one sellable thing — it's nine, one per colour and size, each with its own SKU and stock count. Getting that wrong means rewriting the cart, checkout and every stock check later, so it was the first thing I got right.

**AI that isn't a gimmick.** A chatbot on a clothing store answers questions the FAQ already covers. Size recommendation solves an actual problem: wrong size is the most common reason for apparel returns. It sits behind a provider-agnostic interface, validates what the model returns against sizes that really exist, and falls back to a deterministic calculation when the API is unavailable.

**Money and stock that behave correctly.** Order lines snapshot the price at purchase, so repricing a product doesn't rewrite history. Checkout decrements stock inside a transaction, with a concurrency token catching two people racing for the last item.

**Payments that survive a closed tab.** An order isn't paid because the browser said so. It's paid because Stripe told the server, over a signed webhook, and the server can prove it hasn't already processed that message.

---

## Stack

**Backend**
- .NET 10 / C# 14, ASP.NET Core Web API
- EF Core 10 for writes, CRUD and migrations; Dapper available for complex reads
- PostgreSQL 16
- JWT auth with BCrypt password hashing
- Scrutor for DI auto-registration
- Hosted service for background stock release

**Frontend**
- Angular 22 — zoneless, signal-first
- NgRx SignalStore
- Angular Material
- Reactive forms
- Stripe Elements

**Integrations**
- Stripe (test mode) behind a provider-agnostic interface
- Groq (`openai/gpt-oss-20b`) behind a provider-agnostic interface

---

## How it's structured

```
api/
├── PeakWear.Api/       Controllers, DI wiring, HTTP pipeline, background jobs
├── PeakWear.Core/      Models, services, interfaces — no dependencies
└── PeakWear.Data/      Repositories, DbContext, migrations, Stripe and AI clients

ui/src/app/
├── core/
│   ├── auth/           Token service, HTTP interceptor, route guard
│   └── stripe/         Stripe.js loader
├── modules/
│   ├── login/          Auth store, login and register
│   ├── products/       Product store, list, detail, size recommender
│   ├── cart/           Cart store and page
│   ├── orders/         Checkout, payment, confirmation, order history
│   ├── account/        Profile, addresses, preferences
│   └── home/           Landing page
└── shared/
```

The API references run one way: `Api → Core`, `Api → Data`, `Data → Core`. Core references nothing, so business logic can't accidentally depend on the database — that's enforced by the compiler rather than by discipline.
 
That constraint is why `Stripe.net` is referenced by `PeakWear.Data` and nothing else. `IPaymentClient` lives in Core; the only class that imports the Stripe namespace is `StripePaymentClient`.
 
Two model types per entity, deliberately. `DbModels/User.cs` mirrors the table and holds the password hash. `Models/UserResponse.cs` is what the API returns and doesn't have that property at all. The service maps between them, so the hash can't leak into a response by accident.

---

## Data model

```
users ──┬── user_preferences   (1:1)
        ├── addresses          (1:many, one default enforced by a partial unique index)
        ├── cart_items         (1:many, unique on user + variant)
        └── orders ── order_items   (1:many, prices and names snapshotted)
 
products ── product_variants   (1:many, unique on product + colour + size)
 
processed_stripe_events        (standalone, keyed on Stripe's own event id)

```

Stock lives on the variant, not the product — you can sell out of black-medium while blue-medium is fine.
 
Order items copy the product name, colour, size, SKU and price rather than joining back to the catalogue. An order is a historical record: if a product is renamed, repriced or deleted, past orders have to keep showing what was actually bought and what was actually paid.
 
An order moves through `Pending → Paid`, `Pending → Failed`, or `Pending → Expired`. It carries the Stripe payment intent id under a filtered unique index — filtered because the column is null until an intent exists, and most rows in a busy table would be.
 
Order numbers come from a Postgres sequence rather than a row count. `nextval()` is atomic, so two concurrent checkouts can't be handed the same number. Gaps from abandoned orders are expected and harmless.

Database schema:
![Database schema](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/schema.png)

---

## What it does

**Browse and select**

Products are listed by section with colour swatches. On the detail page, choosing a colour swaps the image and re-evaluates which sizes are actually available — sizes that are out of stock in that colour disable themselves rather than failing at checkout.

Product listing:
![Product listing](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/product_list.png)

**Size recommendation**

Height, weight, build and fit preference go to the API, which builds a prompt from the product's real available sizes and returns a recommendation with reasoning. The suggested size feeds straight into the size selector, so accepting it is one click.

**Bag and checkout**

Adding to the bag validates against remaining stock. A signed-out shopper who clicks Add to bag doesn't lose the item — the intent is held in `sessionStorage` through the login redirect and applied once they're authenticated.

Checkout picks from saved addresses, defaulting to the one marked default.

**Payment**
 
Placing the order creates it as `Pending`, decrements stock inside a transaction, and asks Stripe for a payment intent. The client secret comes back to the browser, which mounts Stripe's card element — an iframe served from Stripe's own domain, so card numbers never touch this application's JavaScript or its server.

Card form:
![Card payment](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/payment.png)

The order becomes `Paid` when Stripe's webhook says so, not when the browser does. The webhook verifies a signature against the raw request body, records the event id so a retried delivery can't be processed twice, then flips the order and clears the cart in one transaction.
 
Anything left `Pending` for thirty minutes is swept by a background service, which cancels the intent at Stripe and puts the reserved stock back.

Order confirmation:
![Order confirmation](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/order_confirmed.png)

**Account**

Profile details, multiple addresses with default handling, shopping preferences, and password change. Order history shows what was bought at the price it was bought for, and which state each order reached.

Order history:
![Order history](https://github.com/nidhi-sadhu/peakwear/blob/main/docs/screenshots/order_history.png)

---

## Running it locally
 
You'll need .NET 10, PostgreSQL 16, Node 22, and the Stripe CLI.
 
```bash
# database
createdb peakwear
psql peakwear -c "CREATE USER peakwear_app WITH PASSWORD 'devpassword123';"
psql peakwear -c "GRANT ALL PRIVILEGES ON DATABASE peakwear TO peakwear_app;"
psql peakwear -c "GRANT ALL ON SCHEMA public TO peakwear_app;"
 
# secrets — stored outside the repo, never committed
cd api/PeakWear.Api
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=peakwear;Username=peakwear_app;Password=devpassword123"
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
dotnet user-secrets set "Jwt:Issuer" "peakwear-api"
dotnet user-secrets set "Jwt:Audience" "peakwear-web"
dotnet user-secrets set "Groq:ApiKey" "your-groq-key"        # console.groq.com, free
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."     # dashboard.stripe.com, test mode
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
 
# schema — products seed themselves on first run
cd ..
dotnet ef database update --project PeakWear.Data --startup-project PeakWear.Api
dotnet watch --project PeakWear.Api
 
# frontend, in another terminal
cd ../ui && npm install && ng serve
```
 
Stripe can't reach `localhost`, so webhooks arrive through a tunnel the CLI opens. In a third terminal:
 
```bash
stripe listen --forward-to localhost:5248/api/webhooks/stripe
```
 
It prints a signing secret that **changes every time you restart it**. Set it and restart the API, or every webhook fails signature verification and orders stay `Pending` forever:
 
```bash
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
```
 
Pay with `4242 4242 4242 4242`, any future expiry, any CVC. `4000 0000 0000 9995` declines if you want to watch stock come back.
 
API on `:5248`, UI on `:4200`, API docs at `http://localhost:5248/scalar/v1`.
 
---
 
## Where it's at
 
**Working**
- [x] Three-project solution with compiler-enforced layering
- [x] EF Core migrations — the schema lives in git, not just on my laptop
- [x] Registration and login: BCrypt hashing, JWT issuing and validation
- [x] HTTP interceptor attaching the token; a 401 clears the session and redirects
- [x] Route guards with a returnUrl round-trip
- [x] Products and variants — colour × size matrix with per-variant stock and SKUs
- [x] Product list and detail with colour swatches and size availability
- [x] Cart with stock validation, quantity controls and a header badge
- [x] Deferred add-to-bag — a signed-out shopper's item survives the login redirect
- [x] AI size recommender behind a provider-agnostic interface
- [x] Account management: profile, addresses, preferences, password change
- [x] Checkout with transactional stock reservation and price snapshotting
- [x] Stripe payments behind a provider-agnostic interface
- [x] Signed, idempotent webhook confirming payment server-side
- [x] Background sweep releasing stock from abandoned checkouts
- [x] Order confirmation and order history
- [x] Angular frontend deployed to GitHub Pages
**Next**
- [ ] Tests — unit tests over the service layer, integration tests against a real Postgres
- [ ] Docker Compose for the whole stack
- [ ] CI/CD
- [ ] Deploy the API and database so the live site actually has data behind it
**Deliberately not built**
 
Admin screens. I've built plenty of CRUD interfaces professionally and the time was better spent on the parts I hadn't done before. In a real system this is how data gets changed at all — a product manager gets an admin screen, not a database client — so it's an omission I'd fill first on a real product. It's also why there's no refund flow: refunds are an admin action, and building the Stripe call without the surface it belongs to would be the wrong half.
 
**Known gaps**
 
These are trade-offs I made knowingly rather than things I missed.
 
- **No tests.** The largest gap in the project, and the first thing I'd fix.
- **Token in `localStorage`.** Simple, but readable by any injected script. The current recommendation is an in-memory access token plus a refresh token in an HttpOnly cookie. All storage access is isolated in one service so the change is cheap when I make it.
- **No token refresh.** An expired session just logs you out mid-flow.
- **The sweeper assumes one instance.** Two API replicas would both sweep the same orders. The status re-check inside the transaction stops an actual double-restore, but it's wasted work and a race the database is refereeing. A distributed lock or a real scheduler like Hangfire is the fix.
- **Stock isn't reserved at add-to-cart.** Two people can hold the last item; the reservation happens when checkout starts. That matches how most stores behave, but it means someone can lose the item at the final step.
- **No guest cart.** Login is required to add to the bag, softened by the deferred add-to-bag flow.
- **No pagination or search.** Fine at eight products, wrong at eight hundred.
- **No password reset.** Needs an email provider, single-use hashed tokens with short expiry, and the same don't-confirm-whether-the-email-exists rule as login.
- **AI calls aren't cached or rate limited.** Identical inputs cost two API calls, and nothing stops a script exhausting the free quota.
- **Stripe's card element rather than the newer Payment Element.** The Payment Element pulls in Link and pay-later methods, which optimise conversion on a real store but make a demo unpredictable. On a live product I'd use the newer one.
---
 
## Things that went wrong
 
The parts I actually learned from.
 
**A route guard is not a security feature.** It's client-side, so anyone can fake a token in `localStorage` and make the page render. The real protection is `[Authorize]` on the API — the page loads and then every request comes back 401. Guards are for user experience.
 
**Payments broke my checkout transaction.** Before Stripe, one transaction decremented stock, created the order and emptied the cart. That works when everything happens on one server in milliseconds. It doesn't when a network call to a third party sits in the middle, and the customer might take two minutes over their card details or close the tab entirely. Holding row locks on the variants table while waiting on someone's banking app is not an option. So checkout splits: reserve stock and create a `Pending` order, then let a webhook decide what happens next. That single constraint is where the status column, the background sweep and the idempotency table all come from.
 
**The browser is not allowed to say a payment succeeded.** It's the same lesson as the route guard, in a different costume. The tab can close the instant the card clears, the network can drop, and anyone can call an endpoint and claim success. Stripe's webhook is a server-to-server message that retries for three days. That's the source of truth; the redirect to the confirmation page is decoration.
 
**Webhook signatures verify against raw bytes.** Stripe signs the exact body it sent, so reading the request into a model and re-serialising it produces a different string and fails verification — with an error that doesn't explain why. The body has to be read as a raw string before model binding touches it.
 
**`COUNT(*) + 1` is a race condition.** My order numbers came from counting rows. Two concurrent checkouts read the same count, build the same number, and the second one hits the unique index. The window was milliseconds so I'd never seen it — until orders started sitting in `Pending` while someone typed their card, and there were more rows in flight. A Postgres sequence hands out numbers atomically and doesn't roll back, so gaps appear and that's fine.
 
**Stripe's defaults optimise for a real store, not a demo.** Automatic payment methods enabled Klarna, Affirm and Cashapp, which redirect off-site and refused to confirm without a `return_url`. Turning redirects off fixed that, and then Link recognised my email from a previous test and replaced the card form with a saved card. Both are good defaults for someone selling things and wrong for a portfolio project where a reviewer should see the same card form every time. Being explicit about payment methods is the same instinct as validating the AI's size recommendation against real variants: a vendor's helpful default is still an assumption you didn't make.
 
**Two sandboxes, one account.** My Stripe account had two test environments. The CLI authorised against one, my API key belonged to the other, and every command returned `resource_missing` for intents I could see in the database. The account id in the error's request-log URL didn't match the one the CLI printed on startup, which is what finally gave it away. Passing `--api-key` explicitly binds the CLI to whichever account the key belongs to and removes the ambiguity entirely.
 
**"CORS is broken" was a middleware ordering bug.** The browser's preflight `OPTIONS` request carries no token, so authentication was rejecting it before CORS ever ran. Moving `UseCors` above `UseAuthentication` fixed it. I'd have spent a lot longer on this if I'd kept looking at the CORS policy itself.
 
**Registering a service after `builder.Build()` does nothing.** The container is sealed at that point. I added the Stripe client near the bottom of `Program.cs`, next to the pipeline configuration, and spent a while wondering why the DI resolution failed. Services go above the line, middleware below it.
 
**The AI provider abstraction paid for itself within a day.** I built the size recommender against Gemini. The model I'd targeted had been renamed; then Google's API key format changed mid-migration and my account could only issue credentials their own endpoint rejected. Swapping to Groq cost one new class and one DI registration, because the prompt builder, the validation and the controller all depended on `ISizeRecommendationClient` rather than a vendor. I'd argued for the interface on testability grounds — it earned its place on portability instead. I put `IPaymentClient` in from the start for the same reason.
 
**Model output is untrusted input.** Asked to pick from S, M and L, a model will occasionally return XL, or "Medium" instead of "M". The recommendation is validated against the product's real variants before it goes anywhere near the UI.
 
**A C# default isn't a database default.** `Role = "Customer"` on the model only applies when EF constructs the object. A raw SQL insert bypasses C# entirely, and the column came back null against a NOT NULL constraint.
 
**Dapper silently returned empty strings.** Postgres uses `display_name`, C# uses `DisplayName`, and Dapper can't bridge snake_case to PascalCase on its own. `id` and `email` mapped fine, so it looked like a data problem rather than a mapping one.
 
**Rotating a JWT signing key logs everyone out silently.** The frontend still believes it's authenticated because `localStorage` looks fine; every request just 401s. That's what prompted the 401 interceptor.
 
**Two package version conflicts.** The project template shipped a dependency with a published CVE, and the latest major version broke the build through a source generator. Both times the fix was adding the package explicitly at the version I wanted — a direct reference beats a transitive one.
 
**Three Node installations fighting over PATH.** Homebrew, nvm, and the nodejs.org installer, with an odd-numbered version winning. `which -a node` is the command I wish I'd known sooner.
 
---
 
## Notes
 
Commands I use day to day are in [docs/COMMANDS.md](docs/COMMANDS.md).
 
A longer write-up of the payment work — how Stripe fits together, the decisions behind the order lifecycle, and the interview questions each one invites — is in [docs/PeakWear-Payments-Doc.pdf](docs/PeakWear-Payments-Doc.pdf). There's an equivalent one for [authentication](docs/PeakWear-Login-Doc.pdf).
 
Stripe runs in test mode only. No real money moves, and no live keys exist for this project.
 
Product photos are placeholders from free stock libraries and aren't PeakWear's own.
 
