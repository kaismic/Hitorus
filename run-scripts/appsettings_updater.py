import json
import sys
import re
from pathlib import Path
from urllib.parse import urlparse

SCRIPT_DIR_PATH = Path(__file__).resolve().parent

if __name__ != '__main__':
    sys.exit(0)
if len(sys.argv) < 2:
    print("Usage: python appsettings_updater.py <api_port>", file=sys.stderr)
    sys.exit(1)

API_PORT = int(sys.argv[1])

apiAppSettingsPath = Path.joinpath(SCRIPT_DIR_PATH, 'Hitorus.Api', 'appsettings.json')
with open(apiAppSettingsPath, "r+", encoding='utf8') as f:
    data = json.load(f)
    apiUrl: str = data['Kestrel']['EndPoints']['Http']['Url']
    parseResult = urlparse(apiUrl)
    if parseResult.port != API_PORT:
        data['Kestrel']['EndPoints']['Http']['Url'] = re.sub(r":\d+'", f":{API_PORT}", apiUrl)
        f.seek(0)
        f.truncate(0)
        json.dump(data, f, indent = 2)

webAppSettingsPath = Path.joinpath(SCRIPT_DIR_PATH, 'Hitorus.Web', 'wwwroot', 'appsettings.json')
with open(webAppSettingsPath, "r+", encoding='utf8') as f:
    data = json.load(f)
    apiUrl: str = data['ApiUrl']
    parseResult = urlparse(apiUrl)
    if parseResult.port != API_PORT:
        data['ApiUrl'] = re.sub(r":\d+'", f":{API_PORT}", apiUrl)
        f.seek(0)
        f.truncate(0)
        json.dump(data, f, indent = 2)