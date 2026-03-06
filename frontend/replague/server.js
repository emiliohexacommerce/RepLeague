const express = require('express');
const rateLimit = require('express-rate-limit');
const path = require('path');

const PORT = process.env.PORT || 8080;
const DIST_DIR = path.join(__dirname, 'dist');

const app = express();

// Trust Azure's reverse proxy so rate limiter uses real client IP
app.set('trust proxy', 1);

// Security headers
app.use((req, res, next) => {
  res.setHeader('X-Content-Type-Options', 'nosniff');
  res.setHeader('X-Frame-Options', 'SAMEORIGIN');
  res.setHeader('X-XSS-Protection', '1; mode=block');
  next();
});

// Rate limiting — applied only to HTML navigation requests, not static assets
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 300,                  // 300 HTML requests per IP per 15 min
  standardHeaders: true,
  legacyHeaders: false,
});

// Serve static files from dist/ (no rate limit on assets/JS/CSS)
app.use(express.static(DIST_DIR));

// Fallback to index.html for SPA routes (rate limited)
app.get('*', limiter, (req, res) => {
  res.sendFile(path.join(DIST_DIR, 'index.html'));
});

app.listen(PORT, () => {
  console.log(`RepLeague Frontend servidor escuchando en puerto ${PORT}`);
  console.log(`Directorio: ${DIST_DIR}`);
});

// Manejo de errores
process.on('uncaughtException', (err) => {
  console.error('Error no capturado:', err);
});
