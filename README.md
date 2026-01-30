# The_Asylum
⚠️ 주의 사항: 본 게임은 공포 장르 특성상 갑작스러운 시각적 연출을 포함하고 있습니다.
- [게임 실행] (https://github.com/jun10-dot/The_Asylum/releases/latest)

## 🎈 프로젝트 소개
- 3D 공포 협동 방탈출
- 몬스터AI 추적을 피하며 방마다 퍼즐 요소를 풀고 10개의 토큰을 얻어 폐쇄 병동을 탈출하는 게임입니다. 
   
## 📝프로젝트 정보
- 개발 인원: 4명
- 프로젝트 기간: 2025.10.20~2025.11.21 (5주)
- 담당 역할: 플레이어 시스템, DB 연동, 컬러 퍼즐 시스템

## 🔧 기술 스택
- 엔진 : Unity/C#
- 개발 환경 : Visual Studio 2022
- 네트워크 : Photon PUN
- 백엔드 : MySQL, PHP
- 버전 관리 : GitHub, SourceTree

## 담당 스크립트

<details>
  <summary style="cursor: pointer; font-size: 1.1em; font-weight: bold; margin-bottom: 5px;">📂 DB & UI</summary>
  <ul style="list-style-type: disc; margin-left: 20px;">
    <li>LanguageButton.cs</li>
    <li>LanguageManager.cs</li>
    <li>LobbyManager.cs</li>
    <li>LocalizationText.cs</li>
    <li>LoginManager.cs</li>
    <li>Login.cs</li>
    <li>Server_URL.cs</li>
  </ul>
</details>

<details>
  <summary style="cursor: pointer; font-size: 1.1em; font-weight: bold; margin-bottom: 5px;">📂 Interactions</summary>
  <ul style="list-style-type: disc; margin-left: 20px;">
    <li>Door.cs</li>
    <li>Interfaces.cs</li>
    <li>OutlineEffect.cs</li>
    <li>OutlineItem.cs</li>
  </ul>
</details>

<details>
  <summary style="cursor: pointer; font-size: 1.1em; font-weight: bold; margin-bottom: 5px;">📂 Player</summary>
  <ul style="list-style-type: disc; margin-left: 20px;">
    <li>FollowCamera.cs</li>
    <li>PhotonPlayerCtrl.cs</li>
    <li>PlayerInteraction.cs</li>
    <li>PlayerDeath.cs</li>
  </ul>
</details>

<details>
  <summary style="cursor: pointer; font-size: 1.1em; font-weight: bold; margin-bottom: 5px;">📂 Puzzle</summary>
  <ul style="list-style-type: disc; margin-left: 20px;">
    <li>ColorNode.cs</li>
    <li>Node.cs</li>
    <li>MultiColorNodeManager.cs</li>
    <li>SingleColorNodeManager.cs</li>
  </ul>
</details>
