# Unity 프로젝트 종합 진단 및 수정 보고서

## 1. 프로젝트 개요

### 조사 범위와 기술

- 기준: 2026-11-17, 현재 작업 트리와 Unity Editor **6000.4.0f1**.
- 자체 게임 스크립트와 Editor 스크립트, Game·Load 빌드 씬, 주요 프리팹과 행 데이터, 입력·물리·렌더링 설정 및 패키지를 조사했다. 외부 에셋과 TMP 예제는 자체 게임 코드와 구분했다.
- [Game](Assets/Scenes/Game.unity), [Load](Assets/Scenes/Load.unity)는 [빌드 설정](ProjectSettings/EditorBuildSettings.asset)에 등록되어 있다.
- [패키지 설정](Packages/manifest.json) 기준 URP 17.4.0, Input System 1.19.0, uGUI 2.0.0, Cinemachine 3.1.7, Test Framework 1.6.0 등을 사용한다. 패키지 설치 자체가 해당 기능의 구현을 뜻하지는 않는다.
- 코드는 `Manager`, `Map`(Cars/Nature/River/Train), `Player`, `UI`, `Util` 등으로 분리되어 있다. 행 구성은 [RowData](Assets/Scripts/Map/RowData.cs), 점수 저장은 PlayerPrefs를 사용한다.
- 작업 전부터 [MapGenerator](Assets/Scripts/Map/MapGenerator.cs), 패키지 설정과 `.vscode/`에 사용자 변경이 있었다. 그 변경은 유지했다.

### 구현 상태

| 구분 | 확인 결과 |
|---|---|
| 플레이어 | 마우스 클릭·스와이프 이동, 장애물 판정, 강 발판 탑승과 충돌 처리. 터치·키보드·패드는 지원 범위에 추가하지 않음 |
| 맵 | 가중 랜덤 행 생성, 도로 차량, 강의 통나무·연잎, 철도 차단기·기차, 자연물 |
| 진행·UI | 메인·플레이·게임 오버 상태, 점수·최고 점수, 재시도, 로딩·페이드, 결과 UI |
| 데이터 | 행 유형별 ScriptableObject, 프리팹·Inspector 설정, 최고 점수 저장 |
| 전투·적 AI·체력·웨이브·업그레이드 | 자체 코드에서 확인하지 못함. 기획 범위가 불명확하므로 버그가 아닌 미구현/범위 미확정 |
| 테스트 | 상설 테스트 어셈블리는 없다. 이번 검증은 세션 저장소의 임시 플레이 모드 코드로 수행 |

## 2. 전체 진단 요약

- **완성도:** 핵심 루프가 구성된 3D 프로토타입. 제한된 플레이 모드 검증만으로 출시 준비도를 판단할 수 없다.
- **장점:** 데이터 기반 행 구성, 전방 생성·후방 제거, 생성기별 풀, 게임 상태와 UI의 기본 역할 분리가 있다.
- **남은 위험:** 정적 싱글턴과 씬·Inspector 참조에 대한 의존, 상설 회귀 테스트 부재, 실제 장시간 플레이와 성능 미측정.
- **원래 진단 19건:** Critical 0, High 5, Medium 11, Low 3. 이 중 High 5, Medium 11, Low 2는 수정했다. Low 1건(L-03)은 일부 정리했고 이름 변경은 호환성 확인 전 보류했다. 이는 새 결함이 없다는 보증이 아니다.
- 입력은 사용자 결정에 따라 마우스 전용으로 유지했다. 강 행에는 통나무 1개를 즉시 생성하며, 타이틀 복귀는 Game 씬을 재로드해 메인 상태로 돌아간다. 머티리얼 자동 변환은 기본적으로 꺼져 있다.
- 후속 점검에서 [MapManager](Assets/Scripts/Manager/MapManager.cs)의 행 제거·반환과 생성 실패 방어가 이전 수정 상태와 다른 것을 확인했다. 해당 수정과 미사용 필드 정리를 재적용하고 아래와 같이 다시 검증했다.
- 이후 요청에 따라 생성 행을 프리팹별 풀에 반환하고 재사용하도록 변경했다. 차량·통나무·연잎 풀과 철도 코루틴도 행 재활성화에 맞춰 초기화한다.
- 점수는 이번 게임의 최고 도달 위치를 경신하는 착지에서만 1점씩 증가하도록 변경했다. 게임 시작과 초기화 시 점수·최고 위치 및 점수 UI를 함께 초기화한다.

### 검증 결과와 한계

- **행 풀링 변경 후:** Unity 재컴파일 성공. 실제 잔디·도로·강·철도 프리팹의 반환과 같은 인스턴스 재사용, 자연물 반환, 차량·통나무의 시간 경과 후 재생성, 연잎 20회 재사용, 철도 차단기 초기화 및 분리된 기차 정리를 플레이 모드에서 확인했다.
- 실제 맵 관리자를 통해 100회 전진을 모의해 기존 행의 재사용과 활성 행 큐를 확인했다. 탑승 중 행 반환 시 분리·게임 오버와 중복·누락·다른 생성기의 행 반환 거부도 확인했다. 마지막 검증에서 예상 오류 3건은 임시 로그 핸들러로 확인했다.
- **최고 거리 점수 변경 후:** Unity 재컴파일 성공. 실제 플레이어 점프 코루틴으로 전진·후진·기존 최고점 복귀·신기록·좌우 이동, 첫 이동이 후진인 경우, 게임 재시작·초기화 및 게임 오버 중 점프 중단을 확인했다. 점수 UI도 함께 확인했다. 테스트는 임시 코드로 수행했으며 기존 최고 점수 저장값을 변경하지 않았다.
- 이전 재컴파일 과정의 Pipeline `/api/commands` 요청에서 이미 해제된 `HttpListenerResponse`에 응답을 쓰면서 오류 2건이 발생했다. 정확한 외부 요청 원인은 확인되지 않았다. 이번에는 재컴파일 완료와 도메인 리로드 종료를 확인한 후 다음 요청을 순차 실행했고 컴파일·플레이 모드 검증에 성공했다. 이 방법은 운영상 회피이며 Pipeline 패키지의 근본 오류 수정이나 재발 방지를 보장하지 않는다. 패키지는 변경하지 않았다. 컴파일 이후 Editor 콘솔 오류는 0건이며 이전 콘솔 기록은 컴파일 과정에서 사라졌다.
- **맵 관리 재적용 후:** Unity 6000.4.0f1 재컴파일 성공(`failed: false`, 오류 0). 컴파일된 `AddRows`의 반환 타입도 확인해 수정 코드가 Editor에 반영되었음을 확인했다.
- 재적용 후 플레이 모드에서 유효한 행 생성·큐 등록, 오래된 행 제거와 가까운 행 유지, 자연물 30회 반환, 탑승 중 행 제거 시 플레이어 분리·게임 오버를 확인했다. 빈 목록·null 행·가중치 합 0·잘못된 반복 설정에서는 오류를 기록하고 관리자를 중지하는지 확인했다.
- 잘못된 행 설정 테스트의 **예상 오류 8건**은 임시 로그 핸들러로 확인했고 실제 콘솔에 전달하지 않았다. 테스트 후 콘솔 오류 0건, 플레이 모드 종료. 검증용 코드는 삭제했다.
- 최초 수정 당시 Unity 자동 컴파일도 성공했다. 당시 수동 재컴파일 요청은 자동 갱신 중 HTTP 400을 반환했지만 이후 재컴파일 완료와 오류 0을 확인했다.
- 플레이 모드 집중 검증: 자연물 30회 생성·반환, 통나무·행 제거 시 탑승자 분리, 게임 오버 중 점프 중단, 마우스 부재, 철도 행 제거를 이전 수정에서 확인했다.
- 이전 Medium·Low 수정 당시 실제 연잎 프리팹의 자식 콜라이더 탑승과 풀 반환 시 분리, 연잎 행 파괴, 자연물·연잎의 범위 오류, 초기 통나무 1개, 차량 최소 간격, 전진 회전, River2 데이터, 잘못된 씬 이름, 타이틀 복귀 및 중복 요청을 확인했다. 이번 재적용에서는 맵 관리 경로를 집중 재검증했으며 다른 항목을 모두 다시 실행하지 않았다.
- 이전 검증에서 자동 변환의 기본 비활성화와 기존 명시적 활성화 설정을 확인하고 EditorPrefs를 원상 복구했다. 중복 전환 경고 1건은 의도된 검증 결과였다.
- 씬 로드 **진행 중** 실제 실패와 이전 씬 복구, 누락된 로딩 타이틀의 실제 연출, 버튼의 수동 클릭, 머티리얼 변환 전후 시각 품질은 검증하지 않았다. 빌드·실제 기기·장시간 플레이·Profiler 측정도 수행하지 않았다.
- [복구 사본](C:/Users/mipip/.copilot/session-state/8980bee3-eb10-450f-bc62-b20900b3ab03/files/remaining-fix-backup)에 이번 수정 전 대상 파일과 보고서를 보관했다. 이전 High 수정 전 사본은 같은 세션의 `high-fix-backup`에 있다.
- 맵 재적용 직전의 파일과 보고서는 같은 세션의 `map-before-reapply.cs`, `report-before-reapply.md`에 별도로 보관했다.
- 행 풀링 변경 전 복구 사본은 같은 세션의 `row-pooling-backup`에 보관했다.
- 점수 변경 전 복구 사본은 같은 세션의 `score-before`에 보관했다.
- 참조한 설치 패키지 Pipeline 문서: `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md`, `commands/runtime.md`.

## 3. 발견된 문제 목록

아래는 **최초 진단 기준**의 문제와 현재 조치다. 우선순위는 당시 기준이며, 실행 검증 범위는 위에 명시했다.

### Critical

확정된 항목 없음.

### High — 5건 수정

| ID·문제 | 관련 위치 | 원인·영향 및 수정 방향/현재 상태 | 주의사항 |
|---|---|---|---|
| H-01 자연물 누적 | [MapGenerator.ReleaseRow](Assets/Scripts/Map/MapGenerator.cs), [MapManager](Assets/Scripts/Manager/MapManager.cs), [NatureGenerator](Assets/Scripts/Map/Nature/NatureGenerator.cs) | 행과 별도 루트의 자연물이 제거 시 반환되지 않았다. 자연물을 행 소유로 두고 행 반환 전 풀에 반환하도록 수정. 30회 반복 확인. | 초기 필드의 별도 자연물 소유권을 유지. 장시간 메모리는 미측정. |
| H-02 탑승 중 플랫폼 제거 | [PlayerController.RemoveFromPlatform](Assets/Scripts/Player/PlayerController.cs), [LogGenerator.RemoveLog](Assets/Scripts/Map/River/LogGenerator.cs), [MapManager](Assets/Scripts/Manager/MapManager.cs) | 플레이어가 발판 자식으로 함께 비활성화·파괴될 수 있었다. 통나무 반환이나 행 제거 전 분리하고 게임 오버하도록 수정. | 월드 위치와 원래 부모 유지. 연잎 반환도 동일 정책 적용. |
| H-03 게임 오버 후 점프 지속 | [PlayerController.CoCharJump](Assets/Scripts/Player/PlayerController.cs) | 진행 중 코루틴이 이동·착지·점수 처리를 계속했다. 게임 오버 확인 후 점프·회전을 중단하고 이동 상태를 해제. 정상 점수와 중단 확인. | 물 침수 등 별도 연출 코루틴은 중단하지 않음. |
| H-04 마우스 부재 예외 | [PlayerController](Assets/Scripts/Player/PlayerController.cs), [UIManager](Assets/Scripts/Manager/UIManager.cs) | `Mouse.current`를 직접 역참조했다. 장치가 없으면 경고 후 입력을 중단하고 재연결 시 복구하도록 수정. | 마우스 전용 정책 유지. 실제 장치 연결·해제 UX는 별도 확인. |
| H-05 철도 행 제거 후 기차 잔존 | [TrainGenerator](Assets/Scripts/Map/Train/TrainGenerator.cs) | 기차가 행과 별도 루트에 있어 행 제거 시 남을 수 있었다. 행 비활성화 또는 생성기 파괴 시 기차 정리. 프리팹으로 확인. | 기차가 이미 제거된 경우도 기존 null 검사 유지. 기차 자체는 풀링하지 않는다. |

### Medium — 11건 수정

| ID·문제 | 관련 위치 | 원인·영향 및 수정 방향/현재 상태 | 주의사항 |
|---|---|---|---|
| M-01 연잎 자식 콜라이더 탑승 누락 | [PlayerController.CheckRide](Assets/Scripts/Player/PlayerController.cs), [Lotus 프리팹](Assets/Prefab/Nature/Lotus.prefab) | 같은 오브젝트만 검색해 부모의 `RidePlatform`을 놓쳤다. 부모 계층 검색과 누락 시 오류·게임 오버로 수정; 실제 프리팹으로 탑승 확인. | 강 우선 판정과 맞은 콜라이더의 높이 사용을 유지. |
| M-02 파괴 중 연잎 풀 반환 | [LotusMovement](Assets/Scripts/Map/River/LotusMovement.cs), [LotusGenerator.RemoveLotus](Assets/Scripts/Map/River/LotusGenerator.cs) | `OnDestroy`에서 파괴 대상이 풀에 반환됐다. 해당 콜백을 제거하고 명시적 반환 시 탑승자를 분리. 행 반환과 재사용 검증. | 연잎 풀은 행 인스턴스와 함께 재사용하며 파괴된 객체는 반환하지 않음. |
| M-03 행 데이터 오류 시 무한 루프 | [MapGenerator](Assets/Scripts/Map/MapGenerator.cs), [MapManager.CheckPos](Assets/Scripts/Manager/MapManager.cs) | 빈 목록, 가중치 합 0, 잘못된 반복이나 null 행에서 생성이 진행되지 않을 수 있었다. 설정 검증과 진행 실패 시 오류·관리자 중지 추가. 정상 행과 오류 사례 확인. | 설정 오류는 명시적으로 로그에 남기며 정상 행의 가중 선택과 간격은 유지. |
| M-04 초기 통나무가 항상 0개 | [LogGenerator.CoSpawnLog](Assets/Scripts/Map/River/LogGenerator.cs) | 정수 `Random.Range(0, 1)`의 배타적 상한으로 0만 선택됐다. 사용자 선택에 따라 1개 즉시 생성하도록 수정하고 확인. | 강의 난이도와 연속 강 행은 수동 플레이로 추가 확인. |
| M-05 없는 Title 씬 참조 | [UIManager.LoadTitle](Assets/Scripts/Manager/UIManager.cs) | 등록되지 않은 `Title` 씬을 요청했다. 사용자 선택에 따라 기존 재시도 경로로 Game을 재로드하고 메인 상태로 복귀. | 현재 별도 Title 씬은 만들지 않음. |
| M-06 씬 전환 중복·실패 처리 부족 | [ScenesManager](Assets/Scripts/Manager/SceneManager.cs), [UIManager](Assets/Scripts/Manager/UIManager.cs) | 공용 로더의 중복 보호 및 참조·로드 결과 검증이 부족했다. 로딩 잠금, 씬·페이드 검사, 오류 기록, 이전 씬 복구 시도와 잠금 해제를 추가. 정상 타이틀 복귀·중복 요청·잘못된 이름 확인. | 로드 진행 중 실제 실패와 복구는 미검증; 복구에도 실패하면 Load 씬에 남을 수 있다. 필수 참조와 애니메이션 순서를 유지. |
| M-07 광범위한 자동 머티리얼 변환 | [UrpMaterialConverter](Assets/Editor/LowPolyMaterialConverter.cs) | 자동 변환 기본값이 켜져 있어 import 시 기존 머티리얼이 변할 수 있었다. 기본값을 끄고 새로 활성화할 때 경고·확인을 추가. | 이미 명시적으로 켠 EditorPrefs 값은 보존; 특수 셰이더·투명도 시각적 동등성은 미검증. |
| M-08 랜덤 배치 수 범위 오류 | [NatureGenerator.SpawnNatureIn](Assets/Scripts/Map/Nature/NatureGenerator.cs), [LotusGenerator](Assets/Scripts/Map/River/LotusGenerator.cs) | 9개 위치보다 큰 개수나 역전 범위에서 인덱스 오류가 가능했다. 생성 전 범위 검사와 오류 기록을 추가; 초과값 확인. | 현재 위치 수 9와 검사 기준을 함께 유지. |
| M-09 Unity 6 검색 API 경고 | [ScenesManager.CoLoadScene](Assets/Scripts/Manager/SceneManager.cs) | `FindFirstObjectByType`가 이 Editor 버전에서 경고를 발생시켰다. `FindAnyObjectByType`과 null 검사로 변경; 컴파일 확인. | 타이틀이 여럿이면 선택 순서가 보장되지 않는다. |
| M-10 전진 스와이프 회전 누락 | [PlayerController.SwipeDir](Assets/Scripts/Player/PlayerController.cs) | 클릭 전진과 달리 긴 위쪽 스와이프에서 회전이 없었다. 기존 전진 회전을 추가하고 방향 검증. | 장애물 검사와 회전 시간 유지. |
| M-11 차량 최소 간격 미적용 | [CarGenerator.CoSpawnCars](Assets/Scripts/Map/Cars/CarGenerator.cs) | Inspector의 `_min` 대신 2초를 사용했다. `_min` 적용과 범위 검사를 추가; 지정한 최소값 확인. | 기존 프리팹 값에 따라 차량 빈도가 바뀔 수 있으며 초기 생성 간격은 별도다. |

### Low — 2건 수정, 1건 부분 정리

| ID·문제 | 관련 위치 | 원인·영향 및 수정 방향/현재 상태 | 주의사항 |
|---|---|---|---|
| L-01 미사용 필드 | [MapManager](Assets/Scripts/Manager/MapManager.cs), [PlayerController](Assets/Scripts/Player/PlayerController.cs), [LotusMovement](Assets/Scripts/Map/River/LotusMovement.cs), [TrainMovement](Assets/Scripts/Map/Train/TrainMovement.cs) | 읽지 않는 필드가 Inspector 값과 경고에 혼란을 줬다. 사용되지 않는 필드를 제거했으며 차량 `_min`은 M-11에서 실제 사용한다. | 제거된 직렬화 값은 동작에 사용되지 않았음. 이후 관련 기능을 추가할 때 설정을 새로 설계한다. |
| L-02 River2 데이터 유형 불일치 | [River2Data](Assets/Scripts/ScriptaleObject/River2Data.asset), [RowData](Assets/Scripts/Map/RowData.cs) | River2 에셋이 River1 enum 값을 사용했다. River2 값으로 바로잡고 에셋 로드를 확인. | 가중치·프리팹은 유지. 현재 생성기는 Grass와 그 외만 구분한다. |
| L-03 명명·미사용 구조 | [LodingManager.cs](Assets/Scripts/Manager/LodingManager.cs), [RidePlatform](Assets/Scripts/Map/Marker_Component/RidePlatform.cs) | 철자·접근 명칭과 빈 콜백·오래된 주석이 탐색을 어렵게 했다. 빈 콜백과 기차의 오래된 주석은 정리. | `LodingManager_Del`, `_GM`/`_UM` 이름은 직렬화·참조 호환성 확인 전 유지; 이름 정리는 **미완료**. |

### 문제와 별개인 개선 제안

- 메인 화면 입력 및 게임 오버 후 월드 이동의 허용 여부는 기획 결정 사항이다.
- 빠른 기차의 충돌 누락이나 풀·Raycast 병목은 재현·측정하지 못했으므로 확정 결함으로 취급하지 않는다.
- 싱글턴과 풀을 전면 재설계하기보다 씬·객체 수명 계약과 테스트를 먼저 보강한다.

## 4. 시스템별 상세 분석

### 상태·씬·UI

[GameManager](Assets/Scripts/Manager/GameManager.cs)는 점수·이번 게임의 최고 Z 위치와 상태를 보유하고 [UIManager](Assets/Scripts/Manager/UIManager.cs)에 표시를 위임한다. 첫 완료된 점프의 시작 위치를 기준으로 최고 위치를 정하므로 첫 이동이 후진이어도 출발 위치까지 돌아오는 동안은 점수가 증가하지 않는다. [ScenesManager](Assets/Scripts/Manager/SceneManager.cs)와 [FadeManager](Assets/Scripts/Manager/FadeManager.cs)는 씬 전환을 담당한다. Game → Load → Game 전환 후 메인 상태로 초기화한다. 정적 참조와 Inspector 설정에 의존하므로 새로운 씬을 추가할 때 필수 매니저·UI의 생성 순서와 로드 실패 복구를 함께 점검해야 한다. 별도의 [LodingManager_Del](Assets/Scripts/Manager/LodingManager.cs)은 현재 빌드 씬의 연결을 확인하지 못했으며 삭제하지 않았다.

### 플레이어·입력·물리

[PlayerController](Assets/Scripts/Player/PlayerController.cs)가 입력·이동·회전·장애물 검사·탑승을 담당한다. 플레이어는 kinematic Rigidbody를 사용하고 위험 요소와 트리거 판정에 의존한다. 플랫폼은 `LayerRide` 콜라이더의 부모에서 찾으며, 탑승 중 플랫폼이 제거되면 분리 후 게임 오버한다. 실제 기기와 빠른 물체의 충돌 타이밍은 별도 검증이 필요하다.

### 맵·자연물

[MapGenerator](Assets/Scripts/Map/MapGenerator.cs)가 행 데이터를 가중 선택하고 프리팹별 풀에서 행을 얻으며 [MapManager](Assets/Scripts/Manager/MapManager.cs)가 전방 버퍼와 오래된 행 큐를 관리한다. 행 반환 시 탑승자를 분리하고 자연물을 반환한 뒤 행을 비활성화한다. 재사용할 때 위치를 먼저 바꾸고 다시 활성화한다. 중복·소유권이 다른 행의 반환은 오류로 거부한다. 잘못된 데이터는 로그를 남기고 관리자를 중지하므로 Inspector 수정 후 다시 시작해야 한다. 초기 필드의 [BaseNatureGenerator](Assets/Scripts/Map/Nature/BaseNatureGenerator.cs)는 별도 소유권을 유지한다.

### 차량·강·철도

차량·통나무·연잎 풀은 행 인스턴스와 함께 재사용하며, 반환할 때 활성 자식을 풀로 되돌리고 재활성화할 때 생성 동작을 다시 시작한다. 통나무는 강 생성 시 1개를 즉시 배치하고 이후 설정 간격으로 생성한다. 차단기 애니메이션 이벤트는 [CrossingGateMovement](Assets/Scripts/Map/Train/CrossingGateMovement.cs)와 [애니메이션](Assets/Animation/CrossingGate.anim)의 계약이다. 철도 행을 반환하면 차단기 코루틴을 중지하고 별도 루트 기차를 정리한다.

### 유틸리티·Editor

[GameObjectPool](Assets/Scripts/Util/GameObjectPool.cs)은 큐 기반이며 필요할 때 확장한다. 전역 풀이나 용량 제한은 측정 없이 도입하지 않았다. [머티리얼 변환기](Assets/Editor/LowPolyMaterialConverter.cs)는 Editor 전용이며 자동 변환 기본값을 비활성화했다. 명시적 변환 기능은 유지한다.

## 5. 성능 및 최적화 분석

| 항목 | 현재 구조와 영향 가능성 | 권장 사항 |
|---|---|---|
| 자연물 | 행별 반환 경로 수정, 단기 반복 검증 완료 | 장시간 객체 수·메모리 추적 |
| 행 생성 | 프리팹별 풀을 필요에 따라 확장하고 반환된 행을 재사용. 생성 결과 리스트는 여전히 할당됨 | GC Alloc과 CPU 피크를 측정하고 풀 확장 횟수를 확인 |
| 행별 풀 | 도로·강 행의 자식 풀은 행과 함께 유지되고 필요 시 확장 | 최대 동시 사용량만큼 비활성 행·자식이 남으므로 메모리와 생성 피크를 측정 |
| 물리 | 차량별 Raycast와 다수 콜라이더 | Physics Profiler와 타깃 기기에서 실제 비용·충돌 확인 |
| 코루틴 | 반복 생성과 대기에서 `WaitForSeconds` 등을 사용 | 할당 빈도와 영향 측정 후 변경 |
| 파티클 | 물 튀김 프리팹은 비반복·자동 종료 설정 | 반복 게임 오버 시 실제 정리 확인 |

5분·15분 이동 및 반복 재시도 전후에 활성 객체 수, GC Alloc/frame, CPU·Physics 시간과 메모리 스냅샷을 비교하는 것을 권장한다. 이 보고서에 실측 수치는 없다.

## 6. 개선 작업 목록

- **P0:** 확인된 Critical 없음. 실제 진행 중단이 재현되면 우선 대응.
- **P1:**
  - [x] H-01~H-05 및 M-01~M-04 수정과 집중 검증.
  - [ ] 실제 장치 입력과 장시간 플레이 회귀 검증.
- **P2:**
  - [x] M-05~M-11 수정 및 관련 집중 검증.
  - [x] 맵 행을 프리팹별 풀로 반환·재사용하고 하위 생성기 생명주기 확인.
  - [x] 최고 도달 거리 기준 점수와 게임 재시작 시 초기화 검증.
  - [ ] Pipeline 서버의 응답 객체 해제 오류에 대한 패키지 차원의 근본 수정 확인.
  - [ ] 씬 로드 진행 중 실패·이전 씬 복구와 UI 수동 클릭 검증.
  - [ ] 머티리얼 변환 시각 품질과 강·차량 난이도 확인.
  - [ ] 타깃 기기 프로파일링 후 필요한 최적화 결정.
- **P3:**
  - [x] L-01/L-02 및 L-03의 빈 콜백·오래된 주석 정리.
  - [ ] L-03 이름 변경 여부와 직렬화·UnityEvent 참조 영향 확인.
  - [ ] 핵심 회귀 시나리오를 상설 자동 테스트로 도입.
  - [ ] 게임 오버 후 월드 동작과 지원 플랫폼 명세.

## 7. 권장 개발 순서

1. 실제 기기에서 조작·연잎·통나무·철도·타이틀 복귀와 조정된 난이도를 수동 확인한다.
2. 씬 로드 중 실패·복구, 프리팹 참조 오류, 반복 재시도와 풀 수명을 상설 테스트로 보강한다.
3. 장시간 플레이와 Profiler 측정 결과에 따라 행·풀·물리 최적화를 선택한다.
4. 참조 호환성을 확인한 뒤 L-03 명명 정리를 별도로 진행하고, 기획상 필요한 기능만 추가한다.

## 8. 종합 의견

기존 구조를 유지하면서 진단된 High와 Medium 항목을 수정했고 Low의 명명 정리를 제외한 항목을 처리했다. 집중 검증은 통과했지만, 실제 기기·장시간 안정성·로드 중 실패 복구·머티리얼 시각 품질은 별도 확인이 필요하다. 전투·웨이브 등 기획 범위가 정해지지 않은 미구현 기능은 결함으로 분류하지 않는다.
