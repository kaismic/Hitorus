import json
import sys
from pathlib import Path

# Usage: generate-resx.py [1-3]
# 1: Hitorus.Api, 2: Hitorus.Web, 3: Both

if len(sys.argv) < 2:
    print("Usage: generate-resx.py [1-3]")
    exit(1)
try:
    option = int(sys.argv[1])
except ValueError:
    print("Usage: generate-resx.py [1-3]")
    exit(1)

projects = []
if option & 0b01:
    projects.append('Hitorus.Api')
if option & 0b10:
    projects.append('Hitorus.Web')

resxTemplateContentFront: str
resxTemplateContentBack: str
with open("Hitorus-Localization/resx-template.resx", "r") as f:
    resxTemplateContent = f.read()
    resxTemplateContentFront = resxTemplateContent[:-8]
    resxTemplateContentBack = resxTemplateContent[-8:]

resxKeyValueTemplate = """  <data name="{key}" xml:space="preserve">
    <value>{value}</value>
  </data>"""

# gotta use divide and conquer algorithm
def generateResx(dirPath: Path, data: dict, lang: str):
    outputKeys: list[str] = []
    for key in data.keys():
        if isinstance(data[key], str):
            outputKeys.append(key)
        else:
            generateResx(Path.joinpath(dirPath, key), data[key], lang)
    if (len(outputKeys) > 0):
        outputContent = '\n'.join([resxKeyValueTemplate.format(key = key,value = data[key]) for key in outputKeys])
        dirPath.parent.mkdir(parents = True, exist_ok = True)
        outputFilePath = Path.joinpath(dirPath.parent, dirPath.stem + (".resx" if lang == "en" else f".{lang}.resx"))
        with open(outputFilePath, "w") as f:
            f.write(resxTemplateContentFront + outputContent + resxTemplateContentBack)

for p in projects:
    inputPath = Path("Hitorus-Localization", "src", p)
    outputPath = Path("src", p, "Localization")
    for filePath in inputPath.iterdir():
        lang = filePath.stem
        with open(filePath) as file:
            data: dict = json.load(file)
            generateResx(outputPath, data, lang)
            print("Generated resource files for", f"'{lang}'", "to", outputPath)