# Shadow Trust — DIW 信任清單服務計畫

## 定位

Shadow Trust 是一套**付費信任清單服務**（Trust-as-a-Service），搭配 Shadow Wallet App 構成完整的 DIW 生態：

1. **Shadow Trust**（本 repo）— 付費信任清單服務 + 管理後台
2. **Shadow Wallet App**（獨立 repo）— 免費開源的身份皮夾 App（Android / iOS）
3. **Issuer / Verifier Toolkit**（獨立 repo）— 組織端發行與驗證工具

## 商業模式

```
使用者（免費）                    組織客戶（付費）
┌──────────────┐               ┌──────────────────────────┐
│ Shadow Wallet │               │ 政黨 / NGO / 企業 / 學校  │
│ App（免費）    │◄─── 信任 ────│ 付費加入 Shadow Trust      │
│              │               │ 使用 Issuer Toolkit 發證   │
└──────────────┘               └──────────────────────────┘
```

### Trust List 收費模式

| 方案 | 內容 | 適用對象 |
|------|------|---------|
| 基本方案 | 1 個 DID 上架、基本驗證徽章、年度續約 | 小型 NGO / 社團 |
| 專業方案 | 多 DID、自訂徽章、API 存取、SLA 保證 | 中型組織 / 企業 |
| 企業方案 | 無限 DID、白標支援、專屬客服、HSM 代管 | 大型企業 / 政府機關 |

**核心價值**：moda 的信任清單是政府審核制、不公開申請流程。Shadow Trust 是商業服務，付費即可上架，我們負責審核品質與維運。

---

## 技術架構

```
┌─────────────────────────────────────────────────────────────────┐
│  使用者（手機端）                                                  │
│  └── Shadow Wallet App（fork 自 moda TWDIW，MIT License）         │
│       ├── 連接 Shadow Trust 驗證發行者身份                         │
│       └── 支援 OID4VCI 領證 / OID4VP 出示                         │
└──────────┬──────────────────────────────┬───────────────────────┘
           │ OID4VCI 領證                  │ OID4VP 出示
           ▼                              ▼
┌──────────────────────────┐   ┌──────────────────────────┐
│  Issuer Toolkit（組織端）  │   │  Verifier Toolkit（組織端）│
│  - issuer-api            │   │  - verifier-api           │
│  - oid4vci-handler       │   │  - oid4vp-handler         │
│  - vc-handler            │   │  - vp-handler             │
│  - issuer-web（管理介面）  │   │  - verifier-web（驗證介面）│
└──────────┬───────────────┘   └───────────┬──────────────┘
           │                               │
           ▼                               ▼
┌─────────────────────────────────────────────────────────────┐
│  Shadow Trust（我們維護的付費服務，本 repo）                     │
│  ├── Trust Registry API                                     │
│  │   └── GET /api/trust/{issuerDID} → trusted / untrusted   │
│  ├── Trust Admin Portal（上架審核 / 客戶管理 / 帳務）           │
│  ├── DID Registry（所有付費客戶的 DID 與憑證資訊）              │
│  └── HSM 金鑰代管（企業方案選配）                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Repo 結構

```
shadow-trust/
├── backend/                        # ASP.NET MVC
│   ├── Controllers/
│   │   ├── TrustController.cs      # 對外 /api/trust/:did
│   │   └── AdminController.cs      # 對內 /api/admin/*（需認證）
│   ├── Models/
│   ├── Services/
│   ├── Data/                       # EF Core DbContext + migrations
│   └── Program.cs
├── frontend/
│   ├── landing/                    # Vue 3 — 官網 + 定價
│   └── admin/                      # Vue 3 — 管理後台 SPA
├── plans/
└── README.md
```

相關獨立 repo（fork from moda TWDIW）：
- `shadow-wallet-app/` — Holder App
- `shadow-wallet-toolkit/` — Issuer / Verifier 工具

---

## 實作階段

### Phase 1: Trust List MVP（4 週）

核心目標：先把收費服務的骨架做出來。

1. **Trust Registry API** — 提供 `GET /api/trust/{issuerDID}` 查詢端點
2. **Trust Admin Portal** — 管理後台，可新增/停用受信任的 DID
3. **定價頁面 / Landing Page** — 對外說明服務內容與收費
4. **基本審核流程** — 組織申請 → 人工審核 → 上架

**驗收**：能透過 API 查詢某 DID 是否受信任，後台可管理信任清單。

---

### Phase 2: Wallet App Fork（4–6 週）

1. **Fork moda TWDIW App**（MIT License）→ 獨立 repo `shadow-wallet-app`
2. **替換信任清單端點** — 指向 Shadow Trust API
3. **品牌化** — Shadow Wallet 自有名稱、icon、配色
4. **移除 moda 專屬邏輯** — 保留通用 DIW 功能
5. **測試** — 與 Phase 1 的 Shadow Trust 端對端驗證

**驗收**：App 安裝後可領證、出示，信任徽章由 Shadow Trust 決定。

---

### Phase 3: Issuer / Verifier Toolkit 打包（4 週）

1. **整理發行端三件組** — Docker 化，一鍵部署
2. **整理驗證端三件組** — 同上
3. **Toolkit 文件** — 組織客戶的部署指南
4. **範例模板** — 員工證、會員卡、活動通行證等 VC 模板

**驗收**：新客戶可按文件在 30 分鐘內部署完發行端並發出第一張證。

---

### Phase 4: 商業化與上架（4–6 週）

1. **帳務系統** — 收費、續約、到期停用
2. **App 上架** — Google Play + Apple App Store
3. **第一批客戶** — 找 2-3 個組織試用（例如時代力量、NPO 等）
4. **SLA 與監控** — uptime 保證、告警

---

### Phase 5: 進階功能

1. **HSM 代管服務** — 替企業客戶管理簽章金鑰
2. **白標方案** — 客戶可自訂 App 品牌（但仍走 Shadow Trust）
3. **跨平台互通** — 與 moda 官方或其他 DIW 生態互認
4. **Status List（撤銷機制）** — 完整的證照生命週期管理
5. **Analytics Dashboard** — 客戶可查看驗證次數、活躍持有者等數據

---

## 與 moda TWDIW 的差異

| | moda TWDIW | Shadow Trust |
|---|---|---|
| 信任清單 | 政府審核、不公開申請 | 商業服務、付費上架 |
| App | 官方唯一 | 開源、可 fork（Shadow Wallet） |
| 費用 | 免費但進不去 | 明確定價 |
| 適用對象 | 政府認可機構 | 任何組織 |
| 維運 | 數位發展部 | Shadow Trust 團隊 |

---

## 關鍵風險

| 風險 | 對策 |
|------|------|
| moda 未來開放信任清單申請，價值降低 | 差異化：更快上架、更好客服、進階功能（HSM 代管、白標） |
| 品質控管 — 付費就上架可能被濫用 | 仍需人工審核，拒絕明顯詐騙/違法組織 |
| App Store 審核風險 | 充分品牌差異化，不冒用 moda 商標 |
| 法規風險（個資法） | 皮夾本身不儲存個資於我方伺服器；Trust List 僅存 DID 公開資訊 |

---

## 第一步

立即可做：**Phase 1 — Trust Registry API + Admin Portal**。這不需要 App fork，不需要 HSM，只需要一個後端服務 + 簡單管理介面，就能開始找早期客戶驗證商業模式。
