#!/bin/bash

# Build script for RepLeague
# Separa backend .NET y frontend Angular

set -e

echo "=========================================="
echo "Building RepLeague Application"
echo "=========================================="

# Build Backend (.NET)
echo ""
echo "Building Backend (.NET 9.0)..."
cd backend/src/RepLeague.API
dotnet publish -c Release -o ../../publish/api
cd ../../..

# Build Frontend (Angular)
echo ""
echo "Building Frontend (Angular 18)..."
cd frontend/replague
npm ci
npm run build -- --configuration production --base-href /
cd ../..

echo ""
echo "=========================================="
echo "Build Complete!"
echo "=========================================="
echo "Backend published to: backend/publish/api"
echo "Frontend built to: frontend/replague/dist"
