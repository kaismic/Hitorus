import re
import os

html = """
<li><a href="/index-indonesian.html">Bahasa Indonesia</a></li><li><a href="/index-javanese.html">Basa Jawa</a></li><li><a href="/index-catalan.html">Català</a></li><li><a href="/index-cebuano.html">Cebuano</a></li><li><a href="/index-czech.html">Čeština</a></li><li><a href="/index-danish.html">Dansk</a></li><li><a href="/index-german.html">Deutsch</a></li><li><a href="/index-estonian.html">Eesti</a></li><li><a href="/index-english.html">English</a></li><li><a href="/index-spanish.html">Español</a></li><li><a href="/index-esperanto.html">Esperanto</a></li><li><a href="/index-french.html">Français</a></li><li><a href="/index-hindi.html">Hindi</a></li><li><a href="/index-icelandic.html">Íslenska</a></li><li><a href="/index-italian.html">Italiano</a></li><li><a href="/index-latin.html">Latina</a></li><li><a href="/index-hungarian.html">Magyar</a></li><li><a href="/index-dutch.html">Nederlands</a></li><li><a href="/index-norwegian.html">Norsk</a></li><li><a href="/index-polish.html">Polski</a></li><li><a href="/index-portuguese.html">Português</a></li><li><a href="/index-romanian.html">Română</a></li><li><a href="/index-albanian.html">Shqip</a></li><li><a href="/index-slovak.html">Slovenčina</a></li><li><a href="/index-serbian.html">Srpski</a></li><li><a href="/index-finnish.html">Suomi</a></li><li><a href="/index-swedish.html">Svenska</a></li><li><a href="/index-tagalog.html">Tagalog</a></li><li><a href="/index-vietnamese.html">Tiếng Việt</a></li><li><a href="/index-turkish.html">Türkçe</a></li><li><a href="/index-greek.html">Ελληνικά</a></li><li><a href="/index-bulgarian.html">Български</a></li><li><a href="/index-mongolian.html">Монгол</a></li><li><a href="/index-russian.html">Русский</a></li><li><a href="/index-ukrainian.html">Українська</a></li><li><a href="/index-hebrew.html">עברית</a></li><li><a href="/index-arabic.html">العربية</a></li><li><a href="/index-persian.html">فارسی</a></li><li><a href="/index-thai.html">ไทย</a></li><li><a href="/index-burmese.html">မြန်မာဘာသာ</a></li><li><a href="/index-korean.html">한국어</a></li><li><a href="/index-chinese.html">中文</a></li><li><a href="/index-japanese.html">日本語</a></li>
"""

cwd = os.getcwd()
with open(os.path.join(cwd, "delimiter.txt"), "r") as delimiter_file:
    DELIMITER = delimiter_file.read()
print("DELIMITER characters:", [ord(c) for c in DELIMITER])
print()

file_path = os.path.join(cwd, "languages.txt")

languageTuples: list[tuple[str, str]] = re.findall(r"""<li><a href="/index-(.+?).html">(.+?)</a></li>""", html)

print(f"Writing to {file_path}...")
with open(file_path, "w") as file:
    file.write('\n'.join([DELIMITER.join(pair) for pair in languageTuples]))

print("All Done.")