# Stave Bi, bare på Dansk

The purpose of this project is mostly to play with data a bit and try to generate fun letters combinations to play a Danish version of The New York Times word game [Spelling Bee](https://www.nytimes.com/puzzles/spelling-bee).

## How to run (in docker)

```sh
docker build -t stavebi .
docker run -d --name=stavebi --rm -p 5209:8080 stavebi
```

## Links

[Wordlist source][https://ordregister.dk/]
[Wordlist manual](https://ordregister.dk/doc/COR.html)

[Alternative wordlist that migth be fun to combine COR with](https://korpus.dsl.dk/resources/licences/dsl-open.html)
