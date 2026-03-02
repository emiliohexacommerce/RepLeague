#!/bin/bash
# Pre-deployment script for RepLeague
# This script prepares the application for deployment to Azure

set -e

echo "=========================================="
echo "Pre-Deployment Setup for RepLeague"
echo "=========================================="

# Check prerequisites
echo "Checking prerequisites..."

# Check .NET
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 9.0"
    exit 1
fi

# Check Node.js
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js not found. Please install Node.js 18+"
    exit 1
fi

# Check npm
if ! command -v npm &> /dev/null; then
    echo "ERROR: npm not found. Please make sure Node.js is properly installed"
    exit 1
fi

echo "✓ All prerequisites met"

# Restore .NET packages
echo ""
echo "Restoring .NET packages..."
cd backend/src/RepLeague.API
dotnet restore
dotnet add package Microsoft.ApplicationInsights.AspNetCore
cd ../../..

# Install Frontend dependencies
echo ""
echo "Installing Frontend dependencies..."
cd frontend/replague
npm ci
cd ../..

# Create wwwroot directory if it doesn't exist
echo ""
echo "Creating wwwroot directory..."
mkdir -p backend/src/RepLeague.API/wwwroot

echo ""
echo "=========================================="
echo "Pre-Deployment Setup Complete!"
echo "=========================================="
