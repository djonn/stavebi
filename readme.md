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
docker run --name=stavebi --rm -v "C:\Git\stavebi\dist:/usr/share/nginx/html" -p 5209:80 nginx
```

## Links

[Rules](https://www.nytimes.com/2021/07/26/crosswords/spelling-bee-forum-introduction.html)

[Wordlist](https://ordregister.dk/) -
[manual](https://ordregister.dk/doc/COR.html)

[Alternative wordlist that might be fun to combine COR with](https://korpus.dsl.dk/resources/licences/dsl-open.html)

## TODO

### Need

- [Improvement] Current pointscore in game selector doesn't update when new word is guessed

### Ideas

- Have toasts float on top of the remaining page, instead of possibly moving the hive
- Recreate hive so I haven't just stolen it
- While generating valid game consider the base word and require minimum x count of base words (lemma)
- Make frontend mobile friendly
- Click guessed word to open link in ordnet.dk (`https://ordnet.dk/ddo/ordbog?query=lempe`)
