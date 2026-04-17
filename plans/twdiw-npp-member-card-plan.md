# 時代力量自有黨員證皮夾建置計畫

## Context（為何採取此路徑）

原想用官方 TWDIW App 發行時代力量黨員證，但該 App 的信任清單由數位發展部（moda）集中審核，目前無公開申請流程，未審核之發行者會顯示「不受信任」。經評估後決定走**完全自主路徑**：基於本 repo 的 MIT 授權程式碼，**fork 出時代力量專屬皮夾 App**，並自建發行端、驗證端與信任清單，完全不依賴 moda 基礎設施。

此路徑最適合：黨內活動報到、黨員身份識別、黨務系統登入、未來黨對黨員的數位服務（電子投票、線上聯署等）。

來源 repo：`C:\workspaces\npp\TWDIW-official-app`（MIT License）

---

## 整體架構

```
┌─────────────────────────────────────────────────────────────┐
│  時代力量黨員（手機端）                                          │
│  └── 時代力量皮夾 App（fork 自 moda holder app）               │
│       endpoint 改指向 wallet.npp.org.tw                       │
└──────────┬──────────────────────────────┬───────────────────┘
           │ OID4VCI 領卡                  │ OID4VP 出示
           ▼                              ▼
┌──────────────────────────┐   ┌──────────────────────────┐
│  發行端（黨中央）           │   │  驗證端（黨工/活動現場）     │
│  - twdiw-issuer-api      │   │  - twdiw-verifier-api    │
│  - twdiw-issuer-web      │   │  - twdiw-verifier-web    │
│  - twdiw-oid4vci-handler │   │  - twdiw-oid4vp-handler  │
│  - twdiw-vc-handler      │   │  - twdiw-vp-handler      │
│  - 黨員資料庫對接          │   │                          │
└──────────────────────────┘   └──────────────────────────┘
           │
           ▼
┌──────────────────────────┐
│  自有信任清單（DID Registry）│
│  - 黨中央 DID（簽發黨員證）  │
│  - 各地黨部 DID（未來擴充）  │
│  - HSM 保護簽章金鑰         │
└──────────────────────────┘
```

---

## 實作工作項目（依優先順序）

### Phase 1: PoC（2–4 週，1–2 名工程師）

目標：在本機/測試環境跑通「發卡 → 持有 → 出示」端到端流程，驗證程式碼可用性。

1. **部署發行端三件組**
   - `core-system/twdiw-issuer-api`：管理後台 API
   - `core-system/twdiw-oid4vci-handler`：OID4VCI 協定端點
   - `core-system/twdiw-vc-handler`：VC 簽章服務
   - 依各子目錄 README 跑起來（Spring Boot + Maven）
2. **部署發行端管理介面**
   - `core-system/twdiw-issuer-web`：建立黨員證 VC 模板
   - 設計欄位：黨員編號、姓名、入黨日期、職務、所屬黨部、有效期
3. **部署驗證端三件組**：`twdiw-verifier-api` / `twdiw-oid4vp-handler` / `twdiw-vp-handler` / `twdiw-verifier-web`
4. **暫用官方 sandbox app + 自簽 DID** 跑通流程，先別碰 App fork

**驗收**：能發出 1 張測試黨員證、用驗證端掃 QR 出示成功、後台看到驗證紀錄。

---

### Phase 2: Fork Holder App（4–8 週）

目標：產出可上架的「時代力量皮夾」App。

1. **Fork Android App**：`APP/moda-digitalwallet-holder-androidapp/`
   - 修改 `build.gradle.kts:36` 將 `DID_ISSUER_URL` 指向 `wallet.npp.org.tw`
   - 修改 `applicationId`、應用名稱、icon、配色、啟動畫面（時代力量 CI）
   - 修改 `APP/APPSDK/lib/http_service.dart` 中所有 `frontend.wallet.gov.tw` 端點
2. **Fork iOS App**：`APP/moda-digitalwallet-holder-iosapp/`，做同樣替換
3. **修改 APPSDK Flutter 模組**：`APP/APPSDK/`
   - `lib/openid_vc_vp.dart:127-239` 中的信任查詢邏輯指向自有 DID Registry
   - 重編 Android AAR / iOS Framework
4. **自簽 / 申請開發者帳號**
   - Apple Developer Program（組織帳號需 D-U-N-S 號碼，約 2–4 週審核，年費 $99）
   - Google Play Console（組織帳號，一次性 $25）
5. **App Store 審核**：iOS 審核較嚴，需準備隱私政策、資料蒐集說明

**驗收**：黨員可在 App Store / Play Store 搜尋到「時代力量黨員證」並安裝使用。

---

### Phase 3: 自建信任清單與金鑰管理（與 Phase 2 並行）

1. **DID 產生**
   - 黨中央產生 1 個發行端 DID（建議 `did:web:wallet.npp.org.tw` 或 `did:key`）
   - 配 X.509 憑證鏈（信任徽章邏輯需 `x509_type` 欄位，見 `Modadw101wService.java`）
2. **Trust Registry API**
   - 自建 `https://wallet.npp.org.tw/api/did/{issuerDID}` 端點，回應格式比照官方
   - 初期硬編碼黨中央 DID = trusted；未來擴充各地黨部
3. **金鑰保護**
   - 簽章私鑰必須放 HSM（雲端：AWS CloudHSM / Azure Dedicated HSM；自建：YubiHSM 2 約 NT$20K）
   - **絕對不可** 把私鑰放普通伺服器——一旦外洩等同所有黨員證可被偽造

---

### Phase 4: 黨務系統整合（4–8 週）

1. **黨員資料庫對接**：發行端讀取現有黨員名冊，比對身份後才核發 VC
2. **領卡流程設計**
   - 線上：黨員登入黨務系統 → 產生領卡 QR → App 掃描領取
   - 線下：黨部櫃台人工核身 → 列印領卡 QR
3. **撤銷機制（Status List）**：退黨/停權時把 VC status 設為 revoked，驗證端會即時拒絕
4. **驗證使用情境**
   - 黨內活動報到：活動現場架平板/手機跑 verifier-web，掃 QR 報到
   - 線上服務 SSO：黨員入口網站接 OID4VP 流程

---

### Phase 5: 法遵與營運

1. **個資法**：黨員資料屬政治意見之敏感個資，蒐集/處理需明確告知與書面同意（個資法 §6）
2. **隱私政策**：App 上架必備
3. **資安監控**：發行端 API 至少接 WAF + log 監控
4. **災難復原**：HSM 金鑰備份策略（多人分持 / Shamir Secret Sharing）
5. **客服**：黨員忘記密碼、換手機、App 異常的處理 SOP

---

## 關鍵檔案路徑速查（相對於 `C:\workspaces\npp\TWDIW-official-app`）

| 工作 | 檔案 |
|------|------|
| Android endpoint 修改 | `APP/moda-digitalwallet-holder-androidapp/app/build.gradle.kts:36` |
| iOS endpoint 修改 | `APP/moda-digitalwallet-holder-iosapp/`（對應 Info.plist / config） |
| Flutter SDK 信任邏輯 | `APP/APPSDK/lib/openid_vc_vp.dart:127-239`、`APP/APPSDK/lib/http_service.dart:130-191` |
| Holder 信任驗證 | `APP/moda-digitalwallet-holder-androidapp/app/src/main/java/tw/gov/moda/digitalwallet/core/verifiable/VerifiableManagerImpl.kt:287-305` |
| 發行端 README | `core-system/twdiw-issuer-api/README.md`、`twdiw-oid4vci-handler/README.md`、`twdiw-vc-handler/README.md` |
| 驗證端 README | `core-system/twdiw-verifier-api/README.md`、`twdiw-oid4vp-handler/README.md`、`twdiw-vp-handler/README.md` |
| x509_type 處理 | `core-system/twdiw-issuer-api/.../Modadw101wService.java` |
| 授權 | `LICENSE.txt`（MIT，記得保留原版權聲明） |

---

## 粗估資源

| 項目 | 估計 |
|------|------|
| 開發人力 | 2–3 名全端工程師 × 3–6 個月（含 PoC、App fork、整合、上架） |
| 雲端基礎設施 | NT$ 10K–30K / 月（AWS / GCP，含 HSM） |
| Apple Developer | $99 / 年（組織帳號需 D-U-N-S） |
| Google Play | $25 一次性 |
| 法務 / 個資顧問 | 一次性 NT$ 50K–150K |
| 資安滲透測試（建議上線前） | NT$ 100K–300K |

---

## 風險與對策

| 風險 | 對策 |
|------|------|
| 簽章金鑰外洩 → 所有黨員證可被偽造 | HSM + 多人分持備份 + 定期金鑰輪替計畫 |
| App Store 審核拒絕（與 moda 官方 App 過於相似） | 視覺差異化、明確標示「政黨自主皮夾」、不冒用 moda 商標 |
| 黨員裝錯 App（裝到官方版） | 上架時 App 名稱、icon 高度差異化；領卡 QR 加 deep link |
| 版本維護負擔（追上游 moda 更新） | 用 git remote 追蹤上游，定期 rebase；非必要不大改核心邏輯 |
| 退黨後 App 仍能展示 | 確實實作 Status List 撤銷；驗證端強制檢查 status |
| 黨員證跨黨部 / 跨組織不被信任 | 中長期目標：與其他政黨 / NGO 互認，或推動 moda 開放第三方信任清單 |

---

## 第一步建議

最小可動作的下一步：**先把 Phase 1 的 PoC 跑起來**，因為這完全不需 App fork、不需法務、不需 HSM，只要兩台伺服器 + 開發機，就能驗證整套程式碼能否簽出可被驗證的黨員證 VC。PoC 成功後再向黨內爭取 Phase 2 預算與決議。

---

## 驗證計畫

### Phase 1 驗收
1. 在本機或測試 VM 部署 issuer / verifier 三件組（共 6 個 service）
2. 透過 issuer-web 建立 1 個測試黨員證模板
3. 用官方 TWDIW App（連接到自部署 issuer）或 sandbox app 領取 1 張測試卡
4. 透過 verifier-web 產生驗證 QR，App 掃描出示
5. 確認 verifier 後台收到驗證結果且 VC 簽章驗證通過

### Phase 2 驗收
1. Fork 後的 App 安裝至實機
2. 從自有 issuer 領卡成功，App 顯示「時代力量黨員證」（自有徽章）
3. 自有 verifier 出示驗證成功
4. 反向測試：用自有 App 領官方 sandbox 的卡（應失敗或標示為外部憑證）

### Phase 4 驗收（端到端）
1. 模擬一場黨內活動：黨員用真實流程從黨務系統領卡 → 到場掃 QR 報到 → 撤銷某黨員後其卡無法再用
2. 滲透測試：第三方資安公司針對發行端 / 驗證端 / App 進行 OWASP Top 10 + OID4VC 專屬攻擊測試
