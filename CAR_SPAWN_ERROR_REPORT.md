# 차량 생성 오류 분석 보고서

## 수정 결과

후속 요청에 따라 `CarGenerator._cars`를 기존 `List<GameObject>`로 복원했다. 풀 초기화 전에 빈 목록, 누락된 참조와 `CarMovement`가 없는 프리팹을 검사하고 오류를 기록하도록 했으며, 차량 인스턴스에서 컴포넌트를 가져와 기존 차량 풀에 넣는다. 행 및 차량 풀링 동작은 유지한다.

Unity 재컴파일 후 실제 도로 프리팹에서 차량 참조 12개가 모두 유효한 것을 확인했다. 플레이 모드에서 최초 도로 생성, 도로 반환과 같은 인스턴스 재사용, 차량 풀 유지와 생성 재개, 반환된 차량의 동일 인스턴스 재사용을 확인했다. 빈 목록·null 항목·컴포넌트 누락도 각각 오류와 초기화 중단을 확인했다. 검증용 예상 오류는 임시 로그 핸들러로 확인했다. 장시간 실제 플레이는 검증하지 않았다.

수정 전 파일은 세션 저장소의 `car-prefab-fix-before`에 보관했다. 아래 내용은 **수정 전 진단**이다.

## 결론

**심각도: High.** 도로 행의 차량 목록이 직렬화된 방식과 현재 코드의 필드 타입이 일치하지 않는다. 도로 프리팹에는 차량 **GameObject** 참조가 저장되어 있지만, `CarGenerator`는 같은 `_cars` 필드를 **CarMovement 컴포넌트** 목록으로 읽는다. 현재 Unity Editor에서 해당 목록 12개가 모두 null로 읽히는 것을 확인했다. 따라서 차량 풀을 초기화할 때 null을 `Instantiate`에 전달해 예외가 발생한다.

이 불일치는 이전 행 풀링 변경에서 `_cars` 타입을 `List<GameObject>`에서 `List<CarMovement>`로 변경하면서 기존 프리팹 참조를 함께 이관하지 않아 발생한 회귀다. 이전 풀링 검증에서 이 직렬화 호환성을 놓쳤다.

## 확인한 근거

1. [CarGenerator.cs](Assets/Scripts/Map/Cars/CarGenerator.cs)의 9행은 `_cars`를 `List<CarMovement>`로 선언하고, 92~94행에서 임의로 선택한 요소를 `Instantiate(prefab, _root)`에 전달한다.
2. [Asphalt.prefab](Assets/Prefab/Road/Asphalt.prefab)의 233~245행에는 차량 12개가 저장되어 있다. 예를 들어 첫 참조는 [Car1.prefab](Assets/Prefab/Car/Car1.prefab)의 GameObject fileID `2432322878901096503`이다. 같은 차량의 `CarMovement` 컴포넌트 fileID는 `3279462057076089788`로 별개다.
3. 현재 Editor의 직렬화 속성 조회에서 12개는 GameObject 참조로 확인된다. 반면 같은 프리팹의 실제 `CarGenerator._cars` 필드를 읽으면 **12개 중 12개가 null**이다. 차량 에셋 12개는 모두 존재하고 각 루트에 `CarMovement`가 있는 것도 확인했다.
4. 현재 작업 트리와 기존 Git 버전을 비교했을 때 `_cars`가 `List<GameObject>`에서 `List<CarMovement>`로 변경되었다. 도로 프리팹에는 해당 이관 변경이 없다.
5. Unity Console에서 사용자가 제시한 것과 같은 `CarGenerator.OnEnable` → 풀 생성 → `Instantiate` 예외가 여러 번 기록된 것을 확인했다. 현재 Editor는 Play Mode가 중지된 상태이고 컴파일 오류는 없다. 콘솔 기록은 지우지 않았다.

## 발생 경로와 영향

1. [MapManager.CheckPos](Assets/Scripts/Manager/MapManager.cs)에서 새 행을 요청한다.
2. [MapGenerator.GetRow](Assets/Scripts/Map/MapGenerator.cs)가 도로 행을 활성화하면 `CarGenerator.OnEnable`이 호출된다. 이는 풀에서 재사용할 때뿐 아니라 처음 생성한 행을 활성화할 때도 해당한다.
3. [GameObjectPool](Assets/Scripts/Util/GameObjectPool.cs)의 생성자가 초기 차량을 만들기 위해 생성 함수를 호출하고, 목록에서 선택된 null 프리팹으로 인해 예외가 발생한다.
4. 차량 풀 초기화가 완료되지 않아 해당 도로 행의 차량 생성이 정상 시작되지 않는다. 다른 도로 행이 생성될 때도 동일 예외가 반복될 수 있다. 현재 맵 설정 검사는 행 프리팹 자체를 확인하지만 그 내부 차량 목록의 타입 호환성은 확인하지 않는다.

이 오류는 앞서 조사한 Pipeline HTTP 응답 오류와 별개의 게임 코드·에셋 직렬화 문제다.

## 권장 수정 방향

**최소 변경안:** `_cars`를 기존의 `List<GameObject>`로 되돌려 이미 저장된 프리팹 참조를 유지하고, 생성한 차량에서 `CarMovement`를 가져와 초기화한다. 목록이 비어 있거나 참조가 없고 필수 컴포넌트가 없는 경우에는 행과 항목을 식별할 수 있도록 명시적으로 오류를 기록하고 초기화를 중단한다. 차량 생성기의 OnEnable/OnDisable 및 행 풀링 생명주기는 유지한다.

**대안:** `List<CarMovement>`를 유지하려면 도로 프리팹의 12개 참조를 각 차량의 컴포넌트로 이관하고 누락 검사를 추가한다. 다른 프리팹·씬의 같은 필드도 함께 확인해야 한다. 두 방식을 혼합하거나 코드를 바꾸지 않은 채 기존 GameObject 참조가 자동 변환된다고 가정해서는 안 된다.

## 수정 후 권장 검증

- Editor를 재컴파일한 뒤 프리팹을 다시 읽어 차량 참조 12개가 null이 아님을 확인한다.
- 실제 도로 행을 처음 생성하고, 반환한 뒤 재활성화하여 차량 생성과 풀 재사용을 검증한다.
- 차량 목록 누락, 빈 목록 및 컴포넌트 누락을 별도로 검증하고 명확한 오류가 남는지 확인한다.
- 여러 도로 행 생성과 전진 플레이에서 동일 예외가 재발하지 않는지 확인한다. Play Mode 종료 후 콘솔과 씬 변경 여부를 확인한다.

## 범위와 한계

최초 조사는 코드·프리팹·프로젝트 설정을 변경하지 않고 진행했다. 이후 수정과 검증 범위는 위의 수정 결과에 기재했다.

Unity 6000.4.0f1에서 Editor 상태, 콘솔 로그, 프리팹 직렬화 속성 및 필드 값을 조회했다. 확인한 Pipeline 문서: `commands/editor-lifecycle-and-observability.md`, `commands/runtime.md`, `commands/scripts.md`.
