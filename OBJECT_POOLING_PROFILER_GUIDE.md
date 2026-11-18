# Object Pooling 비교 실험 확인 방법

## 실험 내용

실제 `Car1` 프리팹을 이용해 차량을 200회 처리하는 두 방식을 비교한다.

- `pool`: 미리 생성한 차량을 풀에서 꺼내 활성화한 뒤 비활성화하고 반환한다.
- `instantiate_destroy`: 차량을 생성하고 비활성화한 뒤 `Destroy`를 호출한다.

각 방식을 6회 측정하며 실행 순서는 번갈아 바뀐다. 별도의 `baseline`은 아무 작업도 하지 않는 프레임의 할당량을 확인하는 대조값이다.

## 준비

1. 이 프로젝트를 Unity Editor에서 연다.
2. Unity에서 **Window → Analysis → Profiler**를 연다.
3. **CPU Usage** 모듈을 선택하고 Profiler의 녹화 버튼을 켠다.
4. 상세 화면을 **Timeline** 또는 **Hierarchy**로 설정한다.
5. 기존 Profiler 기록이 필요하면 먼저 저장한다.

아래 명령은 Windows PowerShell에서 실행한다. Unity CLI와 프로젝트의 Unity Pipeline 패키지가 연결되어 있어야 한다.

## 실험 실행

### 1. Play Mode 시작

```powershell
unity command editor.playmode start --project-path "D:\Unity_Project\Crossy Road"
```

Unity의 Play 버튼이 활성화되었는지 확인한다. 이 명령은 실험 중 Editor가 백그라운드에서도 실행되도록 한다.

### 2. 비교 실험 실행

```powershell
unity command script run --file "C:\Users\mipip\.copilot\session-state\8980bee3-eb10-450f-bc62-b20900b3ab03\files\pool_compare.cs" --entry PoolComparison.Run --project-path "D:\Unity_Project\Crossy Road" --format json
```

실험 코드는 프로젝트 밖의 임시 세션 저장소에 있다. 세션 파일이 정리되거나 다른 컴퓨터에서 실행하면 경로가 존재하지 않을 수 있다.

## 결과 읽기

PowerShell 출력의 `samples`에서 같은 `round`의 결과를 비교한다.

| 항목 | 의미 |
|---|---|
| `mode: pool` | 풀링 사용 |
| `mode: instantiate_destroy` | 생성·파괴 |
| `mode: baseline` | 아무 작업도 하지 않는 대조 측정 |
| `mode: known_200x1024_bytes` | GC 할당 카운터 확인용 배열 생성 |
| `cycles` | 측정당 반복 횟수. 현재 200회 |
| `synchronousMilliseconds` | 반복 호출의 동기 실행 시간. 밀리초 |
| `wholeEditorFrameGcBytes` | 해당 Editor 프레임 전체의 관리 메모리 할당량. 바이트 |

Profiler 그래프에서 실험 중 프레임을 선택하면 CPU 시간과 **GC Alloc**을 확인할 수 있다. 그래프에는 두 실험 방식의 이름이 자동 표시되지 않으므로, 방식별 수치는 PowerShell 출력에서 확인한다.

## 결과 해석 시 주의

- 프레임 GC 수치에는 실험 외 Editor·게임·측정 코드의 할당이 포함된다. 따라서 차량 코드만의 정확한 할당량은 아니다.
- `Destroy`의 실제 파괴는 지연될 수 있으므로 동기 시간은 전체 파괴 비용이 아니다.
- 200회 작업을 한 프레임에 몰아서 처리하므로 실제 플레이의 생성 빈도나 차량 이동을 그대로 재현하지 않는다.
- GC Alloc은 **할당량**이며, GC가 실제 실행된 시간이나 끊김 자체를 증명하지 않는다.
- 실제 게임의 풀링을 끄는 실험이 아니다. 실제 플레이 전체를 비교하려면 같은 조건의 Windows 개발 빌드를 별도로 측정해야 한다.

## 종료

1. Profiler 녹화를 중지한다.
2. Unity의 **Play 버튼을 다시 눌러** Edit Mode로 돌아간다.

실험은 임시 객체를 사용하며 프로젝트의 스크립트와 프리팹을 변경하지 않는다.
