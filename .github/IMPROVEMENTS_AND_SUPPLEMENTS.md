# 🔧 Crossy Road 프로젝트 - 개선 및 보완 가이드

## 📌 개요

기존 디자인 패턴 분석을 기반으로, 현재 구현 상태를 더욱 견고하고 유지보수하기 쉽게 개선할 수 있는 방안을 제시합니다.

**작성 기준**: 현재 코드 상태 (2026-11-19)  
**우선순위**: 1순위 (필수) → 2순위 (권장) → 3순위 (선택적)

---

## 1️⃣ 1순위 개선 사항 (필수)
/
### 1.1 **IPoolable 인터페이스 활용 체계화** ⭐⭐⭐

**현재 상태**:
```csharp
// IPoolable이 정의되어 있으나 사용하지 않음
public interface IPoolable
{
    void OnCreate();
    void OnGet();
    void OnReturn();
}
```

**문제점**:
- ❌ 여러 오브젝트가 개별적으로 초기화 메서드 보유
- ❌ 풀 사용 시 초기화 패턴이 일관되지 않음
- ❌ 새로운 개발자가 풀링 오브젝트 생성 시 헷갈림

**개선 방안**:

```csharp
// Step 1: IPoolable 적용
public interface IPoolable
{
    /// <summary>풀에서 처음 생성될 때 호출</summary>
    void OnCreate();
    
    /// <summary>풀에서 꺼내져 활성화될 때 호출</summary>
    void OnGet();
    
    /// <summary>풀로 반환되기 전 정리 작업</summary>
    void OnReturn();
}

// Step 2: GameObjectPool 개선
public class GameObjectPool<T> where T : Component, IPoolable
{
    private void CreateObject()
    {
        T obj = _createFunction();
        if(obj == null)
            throw new InvalidOperationException("Create function returned null.");

        obj.OnCreate();  // ← IPoolable 호출
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        T obj;
        if (_pool.Count > 0)
            obj = _pool.Dequeue();
        else
            obj = CreateObject();

        _activeObjects.Add(obj);
        obj.OnGet();  // ← IPoolable 호출
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null || !_activeObjects.Remove(obj))
            return;

        obj.OnReturn();  // ← IPoolable 호출
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_poolRoot);
        obj.transform.rotation = Quaternion.identity;
        _pool.Enqueue(obj);
    }
}

// Step 3: 각 Movement 클래스에 구현
public class CarMovement : RidePlatform, IPoolable
{
    private CarGenerator _carGenerator;
    
    public void OnCreate()
    {
        // 초기 컴포넌트 캐싱 등
    }
    
    public void OnGet()
    {
        // 활성화 직전 초기화 (상태 리셋)
        ResetState();
    }
    
    public void OnReturn()
    {
        // 비활성화 직전 정리 (이벤트 구독 해제 등)
        // 현재는 별도 작업 없으나, 향후 복잡해지면 필요
    }
    
    public void ResetState()
    {
        // 기존 로직 유지
    }
}

// Step 4: Generator에서 단순화
public void RemoveCar(CarMovement car)
{
    // IPoolable의 OnReturn()이 자동 호출되므로
    // 별도 초기화 불필요
    _carsPools.Return(car);  // ← 이것만으로 충분
}
```

**적용 대상**:
- ✓ CarMovement
- ✓ LogMovement
- ✓ LotusMovement
- ✓ TrainMovement
- ✓ NatureGenerator 하위 오브젝트들

**효과**:
- ✅ 풀링 오브젝트의 생명주기 명확화
- ✅ 초기화 로직 중복 제거
- ✅ 새로운 풀링 오브젝트 추가 시 체계적 접근
- ✅ 타입 안정성 향상 (IPoolable 강제)

**예상 소요 시간**: 1-2시간

---

### 1.2 **ObjectPoolManager 완성** ⭐⭐⭐

**현재 상태**:
```csharp
public class ObjectPool<T> where T : Component
{
    // 비어있음 - 선언만 되어 있음
}
```

**문제점**:
- ❌ 현재 각 Generator에서 독립적으로 GameObjectPool 생성
- ❌ 전역 풀 관리 체계 부재
- ❌ 필요 시 풀 접근이 어려움
- ❌ 메모리 프로파일링 어려움

**개선 방안**:

```csharp
/// <summary>모든 GameObjectPool을 중앙에서 관리</summary>
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<string, object> _pools = new Dictionary<string, object>();
    
    /// <summary>풀 등록</summary>
    public void RegisterPool<T>(string poolName, GameObjectPool<T> pool) 
        where T : Component, IPoolable
    {
        if (_pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool '{poolName}' is already registered. Overwriting.");
        }
        _pools[poolName] = pool;
    }
    
    /// <summary>풀 조회</summary>
    public GameObjectPool<T> GetPool<T>(string poolName) 
        where T : Component, IPoolable
    {
        if (_pools.TryGetValue(poolName, out var pool))
        {
            return pool as GameObjectPool<T>;
        }
        Debug.LogError($"Pool '{poolName}' not found.");
        return null;
    }
    
    /// <summary>모든 풀의 통계</summary>
    public void PrintPoolStatistics()
    {
        Debug.Log("=== Pool Statistics ===");
        foreach (var pair in _pools)
        {
            Debug.Log($"{pair.Key}: {pair.Value.GetType().Name}");
        }
    }
}

// CarGenerator에서 사용
public class CarGenerator : MonoBehaviour
{
    void OnEnable()
    {
        // 기존: 풀을 여기서 생성
        // _carsPools = new GameObjectPool<CarMovement>(...);
        
        // 개선: 풀을 ObjectPoolManager에 등록
        _carsPools = new GameObjectPool<CarMovement>(_root, _initialSize, 
            () => Instantiate(_cars[Random.Range(0, _cars.Count)], _root)
                    .GetComponent<CarMovement>(),
            car => car.InitCar(this));
        
        ObjectPoolManager.Instance.RegisterPool("CarPool", _carsPools);
    }
}

// 필요한 곳에서 조회
public class GameDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ObjectPoolManager.Instance.PrintPoolStatistics();
        }
    }
}
```

**적용 대상**:
- ✓ CarGenerator의 카 풀
- ✓ LogGenerator의 나무통 풀
- ✓ LotusGenerator의 연꽃 풀
- ✓ NatureGenerator의 자연물 풀들
- ✓ MapGenerator의 행 풀들
- ✓ RiverController의 Splash 풀
- ✓ TrainGenerator의 기차 풀

**효과**:
- ✅ 풀 접근 중앙화
- ✅ 디버깅 용이
- ✅ 메모리 프로파일링 가능
- ✅ 풀 상태 모니터링 가능

**예상 소요 시간**: 30분-1시간

---

### 1.3 **State 패턴 강화 - Enum 기반 → 클래스 기반 전환 (선택사항)** ⭐⭐

**현재 상태**:
```csharp
public enum GameState
{
    Main,
    Playing,
    GameOver
}

// GameManager에서 switch-case로 처리
public void ChangeState(GameState state)
{
    CurrentState = state;
    switch (state)
    {
        case GameState.Main:
            UIManager._UM.ShowMain();
            break;
        // ...
    }
}
```

**잠재적 문제**:
- ⚠️ 상태 추가 시 GameManager를 수정해야 함
- ⚠️ 각 상태의 진입/종료 로직이 한 곳에 집중
- ⚠️ 상태별 복잡한 로직이 추가되면 switch-case가 비대해짐

**개선 방안 (고급)** - 현재는 필수 아님:

```csharp
/// <summary>상태의 기본 인터페이스</summary>
public interface IGameState
{
    void Enter();   // 상태 진입 시
    void Update();  // 매 프레임
    void Exit();    // 상태 종료 시
}

/// <summary>메인 상태</summary>
public class MainState : IGameState
{
    private UIManager _uiManager;
    
    public MainState(UIManager uiManager)
    {
        _uiManager = uiManager;
    }
    
    public void Enter()
    {
        _uiManager.ShowMain();
        Debug.Log("Main State Entered");
    }
    
    public void Update()
    {
        if (MouseManager.Instance.IsClicking())
        {
            // 게임 시작 준비
        }
    }
    
    public void Exit()
    {
        Debug.Log("Main State Exited");
    }
}

/// <summary>플레이 상태</summary>
public class PlayingState : IGameState
{
    private UIManager _uiManager;
    private MapManager _mapManager;
    
    public PlayingState(UIManager uiManager, MapManager mapManager)
    {
        _uiManager = uiManager;
        _mapManager = mapManager;
    }
    
    public void Enter()
    {
        _uiManager.ShowPlaying();
        _mapManager.StartMapGeneration();
        Debug.Log("Playing State Entered");
    }
    
    public void Update()
    {
        // 플레이 중 로직
    }
    
    public void Exit()
    {
        _mapManager.StopMapGeneration();
        Debug.Log("Playing State Exited");
    }
}

/// <summary>상태를 관리하는 State Machine</summary>
public class GameStateMachine
{
    private IGameState _currentState;
    
    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
    
    public void Update()
    {
        _currentState?.Update();
    }
}

// GameManager에서 사용
public class GameManager : Singleton<GameManager>
{
    private GameStateMachine _stateMachine;
    private MainState _mainState;
    private PlayingState _playingState;
    private GameOverState _gameOverState;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeStates();
    }
    
    private void InitializeStates()
    {
        _stateMachine = new GameStateMachine();
        _mainState = new MainState(UIManager._UM);
        _playingState = new PlayingState(UIManager._UM, MapManager.Instance);
        _gameOverState = new GameOverState(UIManager._UM);
    }
    
    public void StartGame()
    {
        _stateMachine.ChangeState(_playingState);
    }
    
    public void GameOver()
    {
        _stateMachine.ChangeState(_gameOverState);
    }
    
    void Update()
    {
        _stateMachine.Update();
    }
}
```

**적용 시기**: 상태가 더 복잡해질 때 (현재는 선택사항)

**효과**:
- ✅ Open-Closed Principle 준수
- ✅ 각 상태의 로직 캡슐화
- ✅ 새 상태 추가 시 기존 코드 수정 불필요
- ✅ 단위 테스트 용이

---

## 2️⃣ 2순위 개선 사항 (권장)

### 2.1 **이벤트 시스템 도입 - Observer 패턴 강화**

**현재 상태**:
```csharp
// 초기화 콜백만 사용
GameObjectPool<CarMovement> _carsPools = new GameObjectPool<CarMovement>(
    _root, _initialSize,
    () => Instantiate(_cars[Random.Range(0, _cars.Count)], _root)
            .GetComponent<CarMovement>(),
    car => car.InitCar(this)  // ← 콜백만 사용
);
```

**문제점**:
- ⚠️ 여러 시스템이 같은 이벤트를 기다릴 때 복잡해짐
- ⚠️ 느슨한 결합이 어려움
- ⚠️ 디버깅 시 이벤트 흐름 추적 어려움

**개선 방안**:

```csharp
/// <summary>게임 이벤트 모음</summary>
public static class GameEvents
{
    public static UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public static UnityEvent OnGameStarted = new UnityEvent();
    public static UnityEvent OnGameOver = new UnityEvent();
    public static UnityEvent<float> OnPlayerMoved = new UnityEvent<float>();
    public static UnityEvent<CarMovement> OnCarSpawned = new UnityEvent<CarMovement>();
    public static UnityEvent<CarMovement> OnCarDespawned = new UnityEvent<CarMovement>();
}

// CarGenerator에서 사용
public void SpawnCar(float angle)
{
    var car = _carsPools.Get();
    if (car == null) return;

    car.transform.position = _randomPoint.position;
    car.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    
    // 이벤트 발행
    GameEvents.OnCarSpawned.Invoke(car);
}

public void RemoveCar(CarMovement car)
{
    // 이벤트 발행
    GameEvents.OnCarDespawned.Invoke(car);
    
    car.ResetState();
    _carsPools.Return(car);
}

// GameManager에서 구독
protected override void Awake()
{
    base.Awake();
    GameEvents.OnGameOver.AddListener(HandleGameOver);
}

private void HandleGameOver()
{
    Debug.Log("Game Over Event Received");
    ChangeState(GameState.GameOver);
}

void OnDestroy()
{
    GameEvents.OnGameOver.RemoveListener(HandleGameOver);
}
```

**효과**:
- ✅ 느슨한 결합 (Loose Coupling)
- ✅ 다중 구독자 지원
- ✅ 디버깅 용이
- ✅ 새로운 시스템 추가 시 기존 코드 수정 불필요

**예상 소요 시간**: 2-3시간

---

### 2.2 **Singleton 의존성 감소 - 의존성 주입(DI) 도입**

**현재 상태**:
```csharp
// 여러 곳에서 Singleton으로 직접 접근
GameManager.Instance.StartGame();
UIManager._UM.ShowMain();
ScenesManager.Instance.LoadScene("Game");
MouseManager.Instance.IsClicking();
```

**문제점**:
- ⚠️ 테스트 어려움 (실제 Singleton 필요)
- ⚠️ 순환 의존성 위험
- ⚠️ 메모리 누수 가능성
- ⚠️ 단위 테스트 어려움

**개선 방안 (고급)** - 단계적 적용:

```csharp
// Step 1: GameManager의 필드를 public property로
public class GameManager : Singleton<GameManager>
{
    public event System.Action<GameState> OnStateChanged;
    
    private GameState _currentState;
    public GameState CurrentState => _currentState;
    
    public void ChangeState(GameState state)
    {
        _currentState = state;
        OnStateChanged?.Invoke(state);
    }
}

// Step 2: 필요한 시스템들을 주입받기
public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;
    
    void Awake()
    {
        _gameManager = GameManager.Instance;
        _gameManager.OnStateChanged += HandleStateChanged;
    }
    
    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Main:
                ShowMain();
                break;
            case GameState.Playing:
                ShowPlaying();
                break;
            case GameState.GameOver:
                ShowGameOver();
                break;
        }
    }
    
    void OnDestroy()
    {
        _gameManager.OnStateChanged -= HandleStateChanged;
    }
}

// Step 3: 테스트용 목(Mock) 버전
#if UNITY_EDITOR
public class MockGameManager
{
    private GameState _currentState;
    public event System.Action<GameState> OnStateChanged;
    
    public void ChangeState(GameState state)
    {
        _currentState = state;
        OnStateChanged?.Invoke(state);
    }
}
#endif
```

**적용 시기**: 테스트 가능성이 필요할 때

**효과**:
- ✅ 단위 테스트 가능
- ✅ 느슨한 결합
- ✅ 순환 의존성 방지

---

### 2.3 **문서화 강화 - XML 주석 추가**

**현재 상태**:
```csharp
public class GameObjectPool<T> where T : Component
{
    public T Get()  // 설명 없음
    public void Return(T obj)  // 설명 없음
}
```

**개선 방안**:

```csharp
/// <summary>
/// 제네릭 기반 오브젝트 풀 클래스
/// 
/// 반복적으로 생성/삭제되는 컴포넌트를 미리 풀에서 관리하여
/// 메모리 할당을 최소화합니다.
/// </summary>
/// <typeparam name="T">풀에서 관리할 컴포넌트 타입 (IPoolable 구현 필수)</typeparam>
public class GameObjectPool<T> where T : Component, IPoolable
{
    /// <summary>풀에서 사용 가능한 오브젝트 개수</summary>
    public int Available => _pool.Count;
    
    /// <summary>현재 활성화된 오브젝트 개수</summary>
    public int ActiveCount => _activeObjects.Count;

    /// <summary>
    /// 풀에서 오브젝트를 꺼냅니다.
    /// 
    /// 풀에 사용 가능한 오브젝트가 있으면 반환하고,
    /// 없으면 새로 생성합니다.
    /// </summary>
    /// <returns>활성화된 오브젝트</returns>
    public T Get()
    {
        // 구현...
    }

    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// 
    /// 오브젝트를 비활성화하고 풀에 추가합니다.
    /// null 체크와 중복 반환 체크가 자동으로 수행됩니다.
    /// </summary>
    /// <param name="obj">반환할 오브젝트</param>
    public void Return(T obj)
    {
        // 구현...
    }
}
```

**적용 대상**: 모든 public 메서드와 클래스

**효과**:
- ✅ IDE의 인텔리센스 개선
- ✅ 개발 효율 증가
- ✅ 새로운 개발자 온보딩 용이
- ✅ 자동 문서 생성 가능

---

## 3️⃣ 3순위 개선 사항 (선택적)

### 3.1 **컴포넌트 풀링 추가**

**고려사항**:
```csharp
// 현재: Transform과 GameObjectOnly 풀링
// 개선: 더 세분화된 컴포넌트 풀링

// 예: Animator 컴포넌트 풀
GameObjectPool<Animator> _animatorPool;

// 예: AudioSource 컴포넌트 풀 (많은 효음음이 필요한 경우)
GameObjectPool<AudioSource> _audioPool;
```

---

### 3.2 **디버그 시스템 강화**

```csharp
public class PoolDebugger : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Pool Stats"))
        {
            ObjectPoolManager.Instance.PrintPoolStatistics();
        }
    }
}
```

---

### 3.3 **성능 모니터링**

```csharp
public class PoolPerformanceMonitor : MonoBehaviour
{
    private float _peakMemoryUsage;
    private float _currentMemoryUsage;
    
    void Update()
    {
        _currentMemoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        _peakMemoryUsage = Mathf.Max(_peakMemoryUsage, _currentMemoryUsage);
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"Current: {_currentMemoryUsage:F2}MB, Peak: {_peakMemoryUsage:F2}MB");
        }
    }
}
```

---

## 4️⃣ 구현 로드맵

### Phase 1: 필수 (1-2주)
- [ ] IPoolable 인터페이스 체계화
- [ ] ObjectPoolManager 완성
- [ ] 모든 오브젝트가 IPoolable 구현하도록 수정

### Phase 2: 권장 (2-3주)
- [ ] 이벤트 시스템 도입
- [ ] XML 주석 추가
- [ ] 단위 테스트 작성 (선택)

### Phase 3: 선택적 (필요시)
- [ ] State 클래스 기반 전환
- [ ] DI 컨테이너 도입
- [ ] 성능 모니터링 도구 추가

---

## 5️⃣ 체크리스트

### IPoolable 체계화
```
[ ] IPoolable 인터페이스 정의 확인
[ ] GameObjectPool 개선
[ ] CarMovement - IPoolable 구현
[ ] LogMovement - IPoolable 구현
[ ] LotusMovement - IPoolable 구현
[ ] TrainMovement - IPoolable 구현
[ ] 테스트 및 검증
```

### ObjectPoolManager 완성
```
[ ] ObjectPoolManager 클래스 작성
[ ] RegisterPool 메서드 구현
[ ] GetPool 메서드 구현
[ ] PrintPoolStatistics 메서드 구현
[ ] 모든 Generator에서 RegisterPool 호출 추가
[ ] 테스트 및 검증
```

### 이벤트 시스템
```
[ ] GameEvents 클래스 작성
[ ] 필요한 이벤트 정의
[ ] CarGenerator에서 이벤트 발행
[ ] GameManager에서 이벤트 구독
[ ] 추가 시스템에서 구독 (필요 시)
[ ] 테스트 및 검증
```

---

## 6️⃣ 주의사항

### ⚠️ 주의점 1: 하위 호환성 유지
모든 개선은 **기존 기능을 손상시키지 않으면서** 점진적으로 적용해야 합니다.

```csharp
// 기존 코드 유지
var car = _carsPools.Get();

// 새로운 방식도 지원
// (동시에 작동)
```

### ⚠️ 주의점 2: 과도한 최적화 피하기
필요한 개선만 적용하세요:
- Singleton이 필요한가? → 필요. 유지.
- 클래스 기반 State? → 선택사항.
- DI 컨테이너? → 고급 개발자를 위한 선택사항.

### ⚠️ 주의점 3: 테스트 필수
모든 개선 후 반드시 테스트:
```
[ ] 게임 시작 정상
[ ] 오브젝트 생성/소멸 정상
[ ] 메모리 누수 없음
[ ] 프레임율 개선 또는 유지
[ ] 모든 플레이스타일 테스트 (일반 플레이, 장시간 플레이 등)
```

---

## 7️⃣ 결론

| 항목 | 우선순위 | 복잡도 | 효과 | 소요시간 |
|------|---------|--------|------|---------|
| **IPoolable 체계화** | 🔴 필수 | ⭐⭐ | ⭐⭐⭐⭐⭐ | 1-2h |
| **ObjectPoolManager** | 🔴 필수 | ⭐⭐ | ⭐⭐⭐⭐ | 1h |
| **이벤트 시스템** | 🟡 권장 | ⭐⭐⭐ | ⭐⭐⭐⭐ | 2-3h |
| **XML 문서화** | 🟡 권장 | ⭐ | ⭐⭐⭐ | 1-2h |
| **State 클래스화** | 🟢 선택 | ⭐⭐⭐⭐ | ⭐⭐⭐ | 2-3h |
| **DI 도입** | 🟢 선택 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | 3-4h |

**권장 접근**:
1. **1순위 항목을 모두 완료** (약 2-3시간)
2. **2순위 항목 중 필요한 것 선택** (필요 시 2-3시간 추가)
3. **3순위 항목은 필요할 때만 적용**

---

**작성일**: 2026-11-19  
**프로젝트**: Crossy Road (Unity 6)  
**다음 단계**: 위 항목 중 1순위부터 순차적으로 적용
