import sys
from russiannames.parser import NamesParser
parser = NamesParser()
input = sys.argv[1:]
name = " ".join(input)
result = parser.parse(name)
gender = result.get("gender")
print(gender)