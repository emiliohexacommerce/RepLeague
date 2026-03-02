# Iconos PWA — RepLeague

Genera los PNGs a partir del SVG fuente `../icon.svg` ejecutando:

```bash
# Requisito: instalar sharp-cli globalmente
npm install -g sharp-cli

# Generar todos los tamaños regulares
for size in 72 96 128 144 152 192 384 512; do
  npx sharp -i ../icon.svg -o icon-${size}x${size}.png resize $size $size
done
```

Para el icono **maskable** (con zona segura del 40%), usa la herramienta online:
- Abre https://maskable.app/editor
- Carga `../icon.svg`
- Aplica padding ~20% alrededor
- Exporta como `icon-512x512-maskable.png`

## Tamaños requeridos
| Archivo | Uso |
|---------|-----|
| icon-72x72.png | Android Chrome legacy |
| icon-96x96.png | Shortcuts |
| icon-128x128.png | Chrome Web Store |
| icon-144x144.png | Windows tiles |
| icon-152x152.png | iOS Safari |
| icon-192x192.png | Android add to homescreen |
| icon-384x384.png | Splash screen |
| icon-512x512.png | App store / splash |
| icon-512x512-maskable.png | Android adaptive icon |
