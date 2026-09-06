# Brand assets

| File | Size | Use |
|---|---|---|
| `logo.png` | 1024×1024 | Master. README header, GitHub social preview, anything needing scale. |
| `icon.png` | 128×128 | NuGet package icon — packed via `<PackageIcon>` in `src/UniswapSharp/UniswapSharp.csproj`. |

`icon.png` is a Lanczos downscale of `logo.png`; regenerate it if the master changes:

```bash
python3 -c "from PIL import Image; Image.open('assets/logo.png').convert('RGBA').resize((128,128), Image.LANCZOS).save('assets/icon.png')"
```

## The mark

An abstract constant-product curve crossed by two parallel bars — a nod to the `x * y = k`
invariant rather than to any Uniswap brand element.

This is deliberate. UniswapSharp is an independent port and is **not affiliated with Uniswap
Labs**, so the mark avoids Uniswap's pink-and-unicorn identity entirely. It should read as
"a .NET library for Uniswap", never as an official Uniswap product.

Generated with OpenAI GPT Image 2; `logo.json` carries the generation parameters.
