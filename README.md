# Hitorus

<h1 align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="content/banner-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="content/banner-light.png">
    <img alt="Hitorus" src="content/banner-light.png">
  </picture>
</h1>

[![GitHub latest release](https://img.shields.io/github/release/kaismic/Hitorus.svg?logo=github)](https://github.com/kaismic/Hitorus/releases/latest)
[![GitHub downloads count total](https://img.shields.io/github/downloads/kaismic/Hitorus/total.svg?logo=github)](https://github.com/kaismic/Hitorus/releases)

Hitorus is a local desktop web browser-based application designed to enhance your experience with the website hitomi.la. It offers the following features:

- Create search links with customizable tag filters
- Download galleries
- View galleries with advanced functionalities

... and many more!

## Preview
(TODO show preview images)

## Prerequisites
Install [ASP.NET Core Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (9.0.5 or higher) which matches  your OS and architecture.

## Usage
### Windows
Run `Hitorus-run.ps1`

### MacOS/Linux
Run `Hitorus-run.sh`

## How to resolve possible issues


## Notes
- Downloaded gallery images are stored in the `Galleries` folder.
- You can import/export tag filters in the search page by using the provided buttons. However, to import/export the entire application data and settings, save the `main.db` file (located under `Hitorus.Api`) and paste it into the same directory later.