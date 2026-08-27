# PeakWear

An e-commerce app for athletic wear, built to learn the .NET + Angular stack properly rather than by following a tutorial.

I'm building this in the open and updating it as I go. Auth, the product catalogue, the cart, an AI size recommender, and account management all work end to end. Checkout is next.

---

## Why I'm building it

I wanted a project where I understood every decision, not one where I copied a boilerplate and hoped it worked. A few things I've been deliberate about:

- **Getting the variant model right first.** A pair of leggings isn't one sellable thing — it's nine, one per colour and size, each with its own SKU and stock. Retrofitting that later means rewriting the cart and every stock check.
- **Schema in source control.** Every table is an EF migration, so a fresh clone plus one command gives you the same database.
- **Keeping the layers honest.** The business logic project doesn't reference the data project. That's enforced by the compiler, not by discipline.
- **Abstracting the AI provider.** The size recommender talks to an interface, not to a vendor. That turned out to matter — see below.

It's an e-commerce app because that domain has real problems in it: variants, auth, stock, concurrency. A to-do list doesn't.

---

## Stack

**Backend**
- .NET 10 / C# 14
- ASP.NET Core Web API
- EF Core 10 for writes, CRUD and migrations; Dapper available for complex reads
- PostgreSQL 16
- JWT auth with BCrypt password hashing
- Scrutor for DI auto-registration

**Frontend**
- Angular 22 (zoneless, signal-first)
- NgRx SignalStore for state
- Angular Material
- Reactive forms

**AI**
- Groq (`openai/gpt-oss-20b`) behind a provider-agnostic interface

---

## How it's structured

```
api/
├── PeakWear.Api/       Controllers, DI wiring, HTTP pipeline
├── PeakWear.Core/      Models, services, interfaces — no dependencies
└── PeakWear.Data/      Repositories, DbContext, migrations, AI clients

ui/src/app/
├── core/auth/          Token service, HTTP interceptor, route guard
├── modules/
│   ├── login/          Auth store, login and register
│   ├── products/       Product store, list, detail, size recommender
│   ├── cart/           Cart store and page
│   ├── account/        Profile, addresses, preferences
│   └── home/           Landing page
└── shared/
```

The API reference direction is one-way: `Api → Core`, `Api → Data`, `Data → Core`. Core references nothing, so business logic can't accidentally depend on the database.

Two model types per entity, deliberately. `DbModels/User.cs` mirrors the table and holds the password hash. `Models/UserResponse.cs` is what the API returns and doesn't have that property at all. The service maps between them, so the hash can't leak into a response by accident.

---

## Data model

```
users ──┬── user_preferences   (1:1)
        ├── addresses          (1:many, one default enforced by a partial unique index)
        └── cart_items         (1:many, unique on user + variant)

products ── product_variants   (1:many, unique on product + colour + size)
```

Stock lives on the variant, not the product — you can sell out of black-medium while blue-medium is fine.

---

## Running it locally

You'll need .NET 10, PostgreSQL 16, and Node 22.

```bash
# database
createdb peakwear
psql peakwear -c "CREATE USER peakwear_app WITH PASSWORD 'devpassword123';"
psql peakwear -c "GRANT ALL PRIVILEGES ON DATABASE peakwear TO peakwear_app;"
psql peakwear -c "GRANT ALL ON SCHEMA public TO peakwear_app;"

# secrets (stored outside the repo, never committed)
cd api/PeakWear.Api
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=peakwear;Username=peakwear_app;Password=devpassword123"
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
dotnet user-secrets set "Jwt:Issuer" "peakwear-api"
dotnet user-secrets set "Jwt:Audience" "peakwear-web"
dotnet user-secrets set "Groq:ApiKey" "your-groq-key"   # console.groq.com, free

# schema — products seed themselves on first run
cd ..
dotnet ef database update --project PeakWear.Data --startup-project PeakWear.Api
dotnet watch --project PeakWear.Api

# frontend, in another terminal
cd ../ui && npm install && ng serve
```

API on `:5248`, UI on `:4200`, API docs at `http://localhost:5248/scalar/v1`.

---

## Where it's at

**Done**
- [x] Three-project solution with compiler-enforced layering
- [x] EF Core migrations — schema lives in git
- [x] Registration and login: BCrypt hashing, JWT issuing and validation
- [x] HTTP interceptor attaching the token; 401 clears the session and redirects
- [x] Route guards with returnUrl round-trip
- [x] Products and variants — colour × size matrix, per-variant stock and SKUs
- [x] Product list and detail with colour swatches and size availability
- [x] Cart with stock validation, quantity controls and a header badge
- [x] Deferred add-to-bag — a signed-out shopper's item survives the login redirect
- [x] AI size recommender behind a provider-agnostic interface
- [x] Account management: profile, addresses with default handling, preferences, password change
- [x] Angular frontend deployed to GitHub Pages

**Next**
- [ ] Checkout — order and order_items, address selection, price frozen at purchase
- [ ] Transactional stock decrement using the Postgres `xmin` concurrency token
- [ ] Order history
- [ ] Preferences actually driving behaviour (landing section, pre-filled sizes)
- [ ] Deploy the API and database

**Known gaps, deliberately**
- Token in `localStorage` rather than in-memory plus an HttpOnly refresh cookie. All storage access is isolated in one service so the change is cheap.
- No token refresh — an expired session just logs you out.
- Stock is checked at add-to-cart but not reserved. Two people can hold the last item; checkout will need a transactional re-check.
- No guest cart. Login is required, softened by the deferred add-to-bag flow.
- No pagination or search on the product list.
- No password reset. Needs an email provider, single-use hashed tokens, and the same don't-confirm-the-email-exists rule as login.

---

## Things I got wrong along the way

The parts I actually learned from.

**A route guard is not a security feature.** It's client-side, so anyone can fake a token in `localStorage` and make the page render. The real protection is `[Authorize]` on the API — the page loads and then every request returns 401. Guards are for user experience.

**CORS "not working" was a middleware ordering bug.** The browser's preflight `OPTIONS` request carries no token, so authentication was rejecting it before CORS ever ran. Moving `UseCors` above `UseAuthentication` fixed it.

**A C# default isn't a database default.** `Role = "Customer"` on the model only applies when EF constructs the object. A raw SQL insert bypasses C# entirely, and the column came back null against a NOT NULL constraint.

**Dapper silently returned empty strings.** Postgres uses `display_name`, C# uses `DisplayName`, and Dapper can't bridge snake_case to PascalCase alone. `id` and `email` mapped fine, so it looked like a data problem rather than a mapping one.

**The AI provider abstraction paid for itself within a day.** I built the size recommender against Gemini. The model I targeted had been renamed, then Google's key format changed mid-migration and my account could only issue credentials their own endpoint rejected. Swapping to Groq cost one new class and one registration line, because everything upstream depended on `ISizeRecommendationClient` rather than a vendor. I'd argued for the interface on testability grounds; it earned its place on portability instead.

**Never trust model output.** The recommender validates the returned size against the sizes that actually exist, and falls back to a deterministic BMI calculation if the API is unavailable or returns something invented. A feature that breaks the page when a third party is down isn't finished.

**Rotating a JWT signing key logs everyone out silently.** The frontend still thinks it's authenticated because `localStorage` looks fine; every request just 401s. That's what prompted the 401 interceptor.

**Two package version conflicts.** The project template shipped a dependency with a CVE, and the latest major broke the build through a source generator. Both times the fix was adding the package explicitly at the version I wanted — a direct reference beats a transitive one.

**Three Node installations fighting over PATH.** Homebrew, nvm, and the nodejs.org installer, with an odd-numbered version winning. `which -a node` is the command I wish I'd known sooner.

---

## Notes

Commands I use day to day are in [docs/COMMANDS.md](docs/COMMANDS.md).

Product photos are placeholders from free stock libraries and are not PeakWear's own.

This is a learning project, so some decisions are more deliberate than a real deadline would allow. That's on purpose.
