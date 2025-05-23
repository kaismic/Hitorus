#!/bin/bash

# ------------- NOTE -------------
# This script must be placed in the same directory with Hitorus.Api and Hitorus.Web

WEB_PORT=5214
API_PORT=7076

# Get the directory where the script is located
SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
cd "$SCRIPT_DIR" || exit 1 # Exit if changing directory fails

PYTHON_UPDATER="${SCRIPT_DIR}/appsettings_updater.py"

if ! command -v python3 &> /dev/null; then
    echo "Error: python3 is not installed. Please install Python 3."
    exit 1
fi

# Call Python script to update api and web appsettings
python3 "$PYTHON_UPDATER" "$API_PORT"

# --- Trap for cleanup ---
# Define a cleanup function
cleanup() {
    echo -e "\nCaught signal. Terminating API (PID: $API_PID) and Web App (PID: $WEB_PID)..."
    # Kill the API process if it's running
    if kill -0 "$API_PID" 2>/dev/null; then # -0 sends no signal, just checks if process exists
        kill "$API_PID"
        wait "$API_PID" 2>/dev/null # Wait for it to actually terminate
    fi
    # Kill the Web App process if it's running
    if kill -0 "$WEB_PID" 2>/dev/null; then
        kill "$WEB_PID"
        wait "$WEB_PID" 2>/dev/null
    fi
    echo "Processes terminated."
    exit 0 # Exit the script cleanly
}

# Trap signals: SIGINT (Ctrl+C) and SIGHUP (terminal closed)
trap cleanup SIGINT SIGHUP TERM

# --- Run API ---
echo "Starting Hitorus.Api..."
# Change to the Hitorus.Api directory and run the DLL in the background
(cd "Hitorus.Api" && dotnet Hitorus.Api.dll &)
API_PID=$! # Capture the PID of the background process

# --- Run Web App ---
echo "Starting Hitorus.Web..."
# Calculate paths for dotnet-serve.dll and wwwroot dynamically
SERVE_DLL="${SCRIPT_DIR}/dotnet-serve/dotnet-serve.dll"
WWWROOT="${SCRIPT_DIR}/Hitorus.Web/wwwroot"
(dotnet "$SERVE_DLL" -d "$WWWROOT" -p "$WEB_PORT" -o -q &)
WEB_PID=$! # Capture the PID of the background process

echo "Hitorus.Api (PID: $API_PID) and Hitorus.Web (PID: $WEB_PID) are running. Press Ctrl+C to stop."
wait $API_PID $WEB_PID # Wait for both background processes to finish