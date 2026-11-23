# 🏗️ Crossy Road 프로젝트 - 디자인 패턴 분석 보고서

## 📋 개요

- **프로젝트명**: Crossy Road (Unity 6000.4.0f1)
- **분석 범위**: `Assets/Scripts`의 C# 파일 33개, `Assets/Editor`의 에디터 스크립트 1개, 관련 씬·프리팹·`RowData` 에셋
- **분석 기준**: 현재 작업 트리의 코드와 직렬화된 참조, GoF 디자인 패턴, Unity 게임 개발 관례
- **최종 분석**: 2026-11-23

**분석 방법**:
- 코드와 에셋을 정적으로 확인했다. 이번 보고서 작성을 위한 새로운 플레이 테스트나 성능 측정은 하지 않았다.
- GoF 패턴과 유사한 구조가 있어도 핵심 구조가 없으면 실제 적용으로 단정하지 않았다.
- 스크립트의 기본값과 씬·프리팹에서 설정한 값은 구분했다.

---

## 1️⃣ 발견된 주요 디자인 패턴

**현재 구현 요약**:

1. ✅ Singleton 패턴
2. ✅ Object Pool 패턴과 `IPoolable` 생명주기 콜백
3. ✅ ObjectPoolManager를 통한 소유자별 풀 레지스트리
4. ✅ enum 기반 상태 관리 — GoF State 패턴은 아님
5. ✅ 생성 책임 분리와 생성 함수 주입 — GoF Factory Method와 구별
6. ✅ Unity 컴포넌트 조합과 데이터 기반 설계
7. ⚠️ Strategy, Observer, Adapter, Command의 핵심 구조는 확인되지 않음

---

### 1.1 **Singleton 패턴** ⭐

**정의**: 공통 서비스의 인스턴스를 하나로 유지하고 전역에서 접근하는 방식.

**구현**:

- 파일: [Singleton.cs](../Assets/Scripts/Util/Singleton.cs)
- `Instance`는 기존 컴포넌트를 찾고 없으면 GameObject를 생성한다.
- `Awake()`에서 인스턴스를 지정하고 `DontDestroyOnLoad`를 적용한다. 중복 인스턴스는 파괴한다.

**상속 클래스**:

| 클래스 | 책임 |
|---|---|
| [GameManager](../Assets/Scripts/Manager/GameManager.cs) | 게임 상태·점수·최고 점수 |
| [FadeManager](../Assets/Scripts/Manager/FadeManager.cs) | 페이드 연출 |
| [ScenesManager](../Assets/Scripts/Manager/SceneManager.cs) | 씬 전환과 로딩 순서 |
| [MouseManager](../Assets/Scripts/Util/MouseManager.cs) | Input System 마우스 상태 조회 |
| [ObjectPoolManager](../Assets/Scripts/Manager/ObjectPoolManager.cs) | 풀 등록·조회 |

**특징 및 주의점**:

- ✅ 5개 클래스가 공용 `Singleton<T>`를 사용한다.
- ✅ [UIManager](../Assets/Scripts/Manager/UIManager.cs)는 정적 `_UM`을 사용하지만 `Singleton<T>`를 상속하지 않는 씬 범위 관리자다.
- ⚠️ 정적 접근은 의존성과 초기화 순서를 파악하거나 테스트하기 어렵게 할 수 있다.
- ⚠️ 자동 생성되는 컴포넌트의 직렬화 참조는 자동으로 채워지지 않는다. 예를 들어 `FadeManager`에는 `CanvasGroup` 설정이 필요하다.
- ⚠️ 씬 전환 후에도 유지되는 관리자가 씬 객체를 참조할 때는 참조 수명을 확인해야 한다.

---

### 1.2 **Object Pool 패턴** ⭐⭐⭐

**정의**: 사용한 객체를 바로 파괴하지 않고 보관해 이후 다시 사용하는 방식.

#### **A. 핵심 구현**

- 파일: [GameObjectPool.cs](../Assets/Scripts/Util/GameObjectPool.cs)
- 제약: `T : Component, IPoolable`

```text
생성: 생성 함수 → OnCreate() → 선택적 생성 콜백 → 비활성화
대여: Queue에서 꺼내기 또는 새로 생성 → 활성 목록 등록 → OnGet() → 활성화
반환: 활성 목록에서 제거 → OnReturn() → 비활성화 → 풀 루트로 이동 → Queue
```

**특징**:

- 사용 가능한 객체는 `Queue<T>`, 대여 중인 객체는 `HashSet<T>`로 추적한다.
- 풀이 부족하면 새 객체를 만든다. 현재 최대 크기나 축소 정책은 없다.
- `Func<T>`는 객체 생성 방법을 제공하고, 선택적 `Action<T>`는 새 객체 생성 시 호출된다.
- `IPoolable.OnGet()`은 풀 코드의 `SetActive(true)`보다 먼저 호출된다.
- 유효하지 않거나 해당 풀에서 대여 중이 아닌 객체의 반환은 무시한다.

#### **B. 전체 적용 현황**

아래 초기 크기는 **스크립트 기본값**이며 Inspector 설정에 따라 달라질 수 있다.

| 사용 위치 | 풀 대상 | 기본 초기 크기 |
|---|---|---:|
| [CarGenerator](../Assets/Scripts/Map/Cars/CarGenerator.cs) | `CarMovement` | 10 |
| [LogGenerator](../Assets/Scripts/Map/River/LogGenerator.cs) | `LogMovement` | 10 |
| [LotusGenerator](../Assets/Scripts/Map/River/LotusGenerator.cs) | `LotusMovement` | 10 |
| [TrainGenerator](../Assets/Scripts/Map/Train/TrainGenerator.cs) | `TrainMovement` | 1 |
| [NatureGenerator](../Assets/Scripts/Map/Nature/NatureGenerator.cs) | `NaturesReturn` | 종류별 10 |
| [MapGenerator](../Assets/Scripts/Map/MapGenerator.cs) | `RowObject` | 프리팹별 10 |
| [RiverController](../Assets/Scripts/Map/River/RiverController.cs) | `SpalshPool` | 5 |

#### **C. IPoolable 인터페이스 구현 현황**

파일: [IPoolable.cs](../Assets/Scripts/Interface/IPoolable.cs)

```csharp
public interface IPoolable
{
    void OnCreate();
    void OnGet();
    void OnReturn();
}
```

**구현 클래스: 7개**

| 클래스 | 주요 콜백 동작 |
|---|---|
| [CarMovement](../Assets/Scripts/Map/Cars/CarMovement.cs) | `OnGet`에서 목표 속도·감지 상태 초기화 |
| [LogMovement](../Assets/Scripts/Map/River/LogMovement.cs) | `OnGet`에서 이동 방향 초기화 |
| [LotusMovement](../Assets/Scripts/Map/River/LotusMovement.cs) | `OnGet`이 `ResetState`를 호출하지만 현재 내용은 비어 있음 |
| [TrainMovement](../Assets/Scripts/Map/Train/TrainMovement.cs) | `OnGet`에서 시작 위치 필드를 `Vector3.zero`로 초기화 |
| [NaturesReturn](../Assets/Scripts/Map/Nature/NaturesReturn.cs) | `OnReturn`에서 비활성화·회전 초기화 |
| [RowObject](../Assets/Scripts/Map/RowObject.cs) | 세 콜백이 현재 비어 있음 |
| [SpalshPool](../Assets/Scripts/Map/River/SplashPool.cs) | 파티클 참조 설정, 재생 및 정지 |

빈 콜백 자체가 문제라는 의미는 아니다. 다만 `Start()`는 객체 재사용마다 다시 실행되지 않으므로 필요한 상태가 매 대여 시 초기화되는지 확인해야 한다.

**성능 해석**:

- 풀링은 반복 생성·파괴를 줄일 수 있지만 초기 생성과 객체 보관 비용이 있다.
- 모든 GC 할당이 사라지는 것은 아니며, 효과는 대상 프리팹과 실제 부하에 따라 달라진다.
- [기존 Development Build 측정 보고서](../OBJECT_POOLING_MEASUREMENT_REPORT.md)는 풀 관리와 자연물 반환 경로가 변경되기 전의 통제된 실험이다. 해당 수치를 현재 구현이나 실제 플레이 전체에 그대로 적용할 수 없다.

---

### 1.3 **ObjectPoolManager 패턴** ⭐⭐ (풀 관리 중앙화)

**정의**: 여러 풀의 등록과 조회를 한 관리자에서 처리하는 레지스트리. GoF 공식 패턴명은 아니다.

**구현**: [ObjectPoolManager.cs](../Assets/Scripts/Manager/ObjectPoolManager.cs)

```csharp
// 소유자별 등록과 조회
manager.RegisterPool(this, "Car", pool);
GameObjectPool<CarMovement> carPool = manager.GetPool<CarMovement>(this, "Car");

// 소유자가 파괴될 때 등록 해제
manager.UnregisterPools(this);
```

**동작**:

```text
CarGenerator A ── RegisterPool(A, "Car", 풀 A) ──┐
                                                  ├─ ObjectPoolManager
CarGenerator B ── RegisterPool(B, "Car", 풀 B) ──┘
                     └─ GetPool(B, "Car") → 풀 B
```

- 관리자 키는 **소유 컴포넌트와 풀 이름**의 조합이다. 같은 이름의 생성기가 여러 행에 있어도 서로의 풀을 덮어쓰지 않는다.
- 차량·통나무·연꽃·열차 생성기와 `RiverController`는 최초 `OnEnable`에서 풀을 만들고 등록한 뒤, 활성화 시 관리자로부터 조회한다.
- `NatureGenerator`는 `Awake`에서 종류별로 등록하고, `GetNature()`에서 조회한다.
- `MapGenerator`는 행 프리팹을 처음 사용할 때 등록하고 행 생성·반환 시 조회한다. 프리팹 참조별 순번을 사용하므로 이름이 같은 서로 다른 프리팹도 구분한다.
- 생성기나 관리자가 파괴될 때 자기 소유 등록을 해제한다. 비활성화 후 재활성화에는 기존 풀을 사용한다.
- 소유자 범위 API는 중복 등록, 누락 및 타입 불일치를 예외로 알린다. 기존 이름만 사용하는 API도 별도로 남아 있다.
- `PrintPoolStatistics()`는 이름과 타입을 출력한다. 활성 수·잔여 수의 통합 출력은 현재 없다.

**중요한 구분**: 각 생성기가 소유한 개별 풀을 중앙에서 조회하는 구조이며, 모든 생성기가 하나의 전역 풀을 공유하는 방식은 아니다.

---

### 1.4 **State 관련 구조** ⭐⭐ (enum 기반 상태 관리)

**구현**: [GameManager.cs](../Assets/Scripts/Manager/GameManager.cs)

```csharp
public enum GameState
{
    Main,
    Playing,
    GameOver
}
```

**상태 전이 흐름**:

```text
Main ── 클릭 / StartGame() ──→ Playing
Playing ── 충돌·낙하 등 / GameOver() ──→ GameOver
GameOver ── 재시도 / 씬 전환 / ResetGame() ──→ Main
```

- `ChangeState()`는 enum 값을 설정하고 `switch`로 UI 메서드를 호출한다.
- `PlayerController`, `CameraTargetController` 등은 게임 상태를 확인한다.
- `MapManager.Update()`는 코드상 게임 상태를 검사하지 않고 `CheckPos()`를 호출한다.

**판정**: 상태 객체와 공통 상태 인터페이스에 행동을 위임하는 GoF State 패턴이 아니라 **enum 기반 상태 분기**다.

---

### 1.5 **Factory 관련 구조** ⭐ (생성 책임 분리)

**구현**:

```text
MapGenerator    → 행 선택과 행 풀 사용
CarGenerator    → 차량 생성과 반환
LogGenerator    → 통나무 생성과 반환
LotusGenerator  → 연꽃 생성과 반환
TrainGenerator  → 열차 생성과 반환
NatureGenerator → 자연물 종류 선택과 반환
```

각 생성기는 `GameObjectPool<T>`에 프리팹 생성 함수를 전달하고 생성·배치·반환을 담당한다. `MapGenerator`는 가중치로 `RowData`도 선택한다.

**판정**: 생성 책임 분리와 생성 함수 주입은 적용되어 있다. 공통 Creator의 생성 메서드를 하위 클래스에서 재정의하는 GoF Factory Method 구조로는 분류하지 않는다.

---

### 1.6 **Strategy 패턴** (적용 확인되지 않음)

차량·통나무·열차는 각각 다른 이동 로직을 갖지만, 공통 전략 인터페이스를 통해 알고리즘 객체를 교체하는 구조는 확인되지 않는다. `RowType`에 따른 행 선택도 데이터와 조건 분기다.

**판정**: 서로 다른 컴포넌트나 조건 분기가 있다는 사실만으로 Strategy 패턴을 적용했다고 볼 수 없다.

---

### 1.7 **Observer / Callback 관련 구조** ⭐⭐

**구현**: `GameObjectPool<T>`가 `IPoolable`의 `OnCreate`, `OnGet`, `OnReturn`과 선택적 `Action<T>`를 직접 호출한다. Unity의 `OnEnable`, `OnDisable`, `OnTriggerEnter` 등도 엔진이 호출하는 콜백이다.

**특징**:

- ✅ 풀은 구체 객체별 초기화 내용을 몰라도 공통 인터페이스를 호출할 수 있다.
- ✅ `SpalshPool`은 생성·대여·반환 콜백에서 파티클을 관리한다.
- ⚠️ 생성 콜백 `Action<T>`는 매 대여가 아니라 **새 객체 생성 시** 실행된다.

**판정**: 현재는 **생명주기 콜백**이다. 여러 구독자의 등록·해제와 알림을 갖는 자체 Observer 이벤트 시스템은 분석 범위에서 확인되지 않았다.

---

### 1.8 **Adapter 패턴** (적용 확인되지 않음)

`GameObjectPool<T>`가 제네릭 제약과 `IPoolable`을 사용하고 `MouseManager`가 마우스 조회를 감싸지만, 기존 인터페이스를 다른 인터페이스로 변환하는 GoF Adapter 객체는 확인되지 않는다.

---

### 1.9 **Command 패턴** (적용 확인되지 않음)

`UIManager.OnClickRetry()`는 `ScenesManager.LoadScene("Game")`을 직접 호출한다. 요청을 별도 명령 객체로 캡슐화하고 실행·보관하는 Command 구조는 확인되지 않는다.

---

## 2️⃣ 아키텍처 패턴

### 2.1 **Manager와 컴포넌트 기반 역할 분리**

```text
Manager / 조율:
├─ GameManager       게임 상태·점수                         Singleton
├─ UIManager         씬 UI 화면·입력                        씬 범위
├─ ScenesManager     씬 전환                                Singleton
├─ FadeManager       화면 페이드                            Singleton
├─ MapManager        행 생성·반환 조율                      씬 컴포넌트
├─ MouseManager      마우스 입력 조회                       Singleton
└─ ObjectPoolManager 소유자별 풀 등록·조회                   Singleton

Generator / 생성:
├─ MapGenerator, NatureGenerator
├─ CarGenerator, LogGenerator, LotusGenerator, TrainGenerator
└─ RiverController: 강 충돌 처리와 Splash 풀

Component / 실행:
├─ PlayerController, CameraTargetController
├─ CarMovement, LogMovement, LotusMovement, TrainMovement
├─ RowObject, NaturesReturn, SpalshPool
└─ CrossingGateMovement

공용 기능과 데이터:
├─ GameObjectPool<T>, IPoolable, Singleton<T>
└─ RowData ScriptableObject
```

`LodingManager_Del`도 코드에 존재하지만 `MonoBehaviour`이며 Singleton이 아니다. 위 구조는 명확히 분리된 MVC 구현보다는 Unity 컴포넌트의 역할 분리와 관리자 간 직접 호출에 가깝다.

### 2.2 **Data-Driven Design**

[RowData.cs](../Assets/Scripts/Map/RowData.cs)의 ScriptableObject가 행 종류·프리팹·가중치·반복 범위를 보관한다. `MapGenerator`는 설정 검증 후 가중치 합계에서 난수를 뽑아 행 종류를 선택한다.

**현재 [Game 씬](../Assets/Scenes/Game.unity)의 참조 에셋**:

| 에셋 | 행 종류 | 선택 가중치 | 반복 범위 |
|---|---|---:|---:|
| [Asphalt](../Assets/Scripts/ScriptaleObject/Asphalt.asset) | Road | 30 | 1~2 |
| [GrassData](../Assets/Scripts/ScriptaleObject/GrassData.asset) | Grass | 40 | 1~2 |
| [RailData](../Assets/Scripts/ScriptaleObject/RailData.asset) | Rail | 10 | 1 |
| [River1Data](../Assets/Scripts/ScriptaleObject/River1Data.asset) | River1 | 15 | 1 |
| [River2Data](../Assets/Scripts/ScriptaleObject/River2Data.asset) | River2 | 5 | 1 |

가중치 합이 100이므로 한 번의 **종류 선택** 비율은 각각 30%, 40%, 10%, 15%, 5%다. Road와 Grass는 1~2개 행을 반복 생성할 수 있으므로 실제 생성된 **개별 행 비율**은 선택 비율과 같다고 단정할 수 없다.

---

## 3️⃣ 게임 개발 패턴

### 3.1 **Pool & Reuse 패턴**

```text
생성 또는 대여 → 사용 → Return() → 비활성화·보관 → Get() → 재사용
```

행 반환 시 자연물은 다음 경로로 원래 풀에 돌아간다.

```text
MapGenerator.ReleaseRow()
  → NatureGenerator.Removenature()
    → NaturesReturn.ReturnToPool()
      → GameObjectPool.Return()
```

### 3.2 **Generator-Manager 협업 패턴**

- `MapManager`는 행을 큐에 보관하고 플레이어 위치에 따라 생성·반환을 조율한다.
- `MapGenerator`는 행 데이터를 선택하고 행을 대여하며, 각 행의 생성기가 해당 오브젝트를 담당한다.
- `ObjectPoolManager`는 생성기별 풀의 등록·조회와 소유자 파괴 시 등록 해제를 지원한다.
- `RiverController`는 비활성화 시 코루틴을 중지하고 활성 Splash를 반환한다.

### 3.3 **Input-Action 관련 구조**

`MouseManager`는 `Mouse.current`를 감싸 클릭·누름·릴리즈와 위치를 제공한다. `UIManager`는 클릭으로 게임을 시작하고, `PlayerController`는 입력 값을 이용해 스와이프와 이동을 처리한다. 현재 교체 가능한 입력 전략 구현체나 별도 Command 객체는 확인되지 않는다.

### 3.4 **Marker Component**

[RidePlatform](../Assets/Scripts/Map/Marker_Component/RidePlatform.cs)은 비어 있는 마커 컴포넌트다. `LogMovement`와 `LotusMovement`가 이를 상속하고 `PlayerController`는 충돌체 부모에서 이 컴포넌트를 찾아 탑승 대상을 식별한다.

---

## 4️⃣ 각 패턴의 상호작용

### 흐름 예시 1: 게임 시작과 행 생성

```text
1. UIManager.Update()
   └─ MouseManager.IsClicking()
      └─ GameManager.StartGame() → Playing / UI 갱신

2. MapManager.CheckPos()
   └─ MapGenerator.SpawnRow()
      ├─ RowData 가중치 선택 및 반복 횟수 결정
      ├─ ObjectPoolManager에서 행 풀 조회 → Get()
      └─ Grass인 경우 NatureGenerator에서 자연물 대여

3. 행에 있는 Generator의 활성화
   └─ 소유자별 풀 등록 또는 조회 → 객체 대여
```

`MapManager.CheckPos()`는 코드상 게임 상태로 제한되지 않는다. 또한 생성 콜백으로 생성기 참조를 주입하는 작업은 새 풀 객체를 만들 때 일어나며 매 대여 시 발생하지 않는다.

### 흐름 예시 2: 플레이어 사망과 재시도

```text
1. PlayerController 또는 RiverController가 충돌·낙하 등을 감지
   └─ GameManager.GameOver() → GameOver / 결과 UI 갱신

2. 강 충돌인 경우 RiverController.PlaySplash()
   └─ Splash 풀에서 대여 → 지연 후 반환

3. UIManager.OnClickRetry()
   └─ ScenesManager.LoadScene("Game")
      └─ 페이드·로딩 → GameManager.ResetGame() → Main
```

행 반환과 탑승 해제는 `MapManager.RemoveOldRows()` 등의 조건에 따라 별도로 실행된다. `GameOver()`가 모든 풀 객체를 즉시 반환하는 흐름은 아니다.

---

## 5️⃣ 종합 평가

### 강점

| 항목 | 현재 구조 | 장점 |
|---|---|---|
| 객체 재사용 | `GameObjectPool<T>`와 7개 `IPoolable` 타입 | 반복 생성·파괴를 줄일 수 있음 |
| 풀 관리 | 소유 컴포넌트와 이름으로 등록·조회 | 여러 행의 같은 이름 풀이 충돌하지 않음 |
| 행 설정 | `RowData` ScriptableObject | 가중치·반복 범위·프리팹을 데이터로 설정 |
| 역할 분리 | Manager, Generator, Movement 컴포넌트 | 조율·생성·동작의 책임을 구분 |
| 생명주기 | `IPoolable` 콜백 | 풀과 객체별 초기화 로직을 구분 |

### 개선 가능 영역

| 항목 | 현재 상태 | 권장 확인 |
|---|---|---|
| 열차 재사용 | `OnGet()`에서 시작 위치를 원점으로 설정하며 `Start()`는 재사용 시 다시 호출되지 않음 | 새 스폰 위치를 기준으로 거리 판정이 이뤄지는지 플레이 테스트 |
| Splash 콜백 | 풀의 `Get()`과 `RiverController.PlaySplash()`가 각각 `OnGet()` 호출 | 파티클 재생에 미치는 영향과 의도 확인 |
| 객체별 초기화 | 차량·통나무의 속도는 `Start()`에서 설정, 연꽃 `ResetState()`는 비어 있음 | 재사용 시 의도한 값이 유지·초기화되는지 검토 |
| 풀 관측성 | 관리자가 이름과 타입만 출력 | 필요 시 활성 수·잔여 수·확장 횟수 진단 검토 |
| 전역 의존성 | 싱글톤과 직접 호출 사용 | 테스트나 초기화 문제가 실제로 발생할 때 의존성 분리 검토 |

### 현재 구현 현황 분석

**✅ 확인된 부분**:

1. `Singleton<T>` 상속 클래스 5개와 씬 범위 `UIManager`
2. `GameObjectPool<T>`와 `IPoolable` 구현 클래스 7개
3. 소유자별 풀 등록·조회·등록 해제 및 자연물의 실제 풀 반환
4. `RowData` 기반 가중치 선택과 enum 기반 게임 상태 관리
5. 생성 함수 주입, 생명주기 콜백, Unity 컴포넌트 조합

**⚠️ 구분이 필요한 부분**:

1. enum 상태 분기와 GoF State는 다르다.
2. 생성기와 GoF Factory Method, 직접 콜백과 Observer는 각각 다르다.
3. Strategy, Adapter, Command는 현재 코드에서 적용으로 확인하지 못했다.
4. 현재 작업 트리의 [Rail.prefab](../Assets/Prefab/Road/Rail.prefab)에는 `TrainGenerator._root` 참조가 직렬화되어 있다. 이번 분석에서는 실제 열차 생성·반환을 실행 검증하지 않았다.

---

## 6️⃣ 결론

프로젝트의 핵심 구조는 **싱글톤 서비스, 소유자별 중앙 풀 레지스트리, 공용 Object Pool, 데이터 기반 행 생성, enum 기반 상태 관리**다. 생성 함수 주입, 생명주기 콜백과 Unity 컴포넌트 조합도 활용한다.

패턴 수나 별점으로 품질을 단정하기보다 풀 소유권, 재사용 초기화, 씬 전환 시 수명과 실제 플레이 동작을 확인하는 것이 중요하다.

### 추천 개선 순서

**[🟠 우선 확인]**
1. 원점에서 먼 행까지 포함해 열차 재사용 후 거리 판정을 테스트한다.
2. Splash의 중복 `OnGet()` 호출이 의도된 동작인지 확인한다.

**[🟡 필요 시 검토]**
3. 객체별 재사용 상태와 씬 전환 시 풀 수명을 점검한다.
4. 풀 진단 정보와 의존성 분리는 실제 운영·테스트 필요에 따라 검토한다.
5. 현재 구현의 GC 영향이 필요하면 같은 부하에서 Development Build를 재측정한다.

**[🔵 추가 패턴]**
- 현재 코드만으로 새로운 GoF 패턴 도입이 필요하다고 단정하지 않는다.

---

- **분석 작성**: 2026-11-23
- **프로젝트**: Crossy Road (Unity 6000.4.0f1)
- **분석 기준**: 현재 작업 트리 정적 분석
