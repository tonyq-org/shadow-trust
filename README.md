# Shadow Trust

A paid Trust List service for Decentralized Identity Wallet (DIW) ecosystems.

## Structure

```
shadow-trust/
├── backend/          # ASP.NET MVC — Trust Registry API + Admin API
├── frontend/
│   ├── landing/      # Vue 3 — Public website + pricing
│   └── admin/        # Vue 3 — Admin management SPA
└── plans/            # Project plans
```

## Quick Start

### Backend

```bash
cd backend
dotnet run
```

### Frontend (Landing)

```bash
cd frontend/landing
npm install
npm run dev
```

### Frontend (Admin)

```bash
cd frontend/admin
npm install
npm run dev
```

## License

MIT
