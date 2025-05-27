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

**Read in other languages:** [English](README.md), [한국어](README-ko.md)

Hitorus is a desktop web application that makes using hitomi.la easier.

## Preview
<div align="center">
  <img src="./content/preview-1.jpeg" width="50%">
</div>

## Features
- Create search links with customizable tag filters
- Download galleries
- View galleries with enhanced UI and functionalities

... and many more!

## Usage
1. Download the [latest release](https://github.com/kaismic/Hitorus/releases/latest) and extract it.
2. Run `Hitorus.Api.exe` (Windows) or `Hitorus.Api` (macOS/Linux)
3. Go to https://hitorus.pages.dev/

## How to resolve issues
- If the webpage does not load, try visiting the page in Incognito/Private mode.
- In a rare case, an error might occur when other applications are using the default localhost port. In this case, manually change the Url's port number in `appsettings.json` to a random number between (1024 - 65535).

      "Kestrel": {
        "Endpoints": {
          "Http": {
            "Url": "http://localhost:7076" <-- here
          }
        }
      },
      ...

## Notes
- Downloaded gallery images are stored in the `Galleries` folder.
- The entire application data is stored in the `main.db` file. To export or backup your data, copy, store it somewhere else and later paste it into the same location.