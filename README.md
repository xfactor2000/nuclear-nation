# Nuclear Nation (prototype)

An abandoned experiment in learning game development — engines, frameworks and
build tooling, rather than a finished game. A turn-based post-apocalyptic
strategy prototype exploring settlement generation: roads, house placement and
population growth on a tile map.

The code went through several stacks as I explored different engines:

- **libGDX** with Gradle (the earliest iteration, kept separately in
  [nuclear-nation-libgdx](https://github.com/xfactor2000/nuclear-nation-libgdx))
- **Godot**, first in C#, then ported to **F#**
- finally **Scala 2.13** on libGDX, built with [mill](https://mill-build.org/) — the current state of `master`

Other experiments live on branches: `first_prototype`, `moving_to_libgdx`,
`rust-trial`.

**Status:** abandoned; kept for reference. Code quality reflects rapid
prototyping and mid-experiment snapshots.

## Building (as it was left, ~2021)

```bash
./mill desktop.run
```

Requires a JDK 8–11 (libGDX 1.9.x on LWJGL2 is not friendly to newer JDKs).

## Credits

- [Commodore 64 UI skin](https://ray3k.wordpress.com/commodore64-ui-skin-for-libgdx/)
  by Raymond "Raeleus" Buckley — [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/)
- Font ["Lunchtime Doubly So"](https://zone38.net/font/) by codeman38

## License

Code is released under the [MIT License](LICENSE).
