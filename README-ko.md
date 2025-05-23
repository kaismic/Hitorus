# Hitorus

<h1 align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="content/banner-dark.jpeg">
    <source media="(prefers-color-scheme: light)" srcset="content/banner-light.png">
    <img alt="Hitorus" src="content/banner-light.png">
  </picture>
</h1>

[![GitHub latest release](https://img.shields.io/github/release/kaismic/Hitorus.svg?logo=github)](https://github.com/kaismic/Hitorus/releases/latest)
[![GitHub downloads count total](https://img.shields.io/github/downloads/kaismic/Hitorus/total.svg?logo=github)](https://github.com/kaismic/Hitorus/releases)

**다른 언어로 읽기:** [English](README.md), [한국어](README-ko.md)

Hitorus는 hitomi.la 웹사이트를 더 사용하기 편하게 만들어주는 웹 브라우저 로컬 데스크탑 앱입니다. 이 앱은 다음과 같은 기능들이 있어요:

- 커스터마이징 가능한 태그 필터로 검색 링크를 생성
- 갤러리 다운로드
- 더 향상된 기능들로 갤러리 감상

... 그리고 더 많은 기능들이 있어요!

## 미리보기
<div align="center">
  <img src="./content/preview-1.jpeg" width="50%">
</div>

## 설치방법
### 윈도우
1. 설치 관리자를 통해서 [ASP.NET Core Runtime 그리고 .NET Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)을 (9.0.5 이상) **둘 다** 설치하세요.
2. Powershell을 관리자 권한으로 실행 후 다음 명령어를 실행하세요:

        Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
3. [최신 버전](https://github.com/kaismic/Hitorus/releases/latest)을 다운로드 한 후 압축해제 하세요.

### macOS/리눅스
1. 설치 관리자를 통해서 [ASP.NET Core Runtime 그리고 .NET Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)을 (9.0.5 이상) **둘 다** 설치하세요. [공식 설치 스크립트](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install)를 이용해서 설치하는 것이 권장됩니다. 설치 스크립트를 다운로드 하고 사용하려면, 다음 명령어를 실행하세요:

          wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
          chmod +x ./dotnet-install.sh
          ./dotnet-install.sh --channel 9.0 --runtime aspnetcore
          ./dotnet-install.sh --channel 9.0 --runtime dotnet

2. 설치장소를 `$PATH`에 추가하세요. 만약 설치 스크립트를 사용하였다면, 다음 줄을 `~/.bashrc` 파일 끝에 추가하는 것으로 할 수 있습니다:

        export PATH=$PATH:/home/{YOUR_USERNAME}/.dotnet
    여기서 `{YOUR_USERNAME}`은 사용자 이름입니다.

3. [최신 버전](https://github.com/kaismic/Hitorus/releases/latest)을 다운로드 한 후 압축해제 하세요.


## 사용법
### 윈도우
- `Hitorus-run.ps1`을 실행하세요.

### macOS/리눅스
- `Hitorus-run.sh`을 실행하세요.

## 문제 해결
- 아주 드문 경우로, 다른 앱들이 스크립트에 있는 기본 포트 번호들을 사용중이라면 오류가 발생할 수 있습니다. 이런 경우엔, 직접 스크립트에 있는 포트 번호들을 다른 번호 (1024 - 65535)로 바꾼 후 실행하세요.

  `Hitorus-run.ps1`

      $WEB_PORT=5214
      $API_PORT=7076
  `Hitorus-run.sh`

      WEB_PORT=5214
      API_PORT=7076

## 참고
- 다운로드 된 갤러리 이미지들은 `Galleries` 폴더에 저장됩니다.
- 앱의 모든 데이터는 `main.db` 파일에 저장되어있습니다 (`Hitorus.Api`에 위치). 데이터를 내보내거나 백업하려면, 이 파일을 복사 후 나중에 똑같은 위치에 붙여넣으면 됩니다.