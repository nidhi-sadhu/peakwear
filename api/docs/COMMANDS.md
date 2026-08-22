# PeakWear — Commands

Run from `~/Projects/PeakWear/api`.

---

## Aliases

Open the file, paste at the bottom, save:

```bash
open -e ~/.zshrc
```

```bash
alias efadd='dotnet ef migrations add --project PeakWear.Data --startup-project PeakWear.Api'
alias efup='dotnet ef database update --project PeakWear.Data --startup-project PeakWear.Api'
alias efrm='dotnet ef migrations remove --project PeakWear.Data --startup-project PeakWear.Api'
alias eflist='dotnet ef migrations list --project PeakWear.Data --startup-project PeakWear.Api'

alias apirun='dotnet watch --project PeakWear.Api'
```

```bash
source ~/.zshrc      # reload without reopening the terminal
```

---

## PostgreSQL

```bash
brew services start postgresql@16
brew services stop postgresql@16
brew services list                    # is it running?
```

```bash
PGPASSWORD=devpassword123 psql -h localhost -U peakwear_app -d peakwear -c "\dt"      # list tables
PGPASSWORD=devpassword123 psql -h localhost -U peakwear_app -d peakwear -c "\d users" # describe a table
```

---

## Run the API

```bash
dotnet watch --project PeakWear.Api        # auto-restarts on save
curl http://localhost:5248/api/users
```

---

## Migrations

**The loop:** change the class → `efadd Name` → **read the file** → `efup`

```bash
efadd AddProducts     # writes the migration file (database untouched)
efup                  # runs it against the database
eflist                # what exists
```

### Add a table

1. Create the class in `PeakWear.Core/DbModels/`
2. Add `public DbSet<Thing> Things => Set<Thing>();` to `PeakWearDbContext`
3. `efadd AddThings` → read → `efup`

### Add / remove a column

1. Add or delete the property in the class
2. `efadd AddPhoneToUser` → read → `efup`

New columns on a table with data must be nullable (`string?`) or have a default.

### Delete a table

1. Delete the class file
2. Delete its `DbSet` line and its `OnModelCreating` block
3. Fix anything that referenced it, or the build fails
4. `efadd RemoveThings` → read → `efup`

### Undo

```bash
efrm                                                    # before efup — just deletes the file
```

```bash
dotnet ef database update PreviousMigrationName --project PeakWear.Data --startup-project PeakWear.Api
efrm                                                    # after efup — roll back first, then delete
```

### Wipe everything

```bash
dotnet ef database update 0 --project PeakWear.Data --startup-project PeakWear.Api
rm -rf PeakWear.Data/Migrations
```

---

## Warnings

- **Always read the migration before `efup`.** `DropColumn` and `DropTable` delete data permanently.
- **Renaming a property** generates drop + add, which destroys the data. Edit the migration to use `migrationBuilder.RenameColumn(...)` instead.
- **Never** edit an applied migration, or delete `PeakWearDbContextModelSnapshot.cs`.

---

## Git

```bash
git status            # check no secrets listed
git add .
git commit -m "feat: add products table"
git push
```
