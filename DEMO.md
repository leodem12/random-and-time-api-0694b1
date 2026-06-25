# Demo Runbook: Random & Time API

A step-by-step guide to verify the app is running and demonstrate the four endpoints.

## Prerequisites

- Working directory: `workspace/`
- Docker and Docker Compose available and running
- Network connectivity (no external API calls required)

## Startup

**1. Start the app stack:**
```bash
wsl docker compose up --build --detach
```

Expected output:
```
[+] Building 42.2s (x/y) ...
[+] Creating 2/2
[+] Running 2/2
```

**Wait 10–15 seconds** for PostgreSQL to be healthy and the app to start.

**2. Verify the API is online:**
```bash
curl -s http://localhost:5000/ -w "\nHTTP %{http_code}\n"
```

Expected output: **HTTP 200** and an HTML response (the Angular SPA home page).

**Fallback**: If you see `Connection refused`, the app container is not yet ready. Wait 5 more seconds and retry.

## Happy Path: Random Number

**3. Generate a random number:**
```bash
curl -s http://localhost:5000/api/random | jq .
```

Expected output:
```json
{
  "id": 1,
  "value": 3340686349820027395,
  "createdAt": "2026-06-22T20:13:04.2743547Z"
}
```

(The `value` and exact `createdAt` will differ; the structure is what matters.)

**What happened**: The app called `RandomService.CreateRandomAsync()`, generated a long integer, inserted it into SQLite with a timestamp, and returned the record. The record is now in the SQLite store.

**4. Retrieve the random history:**
```bash
curl -s http://localhost:5000/api/random/history | jq .
```

Expected output:
```json
[
  {
    "id": 1,
    "value": 3340686349820027395,
    "createdAt": "2026-06-22T20:13:04.2743547"
  }
]
```

**Verify**: The `value` from step 3 **appears in this array**. This confirms that the just-generated random number was persisted and is now retrievable.

## Happy Path: Server Time

**5. Get the current server time:**
```bash
curl -s http://localhost:5000/api/now | jq .
```

Expected output:
```json
{
  "id": 1,
  "serverTimeUtc": "2026-06-22T20:13:10.1234567Z"
}
```

(The `serverTimeUtc` will match the current moment on the server.)

**What happened**: The app captured the current UTC time on the server, persisted it to PostgreSQL, and returned the record. The record is now in the PostgreSQL store.

**6. Retrieve the time history:**
```bash
curl -s http://localhost:5000/api/now/history | jq .
```

Expected output:
```json
[
  {
    "id": 1,
    "serverTimeUtc": "2026-06-22T20:13:10.1234567Z"
  }
]
```

**Verify**: The `serverTimeUtc` from step 5 **appears in this array**. This confirms that the just-captured server time was persisted and is now retrievable.

## UI Verification

**7. Open the web UI in a browser:**
```
http://localhost:5000
```

**Expected**:
- Page loads (no 404 or connection errors)
- Four buttons visible:
  - `get-random`
  - `get-random-history`
  - `get-now`
  - `get-now-history`
- A results area below the buttons (initially empty)

**8. Click "get-random":**

Expected on screen:
- A card or section showing a `Value: <large integer>` (the random number returned from `/api/random`)
- No error message

**9. Click "get-random-history":**

Expected on screen:
- A table or list showing the random attempts
- The value you just generated **appears in the list**
- No error message

**10. Click "get-now":**

Expected on screen:
- A card or section showing `Server Time (UTC): <ISO date>` (the server time returned from `/api/now`)
- No error message

**11. Click "get-now-history":**

Expected on screen:
- A table or list showing the time attempts
- The server time you just fetched **appears in the list**
- No error message

**Congratulations!** All four endpoints work end-to-end through the UI. The random number from step 3 and the server time from step 5 are visible in the UI's history views (from steps 9 and 11).

## Empty State Test (Optional)

If you want to verify the empty-state behavior:

**1. Stop and reset the app:**
```bash
wsl docker compose down
wsl docker volume rm sdlc-20260622-214204-0694b1_default
wsl docker compose up --build --detach
```

**2. Immediately click "get-random-history" in the UI:**

Expected: A message like "No records found" or an empty table (no error).

**3. Do the same for "get-now-history":**

Expected: Same empty-state message (no error).

This verifies that the endpoints handle empty history gracefully.

---

## Container & Database Reference

### List Running Containers

```bash
wsl docker ps
```

Expected output shows two containers:
- `sdlc-20260622-214204-0694b1-db-1`: PostgreSQL service
- `sdlc-20260622-214204-0694b1-app-1`: The .NET API app

### Stop Containers

To stop only this app's containers:
```bash
wsl docker stop sdlc-20260622-214204-0694b1-app-1 sdlc-20260622-214204-0694b1-db-1
```

To stop all containers on your machine:
```bash
wsl docker stop $(wsl docker ps -q)
```

### Connect to PostgreSQL

Connect to the `timedb` database to inspect the time history:

```bash
wsl docker exec -it sdlc-20260622-214204-0694b1-db-1 psql -U postgres -d timedb
```

Once connected, run sample queries:

```sql
-- List all time attempts, newest first:
SELECT "Id", "ServerTimeUtc" FROM "TimeAttempts" ORDER BY "ServerTimeUtc" DESC LIMIT 10;

-- Count time attempts:
SELECT COUNT(*) FROM "TimeAttempts";

-- Exit:
\q
```

### SQLite

The SQLite database (`random.db`) lives inside the app container at `/app/random.db`. It is not accessible via `docker exec` (no sqlite3 CLI in the runtime image), but you can inspect it from the host if you copy it out:

```bash
wsl docker cp sdlc-20260622-214204-0694b1-app-1:/app/random.db /tmp/random.db
sqlite3 /tmp/random.db "SELECT * FROM RandomAttempts ORDER BY CreatedAt DESC LIMIT 10;"
```

Or simply use the HTTP endpoint:
```bash
curl -s http://localhost:5000/api/random/history | jq .
```

### Sample Queries

**PostgreSQL: Most recent time attempts**
```sql
SELECT "Id", "ServerTimeUtc" FROM "TimeAttempts" ORDER BY "ServerTimeUtc" DESC LIMIT 5;
```

**PostgreSQL: Total count**
```sql
SELECT COUNT(*) AS total_attempts FROM "TimeAttempts";
```

Note: In PostgreSQL, column names with uppercase letters (as created by EF Core from PascalCase C# properties) must be quoted in queries.
