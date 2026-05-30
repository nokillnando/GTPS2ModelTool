# GTPS2ModelTool

*Built from the research described at [Lifting the Bonnet on Gran Turismo's Model Format](https://nenkai.github.io/gt-modding-hub/blog/2023/11/26/lifting-bonnet-on-gt-models/)*

---

A tool that allows creating **custom models** for Gran Turismo 3. Still in early stages and may break. Other games like **GT4** are not yet supported (please contribute!)

> [!CAUTION]
**TLDR: Building custom models is possible. Building models back from original models are not because repacked textures take too much space on the GS Memory. Texture memory layout optimizations are required (see Current Tasks below).**
> * **It is not intended to be used for extracting (aka 'ripping') models out of the game. It is a modding tool.** No support whatsoever will be provided for such purposes.
> * Rebuilding cars from scratch isn't possible without issues. Refer to the tasks below.

For usage documentation, refer to the [Modding Hub](https://nenkai.github.io/gt-modding-hub/ps2/models/). 

![alt text](https://pbs.twimg.com/media/F9h0TbzWMAAUP5b?format=jpg&name=small)

## Current Tasks

I am slowly retiring from GT research, but here are the current tasks left to be done

* **Tex1 Optimizations - HEAVILY NEEDED.** Some paths mentioned [here](https://github.com/Nenkai/PDTools/blob/master/PDTools.Files/Textures/PS2/TextureSet1.cs)
  * The TextureSet format is a thin wrapper over the GS registers so they have full control over which GS areas/blocks textures go.
  * PDI packs textures in a hyper-optimized manner over the obtuse GS memory layout such that no blocks goes unused, or even better, parts of the memory get reused for other textures or palettes.
  * I've (Nenkai) tried to do optimizations by studying the way they do it, but we're still a good 1.5x above average gs texture block size consumption.
  * For this reason, this tool can only be used to make really basic shapes or cars from scratch with less details.
* Attempt to better recover render commands for each model while dumping. This is needed for rebuilding cars correctly (if the above task is somehow figured out).
* Properly support reflective meshes?
* More GT3 callbacks support
* Greater PGLUmaterial support
* Figure out the bounding bit in GT3 models
* GT4 support + VM/Compiler?

---

## Details

There is some code for GT4 (as the codebase essentially works for both games) but still needs heavy work.

Requires **.NET 8.0** to build.

---

## Credits

* [GlitcherOG/SSX-Collection-Multitool](https://github.com/GlitcherOG/SSX-Collection-Multitool) - Useful reference for using SharpTriStrip
* Xenn - Testing

