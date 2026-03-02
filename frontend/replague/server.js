const http = require('http');
const fs = require('fs');
const path = require('path');
const url = require('url');

const PORT = process.env.PORT || 8080;
const DIST_DIR = path.join(__dirname, 'dist');

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.gif': 'image/gif',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.ttf': 'font/ttf',
  '.eot': 'application/vnd.ms-fontobject',
  '.webmanifest': 'application/manifest+json'
};

const server = http.createServer((req, res) => {
  // Agregar headers de seguridad
  res.setHeader('X-Content-Type-Options', 'nosniff');
  res.setHeader('X-Frame-Options', 'SAMEORIGIN');
  res.setHeader('X-XSS-Protection', '1; mode=block');

  let filePath = path.join(DIST_DIR, req.url);

  // Remover query strings y fragments
  filePath = filePath.split('?')[0].split('#')[0];

  // Se asegura de que el path está dentro del directorio DIST
  if (!fs.realpathSync(filePath).startsWith(fs.realpathSync(DIST_DIR))) {
    filePath = path.join(DIST_DIR, 'index.html');
  }

  // Verifica si el archivo existe
  fs.stat(filePath, (err, stats) => {
    if (err || !stats.isFile()) {
      // Si no existe y no es un archivo estático, sirve index.html (SPA routing)
      const ext = path.extname(filePath).toLowerCase();
      if (ext && mimeTypes[ext]) {
        // Es un archivo estático que no existe
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('404 - Not Found');
        return;
      }

      // Para rutas de SPA, sirve index.html
      filePath = path.join(DIST_DIR, 'index.html');
      fs.readFile(filePath, (err, data) => {
        if (err) {
          res.writeHead(500, { 'Content-Type': 'text/plain' });
          res.end('500 - Internal Server Error');
          console.error('Error reading index.html:', err);
          return;
        }
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end(data);
      });
      return;
    }

    // Archivo encontrado, sirve con el tipo MIME correcto
    const ext = path.extname(filePath).toLowerCase();
    const contentType = mimeTypes[ext] || 'application/octet-stream';

    res.writeHead(200, { 'Content-Type': contentType });
    fs.createReadStream(filePath).pipe(res);
  });
});

server.listen(PORT, () => {
  console.log(`RepLeague Frontend servidor escuchando en puerto ${PORT}`);
  console.log(`Directorio: ${DIST_DIR}`);
});

// Manejo de errores
process.on('uncaughtException', (err) => {
  console.error('Error no capturado:', err);
});
