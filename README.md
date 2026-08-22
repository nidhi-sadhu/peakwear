# PeakWear

An e-commerce app for athletic wear, built to learn the .NET + Angular stack properly rather than just follow tutorials.

I'm building this in the open and updating it as I go. Right now it's early — the API and database are working, the frontend hasn't started yet.

---

## Why I'm building it

I wanted a project where I actually understood every decision, not one where I copied a boilerplate and hoped it worked. So I've been deliberate about a few things:

- **Writing the SQL where it matters.** Dapper for reads, EF Core for writes and schema. I wanted to see the queries, not just trust an ORM.
- **Schema in source control.** Every table is an EF migration, so anyone can clone this and get the same database with one command.
- **Keeping the layers honest.** The business logic project doesn't reference the data project. That's enforced by the compiler, not by discipline.

It's an e-commerce app because that domain has real problems in it — product variants, carts, auth, payments — rather than a to-do list.

---

## Stack

**Backend**
- .NET 10 / C# 14
- ASP.NET Core Web API
- Dapper for queries, EF Core for CRUD and migrations
- PostgreSQL 16

**Frontend** (not started yet)
- Angular 22

**Tooling**
- VS Code on macOS
- Scrutor for DI auto-registration

---

## How it's structured

```
api/
├── PeakWear.Api/       Controllers, DI wiring, HTTP pipeline
├── PeakWear.Core/      Models, services, interfaces — no dependencies
├── PeakWear.Data/      Repositories, DbContext, migrations
└── docs/COMMANDS.md    Commands I use day to day
```

The reference direction is one-way: `Api → Core`, `Api → Data`, `Data → Core`. Core references nothing, so business logic can't accidentally depend on the database.

Two model types per entity, deliberately. `DbModels/User.cs` mirrors the table and holds the password hash. `Models/User/UserResponse.cs` is what the API returns and doesn't have that property at all. The service maps between them, so the hash can't leak into a response by accident.

---

## Running it locally

You'll need .NET 10, PostgreSQL 16, and Node 22.

```bash
# database
createdb peakwear
psql peakwear -c "CREATE USER peakwear_app WITH PASSWORD 'devpassword123';"
psql peakwear -c "GRANT ALL PRIVILEGES ON DATABASE peakwear TO peakwear_app;"
psql peakwear -c "GRANT ALL ON SCHEMA public TO peakwear_app;"

# connection string (stored outside the repo)
cd api/PeakWear.Api
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=peakwear;Username=peakwear_app;Password=devpassword123"

# schema
cd ..
dotnet ef database update --project PeakWear.Data --startup-project PeakWear.Api

# run
dotnet watch --project PeakWear.Api
```

API docs at `http://localhost:5248/scalar/v1`.

---

## Where it's at

**Working**
- [x] Three-project solution with enforced layering
- [x] PostgreSQL + Dapper, reads working end to end
- [x] EF Core migrations — schema lives in git
- [x] `users` and `products` tables
- [x] `GET /api/users` returning JSON

**Next**
- [ ] POST endpoints so the API can create records
- [ ] Registration and login with BCrypt + JWT
- [ ] Product variants (size × colour, each with its own SKU and stock)
- [ ] Angular frontend
- [ ] Cart and checkout
- [ ] AI-assisted product search
- [ ] Deploy somewhere public

---

## Things I got wrong along the way

Keeping these here because they were the parts I actually learned from.

**Dapper silently returned empty strings.** Postgres uses `display_name`, C# uses `DisplayName`, and Dapper can't bridge snake_case to PascalCase on its own. It mapped `id` and `email` fine, so it looked like a data problem rather than a mapping one. Fixed with `MatchNamesWithUnderscores`.

**A C# default isn't a database default.** I had `Role = "Customer"` on the model and assumed inserts would get it. They don't — that only applies when EF constructs the object. A raw SQL insert bypasses C# entirely and the column came back null against a NOT NULL constraint.

**Two package version conflicts.** The project template shipped a dependency with a CVE, and the latest major version broke the build through a source generator. Had to find the highest version that was both patched and compatible. Same thing again later with EF Core assemblies. The fix both times was adding the package explicitly — a direct reference beats a transitive one.

**Three Node installations fighting over PATH.** Homebrew, nvm, and the nodejs.org installer, with an odd-numbered version winning. `which -a node` is the command I wish I'd known sooner.

---

## Notes

The commands I actually use are in [docs/COMMANDS.md](docs/COMMANDS.md).

This is a learning project, so some of it is more deliberate than a real deadline would allow. That's on purpose.