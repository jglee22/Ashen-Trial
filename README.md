# Ashen Trial

Unity 클라이언트 개발자로서 3D 액션 전투 콘텐츠를 구조적으로 구현하고, 보스 AI, Telegraph, Upgrade, Feedback, Presentation까지 연결해 하나의 완결된 Boss Rush 플레이 루프로 마감한 포트폴리오 프로젝트입니다.

[GitHub](https://github.com/jglee22/Ashen-Trial)

---

## 🎥 Gameplay Video

Gameplay video coming soon.

<!-- Gameplay GIF: 촬영 후 여기에 추가 -->

---

## 🎮 Project Overview

| | |
|---|---|
| 장르 | 3D Boss Rush Roguelite Action |
| 엔진 | Unity 6000.4.2f1 / URP |
| 플레이 | 한 번의 짧은 Run, 보스 3종 |
| 씬 | `Assets/Scenes/Main.unity` |
| 범위 | Single-player, Keyboard/Mouse 중심 |

핵심 Loop:

```text
Boss01 → Upgrade → Boss02 → Upgrade → Boss03 → RunComplete
```

사망 시 GameOver, 클리어 또는 실패 후 Retry는 활성 씬 재로드입니다.

---

## 🎯 Project Goal

3D 액션 전투 콘텐츠를 직접 설계하고 구현해, 하나의 완결된 플레이 루프로 마감하는 것을 목표로 했습니다.

플레이어 전투 규칙을 설계하고, 역할이 다른 보스 3종을 패턴·Telegraph·Phase로 구성한 뒤, Upgrade·환경 상호작용·전투 Feedback·Presentation을 같은 루프에 연결해 끝까지 닫는 Unity Client / Gameplay 작업입니다.

---

## 🕹 Controls

Input System Action 기준입니다.

| Input | Action |
|---|---|
| WASD | Move |
| Left Mouse | Attack (3-hit Combo) |
| Right Mouse | HeavyAttack |
| Space | Dodge |

---

## ⚔️ Core Combat

이동과 Dodge는 `CharacterController` 한 경로로 처리합니다. 공격은 Idle에서만 시작하고, Dodge도 Idle에서만 가능합니다.

**3-hit Combo**는 Straight → Hook → Uppercut으로 구성되며, Recovery 구간의 Combo Window에서 다음 공격을 입력할 수 있습니다. Active 구간에서만 Hitbox가 활성화됩니다.

**Heavy Attack**은 콤보를 끊고 별도 Windup·Hitbox 크기·Recovery를 씁니다. POWER, DESPERATION은 Heavy에도 적용되고, FINISHER는 3타에만 적용됩니다.

**Dodge** 동안은 i-frame입니다. 피격 후에도 짧은 Hit invincibility가 있습니다. Hit 애니메이션은 재생되지만 입력을 잠그지 않습니다. 사망 시 이동·공격·Dodge를 끊습니다.

---

## 👹 Boss Design

세 보스는 같은 색만 바꾼 복제가 아닙니다. 각각 Config ScriptableObject와 Controller를 갖고, HP 50%에서 Phase 2로 넘어갑니다. 진행 중인 패턴은 시작 시점의 Phase 값을 유지합니다.

### Boss01 — 근접

Melee Combo, Charge, Circle AoE. Charge는 경로를 잠근 뒤 돌진하고, Destructible Pillar에 막히면 기둥이 파괴됩니다.

### Boss02 — 원거리

거리 유지 후 Straight / Fan 투사체와 Ground AoE. 투사체는 Object Pool을 사용하고, Phase 2에서 Fan 수와 직사 속도가 늘어납니다.

### Boss03 — 기동

DashStrike, Radial Barrage, Sequential Ground Burst. DashStrike만 기둥을 부숩니다. Phase 2에서 Dash 속도, Radial 수, Burst 횟수가 늘어납니다.

---

## 🔄 Game Flow

`GameFlowController`가 현재 보스, 결과 패널, Upgrade, 기둥 리셋을 한 흐름으로 관리합니다.

```text
Combat
  → Boss 사망 (0.7s)
      → Upgrade 선택 (Boss01 / Boss02)
          → 이전 보스 OFF, Pillar Reset, 다음 보스 ON
      → RunComplete (Boss03)
  → Player 사망 (0.5s) → GameOver
Retry → Scene Reload
```

보스 전환 시 `Health.Died` 구독을 끊고 활성화한 뒤에 다시 붙입니다. 보스 처치 후 결과/Upgrade 구간에서는 플레이어 조작을 잠시 비활성화하고, Upgrade 선택이 끝나면 다시 활성화합니다.

---

## 🧩 Upgrade System

보스 1, 2 처치 후 풀 7종 중 아직 고르지 않은 항목에서 3장을 뽑습니다.

| Upgrade | 효과 |
|---|---|
| POWER | 공격력 +20% |
| SWIFT | 이동 속도 +15% |
| VITALITY | 최대 HP +25 |
| FURY | 공격 속도 +20% |
| QUICKSTEP | Dodge 쿨다운 -20% |
| FINISHER | 콤보 3타 +50% |
| DESPERATION | HP 50% 이하일 때 공격력 +30% |

수치는 Config에 두고, 런타임 배율만 `PlayerUpgradeState`가 들고 있습니다. 에셋 HP를 직접 바꾸지 않습니다.

---

## 🧱 Destructible Environment

전 맵 파괴가 아니라, 보스 돌진과 묶인 기둥 3개입니다.

- Boss01 Charge, Boss03 DashStrike만 `TryBreak`
- Intact / Fractured 전환, Chunk Rigidbody에 진행 방향 힘
- Dust, Camera Shake, SFX
- Upgrade 선택 후 다음 라운드에서 `ResetPillar`

플레이어 공격이나 Boss02 투사체로는 부서지지 않습니다.

---

## ✨ Combat Presentation

연출 컴포넌트는 기존 Damage, Telegraph, GameFlow 지연을 바꾸지 않습니다. Intro는 컷신·락·추가 대기 없이 전투 위에 한 번 재생합니다.

- Player: 타격별 TrailRenderer Swipe, Hit Impact
- Boss: Charge / Fan Cast / DashStrike 대표 VFX
- 등장, Phase 전환, Death Presentation (Death는 Hitstop 이후 unscaled delay)
- Camera Shake, Hitstop, Damage Number, Hit Flash

피격 리액션 애니도 전투 상태를 잠그지 않습니다.

라이팅은 URP에서 Directional + Fill + Torch와 Bloom / Vignette를, Player / Boss / Telegraph가 구분되도록 조정했습니다.

---

## 🏗 Architecture / Implementation

필요한 범위만 나누고, 같은 책임은 한곳에 둡니다.

- Player / Boss 수치는 ScriptableObject, 진행 값은 Runtime State
- 보스별 Controller + Telegraph + Config
- `Health` 이벤트로 HUD, Feedback, Phase, Death 연출을 연결
- Gameplay와 Presentation 컴포넌트 분리
- `OnEnable` / `OnDisable`에서 이벤트 구독 쌍을 맞춤
- Boss02 / Boss03 투사체 Object Pool, 비활성·사망 시 회수
- Build Settings에는 `Main.unity`만 포함하며, Retry는 현재 Scene을 Reload합니다.

---

## 🌐 Localization / Audio / UI

- Unity Localization: Korean / English (Upgrade, 결과 UI)
- TextMeshPro HUD: Player / Boss HP, Upgrade 선택, GameOver, RunComplete
- AudioMixer: BGM / SFX / UI

---

## 🛠 Tech Stack

- Unity 6000.4.2f1
- C#
- URP
- Input System
- TextMeshPro
- Unity Localization
- AudioMixer

---

## 📦 Assets

캐릭터, 애니메이션, 환경, 오디오의 일부는 Third-party asset입니다.

Gameplay 코드, 전투 규칙, 보스 로직, GameFlow, 기둥 상호작용, Presentation 연동은 직접 구현했습니다.

---

## 💡 Development Focus

- 전투 규칙은 타이밍과 상태로 읽히게 유지한다
- Gameplay 수치와 Presentation을 같은 레이어에 섞지 않는다
- 보스 3종과 Upgrade·Retry까지 한 Run으로 닫는다
