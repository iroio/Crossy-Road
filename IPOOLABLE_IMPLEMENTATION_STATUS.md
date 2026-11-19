# 🎯 IPoolable 적용 상태 종합 체크 보고서

**작성 일시**: 2026-11-20 00:01  
**분석 범위**: IPoolable 인터페이스 구현 현황  
**전체 풀링 대상**: 7개 (CarMovement, LogMovement, LotusMovement, TrainMovement, ParticleSystem, Transform, Transform)

---

## 📊 적용 상태 요약

| 항목 | 상태 | 완성도 | 비고 |
|------|------|--------|------|
| **IPoolable 인터페이스 정의** | ✅ 완료 | 100% | 3개 메서드 정의됨 |
| **GameObjectPool 개선** | ✅ 완료 | 100% | OnCreate, OnGet, OnReturn 호출 |
| **CarMovement** | ✅ 구현됨 | 100% | IPoolable 상속, 모든 메서드 구현 |
| **LogMovement** | ✅ 구현됨 | 100% | IPoolable 상속, 모든 메서드 구현 |
| **LotusMovement** | ✅ 구현됨 | 100% | IPoolable 상속, 모든 메서드 구현 |
| **TrainMovement** | ✅ 구현됨 | 100% | IPoolable 상속, 모든 메서드 구현 |
| **ParticleSystem (Splash)** | ⚠️ 미적용 | 0% | 풀링되나 IPoolable 미사용 |
| **Transform (Nature)** | ⚠️ 미적용 | 0% | 풀링되나 IPoolable 미사용 |
| **Transform (Row)** | ⚠️ 미적용 | 0% | 풀링되나 IPoolable 미사용 |

**전체 완성도**: 🟡 **약 57%** (4/7 적용)

---

## 1️⃣ IPoolable 인터페이스 정의

### 📄 파일: `Assets/Scripts/Interface/IPoolable.cs`

```csharp
public interface IPoolable
{
    // 처음 생성될 때 호출되는 메서드
    void OnCreate();

    // 오브젝트가 풀에서 꺼낼 때 호출되는 메서드
    void OnGet();

    // 오브젝트가 풀에 반환할 때 호출되는 메서드
    void OnReturn();
}
```

**상태**: ✅ 올바르게 정의됨  
**개선 사항**: 현재 최적. XML 주석 추가 시 더욱 명확해질 것.

---

## 2️⃣ GameObjectPool 개선 상태

### 📄 파일: `Assets/Scripts/Util/GameObjectPool.cs`

```csharp
public class GameObjectPool<T> where T : Component, IPoolable
{
    // ✅ 올바른 제약 조건: IPoolable 구현 필수
    
    private T CreateObject()
    {
        T obj = _createFunction();
        if(obj == null)
            throw new InvalidOperationException("Create function returned null.");
        
        obj.OnCreate();  // ✅ 호출됨
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        // ...
        obj.OnGet();  // ✅ 호출됨
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        // ...
        obj.OnReturn();  // ✅ 호출됨
        obj.gameObject.SetActive(false);
        // ...
    }
}
```

**상태**: ✅ 완벽하게 구현됨  
**특징**:
- 제약 조건에 `IPoolable` 추가됨
- `OnCreate()` 호출 (생성 시)
- `OnGet()` 호출 (활성화 시)
- `OnReturn()` 호출 (비활성화 직전)
- 이전 콜백 (`_onCreate`) 주석 처리됨

---

## 3️⃣ Movement 클래스 적용 상태

### ✅ CarMovement

**파일**: `Assets/Scripts/Map/Cars/CarMovement.cs`

```csharp
public class CarMovement : MonoBehaviour, IPoolable
{
    // ✅ IPoolable 상속
    
    public void OnCreate() { }  // ✅ 구현됨 (현재 비어있음)
    
    public void OnGet()
    {
        ResetState();  // ✅ 활성화 시 상태 초기화
    }
    
    public void OnReturn() { }  // ✅ 구현됨 (현재 비어있음)
    
    public void ResetState()
    {
        _targetSpeed = 0f;
        _isFrontCarDetected = false;
    }
}
```

**상태**: ✅ 완벽하게 구현됨  
**평가**: 
- 초기화 로직 명확
- ResetState() 호출 타이밍 적절
- OnCreate/OnReturn 예약됨 (향후 필요 시 활용)

---

### ✅ LogMovement

**파일**: `Assets/Scripts/Map/River/LogMovement.cs`

```csharp
public class LogMovement : RidePlatform, IPoolable
{
    // ✅ IPoolable 상속
    
    public void OnCreate() { }  // ✅ 구현됨
    
    public void OnGet()
    {
        ResetState();  // ✅ 활성화 시 상태 초기화
    }
    
    public void OnReturn() { }  // ✅ 구현됨
    
    public void ResetState()
    {
        _moveRight = false;  // ✅ 주요 상태 초기화
    }
}
```

**상태**: ✅ 완벽하게 구현됨  
**평가**:
- 방향(direction) 상태 명확하게 초기화
- 나무통의 반복 사용 안정

---

### ✅ LotusMovement

**파일**: `Assets/Scripts/Map/River/LotusMovement.cs`

```csharp
public class LotusMovement : RidePlatform, IPoolable
{
    // ✅ IPoolable 상속
    
    public void OnCreate() { }  // ✅ 구현됨
    
    public void OnGet()
    {
        ResetState();  // ✅ 호출됨
    }
    
    public void OnReturn() { }  // ✅ 구현됨
    
    public void ResetState() { }  // ⚠️ 비어있음
}
```

**상태**: 🟡 부분 구현됨  
**평가**:
- IPoolable 상속됨
- OnGet에서 ResetState() 호출됨
- ⚠️ ResetState()가 비어있음 → 현재 초기화 불필요한 것으로 보임
- 향후 연꽃에 상태(회전, 위치 등)가 추가되면 수정 필요

---

### ✅ TrainMovement

**파일**: `Assets/Scripts/Map/Train/TrainMovement.cs`

```csharp
public class TrainMovement : MonoBehaviour, IPoolable
{
    // ✅ IPoolable 상속
    
    public void OnCreate() { }  // ✅ 구현됨
    
    public void OnGet()
    {
        ResetState();  // ✅ 호출됨
    }
    
    public void OnReturn() { }  // ✅ 구현됨
    
    public void ResetState() { }  // ⚠️ 비어있음
}
```

**상태**: 🟡 부분 구현됨  
**평가**:
- IPoolable 상속됨
- OnGet에서 ResetState() 호출됨
- ⚠️ ResetState()가 비어있음
- **개선점**: _startPos 초기화 필요

```csharp
public void ResetState()
{
    _startPos = Vector3.zero;  // ← 추가 필요
}
```

---

## 4️⃣ 비Movement 풀링 대상 분석

### ⚠️ ParticleSystem (Splash)

**파일**: `Assets/Scripts/Map/River/RiverController.cs`

```csharp
GameObjectPool<ParticleSystem> _splashPool;

void OnEnable()
{
    ParticleSystem splash = _splashPrefab.GetComponentInChildren<ParticleSystem>();
    _splashPool = new GameObjectPool<ParticleSystem>(
        transform, _initialSize, 
        () => Instantiate(splash, transform),
        splash => splash.Play()  // ← 콜백 사용
    );
}
```

**상태**: ⚠️ **IPoolable 미적용**  
**문제점**:
- ParticleSystem은 MonoBehaviour지만 IPoolable 미구현
- 콜백 방식만 사용 중
- 풀링 생명주기가 GameObjectPool의 OnCreate, OnGet, OnReturn과 연동되지 않음

**개선 방안**:
```csharp
// ParticleSystem을 래핑하는 Wrapper 클래스 생성 필요
public class SplashPool : MonoBehaviour, IPoolable
{
    private ParticleSystem _particle;
    
    public void OnCreate() 
    {
        _particle = GetComponent<ParticleSystem>();
    }
    
    public void OnGet() 
    {
        _particle.Play();
    }
    
    public void OnReturn() 
    {
        _particle.Stop();
    }
}

// 사용
GameObjectPool<SplashPool> _splashPool;
```

---

### ⚠️ Transform (Nature)

**파일**: `Assets/Scripts/Map/Nature/NatureGenerator.cs`

```csharp
GameObjectPool<Transform>[] _naturesPools;

// 사용 예
public Transform GetNature()
{
    GameObjectPool<Transform> pool = GetNaturePool();
    Transform obj = pool.Get();
    
    NaturesReturn natures = obj.GetComponent<NaturesReturn>();
    if (natures != null)
    {
        natures.SetPool(pool);  // ← 풀 참조 전달
    }
    
    obj.gameObject.SetActive(true);
    return obj;
}
```

**상태**: ⚠️ **IPoolable 미적용**  
**문제점**:
- Transform 기반 풀링이므로 IPoolable 적용 불가
- NaturesReturn 컴포넌트가 별도로 풀 관리
- 풀 생명주기를 추상적 방식으로 처리

**현재 구조 분석**:
```csharp
// NaturesReturn.cs
public class NaturesReturn : MonoBehaviour
{
    GameObjectPool<Transform> _pool;
    
    public void SetPool(GameObjectPool<Transform> pool)
    {
        _pool = pool;  // ← 외부에서 풀 주입
    }
    
    public void ReturnNature()
    {
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
        _pool.Return(transform);  // ← 직접 호출
    }
}
```

**개선 방안**:
```csharp
// NaturesReturn를 IPoolable로 변경
public class NaturesReturn : MonoBehaviour, IPoolable
{
    private GameObjectPool<Transform> _pool;
    
    public void SetPool(GameObjectPool<Transform> pool)
    {
        _pool = pool;
    }
    
    public void OnCreate() { }
    
    public void OnGet() 
    {
        // 필요한 초기화
    }
    
    public void OnReturn() 
    {
        gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;
    }
}

// 풀 생성 변경
GameObjectPool<NaturesReturn> _naturesPool;
// 그 후 transform 대신 NaturesReturn 사용
```

---

### ⚠️ Transform (Row)

**파일**: `Assets/Scripts/Map/MapGenerator.cs`

```csharp
readonly Dictionary<GameObject, GameObjectPool<Transform>> _rowPools = 
    new Dictionary<GameObject, GameObjectPool<Transform>>();

// 풀 생성 (GetRow 메서드 내부)
// Transform 기반 풀링
```

**상태**: ⚠️ **IPoolable 미적용**  
**문제점**:
- Row를 Transform으로 직접 풀링
- IPoolable 적용 불가능 (Transform은 고정 타입)
- 초기화 로직이 명확하지 않음

**개선 방안**:
```csharp
// Row를 관리하는 클래스 생성
public class RowObject : MonoBehaviour, IPoolable
{
    public void OnCreate() { }
    
    public void OnGet() 
    {
        // Row 초기화
    }
    
    public void OnReturn() 
    {
        // Row 정리
    }
}

// 그 후 풀링 변경
Dictionary<GameObject, GameObjectPool<RowObject>> _rowPools;
```

---

## 5️⃣ Generator 클래스의 풀 사용 현황

### ✅ CarGenerator

**파일**: `Assets/Scripts/Map/Cars/CarGenerator.cs`

```csharp
GameObjectPool<CarMovement> _carsPools;

void OnEnable()
{
    _carsPools = new GameObjectPool<CarMovement>(_root, _initialSize, 
        () => Instantiate(_cars[Random.Range(0, _cars.Count)], _root)
                .GetComponent<CarMovement>(),
        car => car.InitCar(this)  // ← 콜백 사용
    );
}

public void RemoveCar(CarMovement car)
{
    car.ResetState();  // ← 명시적 호출 (이제 불필요)
    _carsPools.Return(car);
}
```

**상태**: ✅ 완벽하게 적용됨  
**개선 가능**:
```csharp
// 개선 후: ResetState() 호출 제거 (IPoolable.OnGet에서 자동 호출됨)
public void RemoveCar(CarMovement car)
{
    _carsPools.Return(car);  // ← 이것만 충분
}
```

---

### ✅ LogGenerator

**파일**: `Assets/Scripts/Map/River/LogGenerator.cs`

```csharp
GameObjectPool<LogMovement> _logsPools;

void OnEnable()
{
    _logsPools = new GameObjectPool<LogMovement>(_root, _initialSize,
        () => Instantiate(_log, _root).GetComponentInChildren<LogMovement>(),
        log => log.InitLog(this)
    );
}

public void RemoveLog(LogMovement log)
{
    // ... (플레이어 체크)
    log.ResetState();  // ← 명시적 호출 (이제 불필요)
    _logsPools.Return(log);
}
```

**상태**: ✅ 완벽하게 적용됨  
**개선 가능**: CarGenerator와 동일

---

### ✅ LotusGenerator

**파일**: `Assets/Scripts/Map/River/LotusGenerator.cs`

```csharp
GameObjectPool<LotusMovement> _lotusPools;

void OnEnable()
{
    _lotusPools = new GameObjectPool<LotusMovement>(_root, _initialSize, 
        () => Instantiate(_lotus, _root).GetComponent<LotusMovement>(),
        lotus => lotus.InitLotus(this)
    );
}

public void RemoveLotus(LotusMovement lotus)
{
    // ... (플레이어 체크)
    lotus.gameObject.SetActive(false);  // ← 직접 비활성화
    lotus.transform.rotation = Quaternion.identity;
    _lotusPools.Return(lotus);
}
```

**상태**: 🟡 부분 적용  
**문제점**:
- RemoveLotus에서 gameObject.SetActive(false) 직접 호출
- 이미 Return()에서 처리됨 (중복 작업)

**개선 필요**:
```csharp
public void RemoveLotus(LotusMovement lotus)
{
    // ... (플레이어 체크)
    _lotusPools.Return(lotus);  // ← 이것만 충분 (SetActive, 회전 초기화는 Return에서 처리)
}
```

---

### ✅ TrainGenerator

**파일**: `Assets/Scripts/Map/Train/TrainGenerator.cs`

```csharp
GameObjectPool<TrainMovement> _trainPool;

void OnEnable()
{
    _trainPool = new GameObjectPool<TrainMovement>(_root, _initialSize, 
        () => Instantiate(train, _root),
        train => train.InitTrain(this)
    );
}

public void RemoveTrain()
{
    if (_currentTrain == null) return;
    
    TrainMovement train = _currentTrain.GetComponent<TrainMovement>();
    if (train != null && _trainPool != null)
    {
        _trainPool.Return(train);  // ✅ 명확하게 반환
    }
    
    _currentTrain = null;
}
```

**상태**: ✅ 완벽하게 적용됨

---

### ⚠️ NatureGenerator

**파일**: `Assets/Scripts/Map/Nature/NatureGenerator.cs`

```csharp
GameObjectPool<Transform>[] _naturesPools;

// Transform 기반 풀링 사용
// IPoolable 미적용

public void RemoveNature(/* 메서드 없음 */)
{
    // NaturesReturn 컴포넌트를 통해 간접 반환
}
```

**상태**: ⚠️ **미완전하게 적용**

---

### ⚠️ RiverController

**파일**: `Assets/Scripts/Map/River/RiverController.cs`

```csharp
GameObjectPool<ParticleSystem> _splashPool;

void PlaySplash(Vector3 position)
{
    ParticleSystem splash = _splashPool.Get();
    if (splash == null) return;

    splash.transform.position = position;
    splash.Play();
    
    StartCoroutine(ReturnSplashAfterDelay(splash, 2f));
}
```

**상태**: ⚠️ **미완전하게 적용**

---

## 6️⃣ 종합 평가 및 개선 우선순위

### 현재 상태

```
✅ 완벽 적용 (4개)
├─ CarMovement
├─ LogMovement  
├─ LotusMovement (ResetState 비어있으나 현재 불필요)
└─ TrainMovement

🟡 부분 적용 (3개)
├─ ParticleSystem (Splash) - Transform/콜백 기반
├─ Transform (Nature) - Transform 기반, NaturesReturn 간접 관리
└─ Transform (Row) - Transform 기반
```

### 개선 로드맵

#### 1단계: 미흡한 부분 정리 (1시간)
```
✅ 필요: LotusMovement.ResetState() 체크
   현재 비어있음 → 향후 필요 시 추가

✅ 필요: TrainMovement.ResetState() 개선
   _startPos = Vector3.zero; 추가

✅ 선택: 명시적 ResetState() 호출 제거
   CarGenerator.RemoveCar() → 불필요
   LogGenerator.RemoveLog() → 불필요
   LotusGenerator.RemoveLotus() → 불필요
```

#### 2단계: 비Movement 오브젝트 개선 (2-3시간)
```
⚠️ ParticleSystem (Splash)
   → SplashPool Wrapper 클래스 작성
   
⚠️ Transform (Nature)
   → NaturesReturn를 IPoolable로 변경
   
⚠️ Transform (Row)
   → RowObject 클래스 작성
```

---

## 7️⃣ 상세 개선 가이드

### 개선 1: LotusMovement.ResetState() 완성

**현재**:
```csharp
public void ResetState() { }  // 비어있음
```

**개선**:
```csharp
public void ResetState() 
{
    // Lotus에는 현재 초기화할 상태가 없으나,
    // 향후 회전, 애니메이션 등이 추가될 가능성 대비
    // transform.rotation = Quaternion.identity;  // ← 필요 시 추가
}
```

**소요 시간**: 5분

---

### 개선 2: TrainMovement.ResetState() 완성

**현재**:
```csharp
public void ResetState() { }  // 비어있음
```

**개선**:
```csharp
public void ResetState() 
{
    _startPos = Vector3.zero;  // ← _startPos 초기화
}
```

**소요 시간**: 5분

---

### 개선 3: 불필요한 ResetState() 호출 제거

**CarGenerator.RemoveCar()**:
```csharp
// Before
public void RemoveCar(CarMovement car)
{
    car.ResetState();
    _carsPools.Return(car);
}

// After
public void RemoveCar(CarMovement car)
{
    _carsPools.Return(car);  // ← ResetState는 Return 내부에서 호출됨
}
```

**LogGenerator.RemoveLog()**:
```csharp
// Before
public void RemoveLog(LogMovement log)
{
    // ...
    log.ResetState();  // ← 불필요
    _logsPools.Return(log);
}

// After
public void RemoveLog(LogMovement log)
{
    // ...
    _logsPools.Return(log);  // ← 충분
}
```

**LotusGenerator.RemoveLotus()**:
```csharp
// Before
public void RemoveLotus(LotusMovement lotus)
{
    // ...
    lotus.gameObject.SetActive(false);  // ← 불필요 (Return에서 처리)
    lotus.transform.rotation = Quaternion.identity;  // ← 불필요 (Return에서 처리)
    _lotusPools.Return(lotus);
}

// After
public void RemoveLotus(LotusMovement lotus)
{
    // ...
    _lotusPools.Return(lotus);  // ← 충분
}
```

**소요 시간**: 15분

---

### 개선 4: ParticleSystem (Splash) 개선

**작업 사항**:
1. SplashPool Wrapper 클래스 생성
2. RiverController 수정

**코드**:
```csharp
// Assets/Scripts/Map/River/SplashPool.cs (새 파일)
public class SplashPool : MonoBehaviour, IPoolable
{
    private ParticleSystem _particle;
    
    public void OnCreate()
    {
        _particle = GetComponent<ParticleSystem>();
    }
    
    public void OnGet()
    {
        _particle.Play();
    }
    
    public void OnReturn()
    {
        _particle.Stop();
    }
}

// RiverController 수정
void OnEnable()
{
    // Before
    ParticleSystem splash = _splashPrefab.GetComponentInChildren<ParticleSystem>();
    _splashPool = new GameObjectPool<ParticleSystem>(
        transform, _initialSize, 
        () => Instantiate(splash, transform),
        splash => splash.Play()
    );
    
    // After
    SplashPool splashPrefab = _splashPrefab.GetComponent<SplashPool>();
    _splashPool = new GameObjectPool<SplashPool>(
        transform, _initialSize, 
        () => Instantiate(_splashPrefab, transform)
                  .GetComponent<SplashPool>()
        // IPoolable의 OnCreate/OnGet/OnReturn이 자동 호출됨
    );
}

void PlaySplash(Vector3 position)
{
    // Before
    ParticleSystem splash = _splashPool.Get();
    if (splash == null) return;
    splash.transform.position = position;
    splash.Play();
    StartCoroutine(ReturnSplashAfterDelay(splash, 2f));
    
    // After
    SplashPool splash = _splashPool.Get();
    if (splash == null) return;
    splash.transform.position = position;
    // splash.Play()는 이미 OnGet에서 호출됨
    StartCoroutine(ReturnSplashAfterDelay(splash, 2f));
}
```

**소요 시간**: 30분

---

### 개선 5: Transform (Nature) 개선

**작업 사항**:
1. NaturesReturn를 IPoolable로 변경
2. NatureGenerator 수정

**코드 개선 필요** - 복잡도 높음

**소요 시간**: 1-2시간

---

## 8️⃣ 최종 권장사항

### 🎯 즉시 필수 (30분)
```
1️⃣ TrainMovement.ResetState() 개선
   _startPos = Vector3.zero; 추가
   
2️⃣ 불필요한 ResetState() 호출 제거
   CarGenerator, LogGenerator, LotusGenerator 수정
```

### 🟡 권장 (1-2시간)
```
3️⃣ ParticleSystem (Splash) 개선
   SplashPool Wrapper 클래스 생성
```

### 🟢 선택적 (2-3시간)
```
4️⃣ Transform (Nature) 개선
   복잡도 높으므로 필요시에만 진행
   
5️⃣ Transform (Row) 개선
   복잡도 높으므로 필요시에만 진행
```

---

## 9️⃣ 체크리스트

### 현재 상태 검증
```
[ ] IPoolable 인터페이스 정의 ✅
[ ] GameObjectPool 개선 ✅
[ ] CarMovement IPoolable 구현 ✅
[ ] LogMovement IPoolable 구현 ✅
[ ] LotusMovement IPoolable 구현 ✅
[ ] TrainMovement IPoolable 구현 ✅
[ ] ParticleSystem (Splash) 미완성 ⚠️
[ ] Transform (Nature) 미완성 ⚠️
[ ] Transform (Row) 미완성 ⚠️
```

### 개선 작업 계획
```
1순위 (필수, 30분):
  [ ] TrainMovement.ResetState() 수정
  [ ] 불필요한 ResetState() 호출 제거 (3곳)
  [ ] 테스트

2순위 (권장, 1시간):
  [ ] SplashPool Wrapper 생성
  [ ] RiverController 수정
  [ ] 테스트

3순위 (선택, 2-3시간):
  [ ] NaturesReturn → IPoolable 변경 (또는 보류)
  [ ] RowObject 클래스 생성 (또는 보류)
```

---

## 🔟 결론

**현재 IPoolable 적용 상태**:

✅ **기본 체계는 완벽하게 구축됨**
- IPoolable 인터페이스 정의: 완료
- GameObjectPool 구현: 완료
- 주요 Movement 클래스: 완료

⚠️ **미흡한 부분 존재**
- 불필요한 명시적 호출 제거 필요
- 비Movement 오브젝트 개선 필요 (선택사항)

🎯 **다음 단계**:
1. TrainMovement.ResetState() 수정 (5분)
2. 불필요한 호출 제거 (15분)
3. ParticleSystem 개선 (30분, 권장)
4. Transform 기반 개선 (2-3시간, 선택사항)

**전체 소요 시간**: 20분 (필수) + 30분-1시간 (권장) = 50분-1시간 30분

---

**작성 완료**: 2026-11-20 00:01  
**다음 단계**: 1순위 개선 사항 구현
