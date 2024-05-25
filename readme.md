# Stave Bi, bare på Dansk

The purpose of this project is mostly to play with data a bit and try to generate fun letters combinations to play a Danish version of The New York Times word game [Spelling Bee](https://www.nytimes.com/puzzles/spelling-bee).

## Setup

```sh
dotnet tool restore
```

This will install the EntityFramework command line tool into the project

The tool will allow you to create migration scripts using this command

```sh
dotnet ef migrations add <MIGRATION_NAME>
```

```
docker run --name=stavebi --rm -v "./dist:/usr/share/nginx/html" -p 5209:80 nginx
```

## Links

[Rules](https://www.nytimes.com/2021/07/26/crosswords/spelling-bee-forum-introduction.html)

[Wordlist](https://ordregister.dk/) (CC0 license) -
[manual](https://ordregister.dk/doc/COR.html)

- [Alternative wordlist](https://korpus.dsl.dk/resources/licences/dsl-open.html) that might be fun to combine with COR.
- [Gigaword - Danish word corpus](https://gigaword.dk/) - May be interesting to count word usage and not use words that are seldom used. (Notice CC-BY 4.0 license requiring acknowledgement)
- [Frequency of lemmas](https://korpus.dsl.dk/resources/details/freq-lemmas.html) - Precomputed list of frequencies of danish words

## TODO

### Need

- [Improvement] Current pointscore in game selector doesn't update when new word is guessed

### Ideas

- Recreate hive so I haven't just stolen it
- While generating valid game consider the base word and require minimum x count of base words (lemma)
- Make frontend mobile friendly
