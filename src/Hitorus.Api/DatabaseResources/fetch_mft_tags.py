import re
import requests
import string
import os

cwd = os.getcwd()
with open(os.path.join(cwd, "delimiter.txt"), "r") as delimiter_file:
    DELIMITER = delimiter_file.read()
print("DELIMITER characters:", [ord(c) for c in DELIMITER])
print()

categories = ["males", "females", "tags"]
alphabetsWith123 = list(string.ascii_lowercase)
alphabetsWith123.insert(0, "123")

MALE_SYMBOL = "♂"
FEMALE_SYMBOL = "♀"

dir_paths = []
for i in range(len(categories)):
    # create directories for each category
    dir_name = categories[i][0].upper() + categories[i][1:]
    dir_paths.append(os.path.join(cwd, dir_name))
    if not os.path.exists(dir_paths[i]):
        os.makedirs(dir_paths[i])

# iterate over 123 and alphabets
for letterOr123 in alphabetsWith123:
    outputs_list = [[] for _ in range(len(categories))]
    file_names = [f"{category}-{letterOr123}.txt" for category in categories]
    html = requests.get(f'https://hitomi.la/alltags-{letterOr123}.html').text
    content: str = re.findall(r"""<div class="content">(.+?)</div>""", html)[0]
    tagInfoTuples: list[tuple[str, str]] = re.findall(r"""<a href="[^"]+">(.+?)</a> \((\d+)\)""", content)
    for tagInfoTuple in tagInfoTuples:
        tag_with_symbol, gallery_count = tagInfoTuple
        if tag_with_symbol.endswith(MALE_SYMBOL):
            outputs_list[0].append(tag_with_symbol[:-2] + DELIMITER + gallery_count)
        elif tag_with_symbol.endswith(FEMALE_SYMBOL):
            outputs_list[1].append(tag_with_symbol[:-2] + DELIMITER + gallery_count)
        else:
            outputs_list[2].append(tag_with_symbol + DELIMITER + gallery_count)

    for i in range(len(file_names)):
        print(f"Writing to {file_names[i]}...")
        with open(os.path.join(dir_paths[i], file_names[i]), "w") as file:
            file.write('\n'.join(outputs_list[i]))

print("All Done.")