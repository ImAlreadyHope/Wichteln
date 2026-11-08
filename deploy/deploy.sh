#!/usr/bin/env bash
# Copies the API source to the VPS and (re)builds + restarts the container there.
# Run from Git Bash / any shell with ssh and tar:
#   deploy/deploy.sh                 # uses $WICHTELN_HOST or root@217.154.17.113
#   deploy/deploy.sh user@host
# The server keeps its own deploy/.env (secrets); this script never overwrites it.
set -euo pipefail

HOST="${1:-${WICHTELN_HOST:-root@217.154.17.113}}"
KEY="${WICHTELN_SSH_KEY:-$HOME/.ssh/momo_ionos}"
DIR=/opt/apps/wichteln
SSH=(ssh -i "$KEY" -o StrictHostKeyChecking=accept-new "$HOST")

cd "$(dirname "$0")/.."

echo "Uploading source to $HOST:$DIR …"
"${SSH[@]}" "mkdir -p $DIR/deploy && rm -rf $DIR/src"
tar czf - \
    --exclude='bin' --exclude='obj' --exclude='*.db*' --exclude='.vs' --exclude='*.user' --exclude='deploy/.env' \
    Wichteln.sln src/Wichteln.Shared src/Wichteln.Api deploy \
  | "${SSH[@]}" "tar xzf - -C $DIR"

echo "Building and starting the container …"
"${SSH[@]}" "cd $DIR/deploy && test -f .env || { echo 'Missing $DIR/deploy/.env (see .env.example)'; exit 1; }; docker compose up -d --build && docker image prune -f >/dev/null && docker compose ps"
