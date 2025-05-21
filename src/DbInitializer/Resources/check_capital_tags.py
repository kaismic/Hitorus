import string
import os

cwd = os.getcwd()
with open(os.path.join(cwd, "delimiter.txt"), "r") as delimiter_file:
    DELIMITER = delimiter_file.read()
print("DELIMITER characters:", [ord(c) for c in DELIMITER])

categories = ["artists", "groups", "characters", "series", "males", "females", "tags"]
alphabetsWith123 = list(string.ascii_lowercase)
alphabetsWith123.insert(0, "123")

result = []

for category in categories:
    dir_path = os.path.join(cwd, category[0].upper() + category[1:])
    for letterOr123 in alphabetsWith123:
        file_path = os.path.join(dir_path, f"{category}-{letterOr123}.txt")
        with open(file_path, "r") as file:
            content = file.readlines()
            for tag_info in content:
                tag_value, _ = tag_info.split(DELIMITER)
                if tag_value.isupper():
                    result.append(tag_value)


for tag in result:
    print(tag)


print("-----------  All Done ---------------")
