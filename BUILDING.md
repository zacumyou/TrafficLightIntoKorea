# Building Traffic Light Into Korea 2.3.40

## 한국어

공식 Modding Toolchain이 설정된 Cities: Skylines II 개발 환경이 필요합니다. 빌드 기준은 게임 1.6.2f1이며 공개 호환 표기는 1.6.*입니다. 게임/Unity DLL과 사용자 세이브·인증 정보는 저장소에 포함하지 않습니다.

1. 게임 옵션에서 공식 Modding Toolchain을 설치하고 해당 환경 변수를 사용할 수 있는 터미널을 엽니다.
2. 저장소 루트에서 빌드와 UI 검사를 실행합니다.

```powershell
dotnet build src/TrafficLightIntoKorea.csproj -c Release
node verify-ui.cjs
```

공식 ModPostProcessor 및 Burst 단계를 거쳐 `src/bin/Release/net48`에 결과가 생성됩니다. `DeployWIP`는 비활성화돼 있어 빌드만으로 게임에 설치되지 않습니다.

로컬 설치 시 게임을 종료하고 기존 파일을 백업한 뒤, 모드 DLL·생성된 세 플랫폼 네이티브 라이브러리·IndividualSignals.js·두 도구 SVG·Icons·lang·package/readme.txt를 `Mods/TrafficLightIntoKorea`에 복사합니다. 구독본과 로컬본의 중복 활성화를 피하세요.

회귀 테스트에는 .NET 9가 필요합니다. placement-tests는 공식 도구 체인의 `CSII_MANAGEDPATH`에서 Newtonsoft.Json을 참조합니다.

```powershell
$testProjects = @(
  'tests/RecoveryTests.csproj',
  'fixed-tests/FixedTests.csproj',
  'accessory-tests/AccessoryTests.csproj',
  'merge-tests/MergeTests.csproj',
  'signal-state-tests/SignalStateTests.csproj',
  'three-lamp-tests/ThreeLampTests.csproj',
  'placement-tests/PlacementTests.csproj',
  'performance-tests/PerformanceTests.csproj',
  'preview-tests/PreviewTests.csproj',
  'restore-tests/RestoreTests.csproj',
  'visibility-tests/VisibilityTests.csproj'
)
foreach ($project in $testProjects) {
  dotnet run --project $project -c Release
  if ($LASTEXITCODE -ne 0) { throw "Failed: $project" }
}
```

visibility-tests는 1.6.2f1 네이티브 숨김 쿼리 동작을 모형으로 재현하고 실제 보호 코드를 연결합니다. performance-tests와 preview-tests도 ECS 모형을 사용합니다. 이 검사는 게임 렌더러, 실제 FPS 또는 세이브 재로드의 인게임 검증을 대신하지 않습니다.

## English

Install the game's official [Modding Toolchain](https://cs2.paradoxwikis.com/Modding_Toolchain) and use its configured environment variables. Run the build, UI check and .NET 9 test commands above from the repository root. The SDK build includes ModPostProcessor and Burst and writes to `src/bin/Release/net48`; it does not install automatically.

For a local install, close the game, back up the existing mod, and copy the mod DLL, generated native libraries, UI JavaScript, tool SVGs, Icons, lang and package/readme.txt into the local mod directory. Game/Unity assemblies are not redistributed. Do not load local and subscribed copies together.

The visibility, performance and preview suites use model ECS/native-query contracts with production code. They verify regressions but do not measure live rendering, FPS or all mod interactions.
