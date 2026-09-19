2.3.40 업데이트 / Update

- 찾지 못한 교차로는 제한된 복원 검색 후 JSON의 별도 보관 목록으로 옮깁니다. 저장 이후 다음 로드에서 해당 항목을 반복 검색하지 않으며, 개별 신호등 설정은 보존합니다.
- 게임의 자식 객체 숨김 해제와 TLIK의 재숨김이 매 프레임 반복되던 경로를 수정했습니다. 한국식 신호등 신규 생성도 여러 프레임에 나눕니다.
- 반복적인 저장 키 계산, 렌더 상태 쓰기, 표지판 재검사와 맞은편 배치 계산을 줄였습니다.
- 60m 범위의 커서 수동 복제 배치와 복제 신호등 ±90도 회전을 추가했습니다. 수동 미리보기 객체의 생성·검색 등록 순서도 수정했습니다.
- SDK 빌드와 자동 회귀 검사를 통과했습니다. 실제 게임의 FPS 개선율이나 모든 로딩·렌더 멈춤 해소를 보장하지는 않습니다.

- Unmatched junction identifiers move to a dormant JSON list after bounded restore attempts. Once saved, they are excluded from automatic searches on the next load; individual signal settings are preserved.
- Fixed repeated native unhide/TLIK re-hide processing. New Korean signal instances are created over multiple frames.
- Reduced repeated save-key calculations, render-state writes, sign maintenance and opposite-side placement work.
- Added cursor-based duplicate placement within 60m and ±90-degree rotation for duplicate signals. Corrected preview creation/search-registration ordering.
- SDK builds and automated regression checks passed. In-game FPS gains and elimination of all loading/rendering stalls are not guaranteed.
