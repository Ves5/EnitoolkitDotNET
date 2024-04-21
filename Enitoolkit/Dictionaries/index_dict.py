import json
from collections import Counter

def create_index(words):
    index = {}
    for word in words:
        sorted_word = ''.join(sorted(word))
        if sorted_word not in index:
            counted_letters = Counter(sorted_word)
            index[sorted_word] = {"letters": counted_letters, "words":[]}
        index[sorted_word]["words"].append(word)
    return index

with open("wwf.txt") as f:
    dictionary = [word.strip() for word in f.readlines()]

with open("wwf_index.json", "w") as i:
    json.dump(create_index(dictionary), i, indent=4)