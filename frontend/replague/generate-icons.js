/**
 * Genera todos los iconos PWA para RepLeague a partir de icon.svg
 * Ejecutar: node generate-icons.js
 */
const sharp = require('sharp');
const path = require('path');
const fs = require('fs');

const SRC = path.join(__dirname, 'src/assets/media/logos/icon.svg');
const OUT = path.join(__dirname, 'src/assets/media/logos/icons');
const FAVICON_OUT = path.join(__dirname, 'public/favicon.ico');

const SIZES = [72, 96, 128, 144, 152, 192, 384, 512];

async function generate() {
  if (!fs.existsSync(SRC)) {
    console.error('❌  No se encontró el SVG fuente:', SRC);
    process.exit(1);
  }

  fs.mkdirSync(OUT, { recursive: true });
  console.log('📂  Directorio de salida:', OUT);

  // Iconos regulares
  for (const size of SIZES) {
    const dest = path.join(OUT, `icon-${size}x${size}.png`);
    await sharp(SRC)
      .resize(size, size)
      .png()
      .toFile(dest);
    console.log(`✅  icon-${size}x${size}.png`);
  }

  // Icono maskable (512x512 con padding 20% → safe zone 60%)
  const maskableSize = 512;
  const innerSize = Math.round(maskableSize * 0.6); // 307 px
  const padding = Math.round((maskableSize - innerSize) / 2); // ~102 px

  const maskableDest = path.join(OUT, 'icon-512x512-maskable.png');
  await sharp(SRC)
    .resize(innerSize, innerSize)
    .extend({
      top: padding,
      bottom: maskableSize - innerSize - padding,
      left: padding,
      right: maskableSize - innerSize - padding,
      background: { r: 255, g: 122, b: 26, alpha: 1 } // #FF7A1A
    })
    .png()
    .toFile(maskableDest);
  console.log('✅  icon-512x512-maskable.png (maskable con fondo #FF7A1A)');

  // Apple touch icons adicionales
  const appleSizes = [120, 180];
  for (const size of appleSizes) {
    const dest = path.join(OUT, `apple-touch-icon-${size}x${size}.png`);
    await sharp(SRC)
      .resize(size, size)
      .png()
      .toFile(dest);
    console.log(`✅  apple-touch-icon-${size}x${size}.png`);
  }

  // apple-touch-icon.png estándar (180x180)
  await sharp(SRC)
    .resize(180, 180)
    .png()
    .toFile(path.join(OUT, 'apple-touch-icon.png'));
  console.log('✅  apple-touch-icon.png (180x180)');

  // favicon.png en la carpeta logos (para referencia)
  await sharp(SRC)
    .resize(32, 32)
    .png()
    .toFile(path.join(__dirname, 'src/assets/media/logos/favicon-32x32.png'));
  console.log('✅  favicon-32x32.png');

  await sharp(SRC)
    .resize(16, 16)
    .png()
    .toFile(path.join(__dirname, 'src/assets/media/logos/favicon-16x16.png'));
  console.log('✅  favicon-16x16.png');

  console.log('\n🎉  Todos los iconos generados correctamente.');
  console.log('\n📌  PRÓXIMO PASO:');
  console.log('   Para el favicon.ico (multi-resolución) usa:');
  console.log('   https://realfavicongenerator.net/ cargando icon.svg');
  console.log('   o convierte favicon-32x32.png con: png-to-ico');
}

generate().catch(err => {
  console.error('❌  Error:', err.message);
  process.exit(1);
});
