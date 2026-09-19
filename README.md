# Traffic Light Into Korea — 2.3.40

Cities: Skylines II의 교차로 신호등을 한국식 외형으로 바꾸는 모드입니다. 이 저장소는 Paradox Mods에 공개한 **2.3.40**의 소스, UI 리소스 및 회귀 테스트를 제공합니다.

- [Paradox Mods](https://mods.paradoxplaza.com/mods/159262/Windows)
- [필수 신호등·표지판 에셋 팩](https://mods.paradoxplaza.com/mods/159257/Windows)
- [상세 사용법](package/readme.txt) · [변경 기록](CHANGELOG.md) · [빌드 방법](BUILDING.md)
- [오류 제보](https://github.com/zacumyou/TrafficLightIntoKorea/issues) · [공식 포럼](https://forum.paradoxplaza.com/forum/threads/traffic-light-into-korea-tlik-discussion-bug-reports-and-suggestions.1941598/)

## 주요 기능

- 교차로별 또는 도시 전체 한국식 적용과 기본 신호등 복원
- 도로 구조에 따른 에셋 자동 선택, 개별 에셋·표지판·부속 프롭 편집
- 원본 신호 상태에 연동되는 본 위치·맞은편 표시
- 반경 60m 안의 커서 수동 복제 배치, 자동·수동 복제 신호등 ±90도 회전
- 본 위치 및 맞은편의 앞뒤·좌우 조절, 맞은편 준비 후 본 위치 숨김
- 한국어·영어 개별 편집 UI 및 [커뮤니티 번역 안내](package/lang/README.txt)

차량 통행 규칙이나 실제 신호 주기를 변경하는 모드는 아닙니다. 필수 에셋 팩을 활성화하고 로컬본과 구독본을 중복 활성화하지 마세요. TTE 또는 TLM을 사용하는 경우 둘 중 하나를 선택하는 것을 권장합니다.

## 기본 사용법

1. 도로 메뉴에서 한국식 신호등 도구를 선택합니다.
2. 교차로 좌클릭으로 적용하고 우클릭으로 기본 신호등을 복원합니다.
3. 개별 편집 도구로 신호등을 선택해 에셋·표지판·배치를 조절합니다.
4. 커서 배치는 개별 편집의 배치 항목에서 시작하고, 좌클릭으로 확정하거나 우클릭으로 취소합니다.
5. 편집 후 도시를 저장합니다.

## 2.3.40 성능 및 저장 변경

게임의 자동 숨김 해제와 TLIK의 재숨김이 매 프레임 반복되던 경로를 수정했습니다. 새 표시 객체는 프레임당 최대 4개로 나누어 생성하며, 반복적인 저장 키 계산·렌더 쓰기·표지판 재검사 비용도 줄였습니다.

두 차례의 복원 검색 후에도 찾지 못한 교차로 식별자는 JSON의 `dormantNodes`에 보관합니다. **복원 검사가 끝난 뒤 도시를 저장하면 다음 로드에서 이 항목들을 자동 검색하지 않습니다.** 개별 신호등 설정은 보존하며, 필요한 교차로는 도구로 다시 적용할 수 있습니다. 검색 완료 전 저장이나 빈 도시는 미확인 항목을 성급하게 제외하지 않습니다.

도시 세이브와 아래 폴더의 JSON을 함께 백업하세요.

```text
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\TrafficLightIntoKorea\IndividualSignals
```

## 검증 범위

게임 1.6.2f1 SDK 빌드와 자동 회귀 검사를 통과했습니다. 숨김 보호 19개, 복원 스캐너 24개, 비활성 보관·저장 8개를 비롯해 배치·미리보기·신호 상태·UI 검사를 포함합니다. 테스트 하네스의 작업량 감소는 실제 게임 FPS 개선율을 뜻하지 않습니다. 이번 버전의 인게임 성능과 모든 모드 조합은 아직 검증하지 않았습니다.

## English

This repository contains the source, UI resources and regression tests for **Traffic Light Into Korea 2.3.40**, published on [Paradox Mods](https://mods.paradoxplaza.com/mods/159262/Windows). The [asset pack](https://mods.paradoxplaza.com/mods/159257/Windows) is required.

Apply Korean visuals per junction or city-wide, edit individual signals and signs, and place linked duplicates with a cursor inside a 60m radius. Duplicate rotation supports ±90 degrees. The mod follows native signal states and does not change traffic permissions or signal timing.

Version 2.3.40 prevents repeated native unhide/TLIK re-hide processing, spreads new signal creation over frames, and parks unresolved junction identifiers in `dormantNodes`. Save after restoration checks finish to exclude these identifiers from automatic searches on the next load. Individual signal settings remain stored; junctions can be reapplied manually.

Back up the city and its sidecar JSON together. Do not enable local and subscribed copies simultaneously. SDK builds and automated fixtures passed; in-game FPS gains and all mod combinations remain unverified. See [BUILDING.md](BUILDING.md), [CHANGELOG.md](CHANGELOG.md), and [package/readme.txt](package/readme.txt).
