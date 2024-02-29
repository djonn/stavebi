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

## How to run (in docker)

```sh
docker build -t stavebi .
docker run -d --name=stavebi --rm -p 5209:8080 stavebi
```

During development it might be useful to run sqlite in a container, this is done by running `docker compose up -d`

## Links

[Wordlist source][https://ordregister.dk/]
[Wordlist manual](https://ordregister.dk/doc/COR.html)

[Alternative wordlist that migth be fun to combine COR with](https://korpus.dsl.dk/resources/licences/dsl-open.html)


## TODO

### Need

- [BUG] Dockerfile doesn't build frontend after change to ts
- [BUG] Calculation of totalScore is faulty
  * `aijlosu` says it has 58 points, but actually has 114
- [FEATURE] Previous game selector (FE)
- [FEATURE] Only add `/debug` and (maybe) `/admin` endpoints when NOT running in production

### Ideas

- Upload to GitHub
- Admin page for seeing the average word score for a set of letters
  * Can this somehow be used for difficulty level?
- If no db is found on startup: create one, seed with words, generate a couple of games
- Host it somewhere online for people to try it out
- Have toasts float on top of the remaining page, instead of possibly moving the hive
- Recreate hive so I haven't just stolen it
- Auth for admin pages
