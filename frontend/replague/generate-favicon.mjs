import pngToIco from 'png-to-ico';
import { writeFileSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const logos = join(__dirname, 'src/assets/media/logos');

try {
  const buf = await pngToIco([
    join(logos, 'favicon-16x16.png'),
    join(logos, 'favicon-32x32.png'),
  ]);
  writeFileSync(join(logos, 'favicon.ico'), buf);
  writeFileSync(join(__dirname, 'public/favicon.ico'), buf);
  console.log('✅  favicon.ico generado en logos/ y public/');
} catch (err) {
  console.error('❌  Error:', err.message);
  process.exit(1);
}
